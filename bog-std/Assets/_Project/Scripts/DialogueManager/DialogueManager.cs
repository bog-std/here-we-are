using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _Project.Scripts.DialogueParser;
using _Project.Scripts.Scene;
using Assets._Project.Scripts.DialogueData;
using FMOD;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;



namespace Assets._Project.Scripts.DialogueManager
{
    public class DialogueManager : MonoBehaviour
    {
        // Injected from Editor
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip dialogueSound;
        [SerializeField] private AudioClip carCrashSound;
        [SerializeField] private GameObject dialoguePrefab;
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private float textSpeed = 1f;
        [SerializeField] private bool useParser = false;
        [SerializeField] private List<TextAsset> txtFiles;
        
        [SerializeField] private TextAsset _currentTxt;
        [SerializeField] private Stack<TextStackItem> _txtStack;

        private AudioSource _audioSource;

        private Dictionary<string, SceneState> _facts;

        private GameObject _currDialogueBox = null;
        private List<GameObject> _currChoices;
        private bool _readingText;
        private bool _finished;
        // private int _lineIndex = 0;
        // private bool _done = false;
        public LayeredScene scene;

        private bool _autocompleteToggle = true;

        private Queue<Dialogue> _dialogueScript;
        private int _dialogueDistance = 0;
        private bool _hasStarted = false;
        private bool _isWaiting = false;
        
        public bool ThroughPhone = false;
        public bool IsActive = true;

        private TitleMenuController _titleMenu;
        private PauseMenuController _pauseMenu;
        private PhoneHubController _phoneHub; 
        private NotificationController _notification;

        public bool InDialogue => _currDialogueBox != null;
        
        public void Awake()
        {
            _dialogueScript = new Queue<Dialogue>();
            _txtStack = new Stack<TextStackItem>();
            _currChoices = new List<GameObject>();
            
        }

        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            scene = FindObjectOfType<LayeredScene>();

            _titleMenu = FindObjectOfType<TitleMenuController>();
            _pauseMenu = FindObjectOfType<PauseMenuController>();
            _phoneHub = FindObjectOfType<PhoneHubController>();
            _notification = FindObjectOfType<NotificationController>();

            _titleMenu.Hide();
            _pauseMenu.Hide();
            _notification.HideNotification();

            DisplayNext();
        }

        public void Update()
        {
            try
            {
                if (IsActive && (!_hasStarted && !_isWaiting) && (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)))
                {
                    if (_currChoices.Count == 0)
                    {
                        DisplayNext();
                        _hasStarted = true;
                    }
                }
                // Toggle Fullscreen
                else if (Input.GetKeyDown(KeyCode.F11))
                {
                    Screen.fullScreen = !Screen.fullScreen;
                }
                // Quit
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _pauseMenu.Display();
                }
                // Toggle fast reading 
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    _autocompleteToggle = !_autocompleteToggle;
                }
                
                
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        
        public void DisplayNext()
        {
            Debug.Log("Display next");

            if (_currChoices != null && _currChoices.Count > 0) return;

            _isWaiting = false;

            if (_autocompleteToggle && _readingText)
            {
                _readingText = false;
            }
            else if (!_readingText)
            {
                // Otherwise try and display the next DialogueBox

                // If one is already being display, destroy it 
                if (_currDialogueBox != null)
                    Destroy(_currDialogueBox);

                // Initialize first script in txtFiles
                if (_txtStack.Count == 0) PushDialogue(txtFiles[0]);
                
                // If at end of current script
                if (_dialogueScript.Count == 0)
                {
                    
                    Debug.Log("End of script");
                    
                    if (_txtStack.Count > 1)
                    {
                        // Pop and load previous script
                        PopDialogue();
                    }
                    else if (_txtStack.Count == 1)
                    {
                        OpenPhone();
                        
                        // Placeholder behavior - reload script
                        Debug.Log("RequestingDialogue");
                        RequestDialogue(_txtStack.Peek().textAsset);
                    }
                        
                }
                
                DisplayTextBox();
            }
        }
        
        public void OpenPhone() => _notification.DisplayNotification();

        public void DisplayTextBox()
        {
            if (dialoguePrefab == null || _dialogueScript.Count == 0) return;
            
            try
            {
                var dialogue = DequeueScript();

                if (dialogue.command != Command.None)
                {
                    ProcessCommand(dialogue);
                    return;
                }
                
                // Get a copy of the prefab to instantiate
                var boxToDisplay = dialoguePrefab;
            
                // TODO: Calculate position on screen according to who is speaking

                // Get the UI Canvas and instantiate it in the scene 
                var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>();
                _currDialogueBox = Instantiate(
                    boxToDisplay, 
                    new Vector3(
                        (0f * 10f) / (canvas.rect.width), 
                        (-125f * 10f) / (canvas.rect.height)), 
                    Quaternion.identity, 
                    canvas);

                // Get the TextMeshPro Components and start the read text coroutine
                var textMeshes = _currDialogueBox.GetComponentsInChildren<TextMeshProUGUI>();
                var mainTextMesh = textMeshes[0];
                var nameTextMesh = textMeshes[1];

                // Set initial values
                mainTextMesh.maxVisibleCharacters = 0;
                mainTextMesh.SetText(dialogue.line);

                CalculateUI();

                nameTextMesh.text = dialogue.name;
                nameTextMesh.color = Lookup.Colour(dialogue.name);
                
                ReadText(mainTextMesh, dialogue);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void ReadText(TextMeshProUGUI textMesh, Dialogue dialogue) =>
            ReadText(textMesh, dialogue.line, dialogue);

        public void ReadText(TextMeshProUGUI textMesh, string str, Dialogue dialogue = null)
        {
            if (textMesh == null) return;

            var choices = dialogue == null ? new List<Choice>() : dialogue.choices;

            // Set initial values
            textMesh.maxVisibleCharacters = 0;
            textMesh.SetText(str);
        
            // Start read coroutine
            int visibleChars = 0;
            StartCoroutine(Read());
            IEnumerator Read()
            {
                _readingText = true;
        
                while (visibleChars < str.Length)
                {
                    // If we should stop reading, complete the text
                    if (!_readingText)
                    {
                        var cleaned = RemoveJunk(str);
                        //textMesh.maxVisibleCharacters = str.Length;
                        textMesh.text = cleaned;
                        textMesh.maxVisibleCharacters = cleaned.Length;
                        break;
                    }
        
                    // If a command
                    if (visibleChars > 0 && str[visibleChars] == '[')
                    {
                        var cmdLength = str.IndexOf(']', visibleChars) - visibleChars;
                        var cmd = str.Substring(visibleChars + 1, cmdLength - 1).Split(' ');

                        switch (cmd[0])
                        {
                            case "wait":
                                Debug.Log("Wait " + cmd[1]);
                                var sec = Convert.ToSingle(cmd[1]);
                                yield return new WaitForSeconds(sec);
                                break;
                        }
                        
                        str = str.Remove(visibleChars, cmdLength + 1);
                        textMesh.SetText(str);
                    }

                    if(!_audioSource.isPlaying)
                        _audioSource.PlayOneShot(dialogueSound);
                    
                    // Display 1 more character, wait
                    textMesh.maxVisibleCharacters = ++visibleChars;
                    
                    yield return new WaitForSeconds(1f / textSpeed);
                }

                // Do post-dialogue operations
                _readingText = false;
                if (choices.Count > 0) ShowChoices(choices);

                // Call back
                FinishedReadingText();
            }
        }

        public string RemoveJunk(string str)
        {
            int i = str.IndexOf('[');
            while (i >= 0)
            {
                int j = str.IndexOf(']');
                str = str.Remove(i, j - i + 1);

                i = str.IndexOf('[');
            }

            return str;
        }

        public void FinishedReadingText()
        {
            // Make the chevron visible 
            var chevron = _currDialogueBox.GetComponentsInChildren<Image>();
            chevron[2].enabled = true;
        }

        public void ShowChoices(List<Choice> choices)
        {
            if (choices.Count == 0) return;
            choices.Reverse();    
            
            int count = choices.Count;
            var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>();


            float yChoiceStart = 70.0f;
            if (_currDialogueBox != null)
            {
                var rects = _currDialogueBox.GetComponentsInChildren<RectTransform>();

                Debug.Log(rects.Length);

                yChoiceStart = (-rects[0].localPosition.y) - (rects[1].rect.height / 2) - 30;
                Debug.Log(rects[0].localPosition.y + " " + (rects[1].rect.height / 2));
            }
            for (int i = 0; i < count; i++)
            {
                Choice choice = choices[i];
                
                GameObject choiceObject = Instantiate(optionPrefab, new Vector3((canvas.rect.width / 2), i * 50 + 15), Quaternion.identity, canvas);

                // Get the TextMeshPro Component 
                var textMesh = choiceObject.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.color = Color.white;
                textMesh.text = choice.choiceOption;

                // Adjust the RectTranforms 
                var rects = choiceObject.GetComponentsInChildren<RectTransform>();
                var backgroundBox = rects[1];
                var textBox = rects[2];
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(textBox);
                LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundBox);

                rects[0].anchoredPosition = new Vector2((canvas.rect.width / 2) - (backgroundBox.rect.width / 2), (i * (backgroundBox.rect.height + 5) - yChoiceStart));

                ChoiceScript choiceScript = choiceObject.GetComponentInChildren<ChoiceScript>();
                choiceScript.dialogueManager = this;
                choiceScript.choice = choice;
                
                _currChoices.Add(choiceObject);

                textMesh.text = choice.choiceOption;
            }
        }

        public void RollCredits()
        {
            var dialogue = new Dialogue()
            {
                command = Command.Script,
                name = "credits"
            };
            ProcessCommand(dialogue);
        }

        public void ReturnToMenu()
        {
            // Clear & Reset Scene
            _isWaiting = false;
            _readingText = false;
            StopAllCoroutines();
            scene.ResetLayers();
            Destroy(_currDialogueBox);
            _phoneHub.HidePhone();
            _notification.HideNotification();
            _titleMenu.Hide();
            ClearChoices();

            // Eradicate the script stack
            _txtStack.Clear();
            DisplayNext();
        } 

        private void RequestDialogue(TextAsset script)
        {
            _currentTxt = script;
            EnqueueAll(DialogueParser.GetDialogue(script));
        }
        
        private Vector3 GetTextBoxTarget()
        {
            // TODO: Find the location in world space with respect to a target
            return Vector3.zero;
        }

        private void CalculateUI()
        {
            // Calculate name box's y position
            var rectTransforms = _currDialogueBox.GetComponentsInChildren<RectTransform>();
            var mainBox = rectTransforms[1];
            var mainTextBox = rectTransforms[2];
            var nameBox = rectTransforms[3];
            var nameTextBox = rectTransforms[4];
            var chevron = rectTransforms[5];

            // Calculate height by forcing layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameTextBox);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameBox);
            LayoutRebuilder.ForceRebuildLayoutImmediate(mainTextBox);
            LayoutRebuilder.ForceRebuildLayoutImmediate(mainBox);

            nameBox.anchoredPosition = new Vector2((mainBox.rect.width / -2), (mainBox.rect.height / 2) - 10);
            chevron.anchoredPosition = new Vector2((mainBox.rect.width / 2) - (chevron.rect.width / 2) - 8, (mainBox.rect.height / -2) + (chevron.rect.height / 2) + 2);
        }

        private void EnqueueAll(IEnumerable<Dialogue> list)
        {
            foreach (var item in list)
            {
                //Debug.Log(item.line);
                _dialogueScript.Enqueue(item);
            }
        }
        private Queue<T> ConvertListToQueue<T>(IEnumerable<T> list) => new Queue<T>(list);

        private void ProcessCommand(Dialogue dialogue)
        {
            switch (dialogue.command)
            {
                case Command.Skip:
                    if (dialogue.name == String.Empty)
                        Seek(dialogue.tag);
                    else if (dialogue.name == "phone" && ThroughPhone == ((int) dialogue.magnitude == 1))
                        Seek(dialogue.tag);
                    break;
                
                case Command.Increment:
                    IncrementLayers(dialogue.layers, (int) dialogue.magnitude);
                    break;
                
                case Command.Set:
                    SetLayers(dialogue.layers, dialogue.name);
                    if (dialogue.magnitude > 0) goto case Command.Wait;
                    break;
                
                case Command.Script:
                    if (dialogue.name == "pop") PopDialogue();
                    else PushDialogue(GetScript(dialogue.name), (int) dialogue.magnitude);
                    break;
                
                case Command.Wait:
                    Debug.Log("Wait " + dialogue.magnitude);
                    
                    Invoke("DisplayNext", dialogue.magnitude);
                    _isWaiting = true;
                    return;
                
                case Command.Phone:
                    OpenPhone();
                    return;

                case Command.Fact:
                    if (dialogue.name == "phone")
                        ThroughPhone = (int) dialogue.magnitude > 0;
                    else 
                        _phoneHub.SetFact((Scene) Convert.ToUInt16(dialogue.name), (SceneState) Convert.ToUInt16(dialogue.magnitude));
                    break;

                case Command.Messages:
                    _phoneHub.DisplayMessages();
                    _notification.DisplayMessagesNotification();
                    return;
                
                case Command.Menu:
                    _titleMenu.Display();
                    return;
                
                case Command.Audio:
                    SetAudio(dialogue.name, dialogue.magnitude);
                    break;

                case Command.Notification:
                    _audioSource.PlayOneShot(_notification.NotificationSound);
                    break;

                case Command.CarCrash:
                    _audioSource.PlayOneShot(carCrashSound);
                    break;
                    
            }
            
            DisplayNext();
        }
        
        private Dialogue DequeueScript()
        {
            _dialogueDistance++;
            return _dialogueScript.Dequeue();
        }
        
        private void PopDialogue()
        {
            if (_txtStack.Count == 0)
            {
                Debug.LogError("Tried popping empty stack.");
                return;
            }
            
            _dialogueScript.Clear();
            TextStackItem item = _txtStack.Pop();
            RequestDialogue(_txtStack.Peek().textAsset);
            _dialogueDistance = 0;
            Forward(item.returnOffset);
        }
        
        public void PushDialogue(TextAsset script, int returnOffset = -1)
        {
            _dialogueScript.Clear();
            RequestDialogue(script);
            _txtStack.Push(new TextStackItem(returnOffset >= 0 ? returnOffset : _dialogueDistance, script));
            _dialogueDistance = 0;
        }

        public TextAsset GetScript(string name)
        {
            return txtFiles.Find(a => a.name == name);
        }
        
        public void ProcessChoice(Choice choice)
        {
            IncrementLayers(choice.layers, choice.magnitude);
            Seek(choice.target);
            ClearChoices();
            DisplayNext();
        }
        
        private void IncrementLayers(List<LayerName> layers, int magnitude)
        {
            foreach(var layer in layers)
                scene.IncrementLayer(layer, magnitude);
        }
        
        private void SetLayers(List<LayerName> layers, string layerTag)
        {
            if (Int32.TryParse(layerTag, out var layerIndex))
            {
                // Set by index
                foreach (var layer in layers)
                    scene.SetLayer(layer, layerIndex);
            }
            else
            {
                // Set by tag
                foreach (var layer in layers)
                    scene.SetLayer(layer, layerTag);
            }
        }

        private void SetAudio(string name, float magnitude)
        {
            scene.SetAudio(name, magnitude);
        }
        
        private void ClearChoices()
        {
            foreach (var option in _currChoices)
                Destroy(option);
            
            _currChoices.Clear();
        }
        
        private void Seek(string target, int offset = 0)
        {
            if (target != string.Empty && _dialogueScript.Count > 1)
            {
                while (_dialogueScript.Count > 0 && _dialogueScript.Peek().tag != target)
                    DequeueScript();
                
                Forward(offset);
            }
        }
        
        private void Forward(int offset)
        {
            if (_dialogueScript.Count > 1) while (offset-- > 0)
                DequeueScript();
        }
        
    }
}



[Serializable]
struct TextStackItem
{
    public TextStackItem(int returnOffset, TextAsset textAsset)
    {
        this.returnOffset = returnOffset;
        this.textAsset = textAsset;
    }

    public int returnOffset;
    public TextAsset textAsset;
}
