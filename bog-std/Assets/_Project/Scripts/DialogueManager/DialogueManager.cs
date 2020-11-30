using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        [SerializeField] private GameObject dialoguePrefab;
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private float textSpeed = 1f;
        [SerializeField] private bool useParser = false;
        [SerializeField] private List<TextAsset> txtFiles;
        
        [SerializeField] private TextAsset currentTxt;
        [SerializeField] private Stack<TextStackItem> txtStack;
        
        private GameObject currDialogueBox = null;
        private List<GameObject> currChoices;
        private bool readingText;
        private bool finished;
        private int lineIndex = 0;
        private bool done = false;
        public LayeredScene scene;

        private bool autocompleteToggle = false;

        private Queue<Dialogue> dialogueScript;
        private int dialogueDistance = 0;
        private bool hasStarted = false;
        private bool isWaiting = false;

        public bool NotificationOrPhoneOpen = false;

        private PhoneHubController _phoneHub; // Reference to the scenes Phone
        private NotificationController _notification;


        public bool InDialogue => currDialogueBox != null;
        
        public void Awake()
        {
            dialogueScript = new Queue<Dialogue>();
            txtStack = new Stack<TextStackItem>();
            currChoices = new List<GameObject>();

            DisplayNext();
        }

        public void Start()
        {
            scene = FindObjectOfType<LayeredScene>();

            _phoneHub = FindObjectOfType<PhoneHubController>();
            _notification = FindObjectOfType<NotificationController>();

            _notification.HideNotification();
        }

        public void Update()
        {
            try
            {
                if (!NotificationOrPhoneOpen && !hasStarted && !isWaiting && (Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)))
                {
                    if (currChoices.Count == 0)
                    {
                        DisplayNext();
                        hasStarted = true;
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
                    Application.Quit();
                }
                // Toggle fast reading 
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    autocompleteToggle = !autocompleteToggle;
                }
                
                
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        

        public void DisplayNext()
        {
            if (currChoices != null && currChoices.Count > 0) return;

            isWaiting = false;

            if (autocompleteToggle && readingText)
            {
                readingText = false;
            }
            else if (!readingText)
            {
                // Otherwise try and display the next DialogueBox

                // If one is already being display, destroy it 
                if (currDialogueBox != null)
                    Destroy(currDialogueBox);

                // Initialize first script in txtFiles
                if (txtStack.Count == 0) PushDialogue("", txtFiles[0]);
                
                // If at end of current script
                if (dialogueScript.Count == 0)
                {
                    
                    Debug.Log("End of script");
                    
                    if (txtStack.Count > 1)
                    {
                        // Pop and load previous script
                        PopDialogue();
                    }
                    else if (txtStack.Count == 1)
                    {
                        // TODO Load phone/memory interface
                        
                        // Placeholder behavior - reload script
                        Debug.Log("RequestingDialogue");
                        RequestDialogue(txtStack.Peek().textAsset);
                    }
                        
                }
                
                DisplayTextBox();
            }
        }

       

        public void DisplayTextBox()
        {
            if (dialoguePrefab == null || dialogueScript.Count == 0) return;
            
            try
            {
                var dialogue = dialogueScript.Dequeue();

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
                currDialogueBox = Instantiate(
                    boxToDisplay, 
                    new Vector3(
                        (0f * 10f) / (canvas.rect.width), 
                        (-125f * 10f) / (canvas.rect.height)), 
                    Quaternion.identity, 
                    canvas);

                // Get the TextMeshPro Components and start the read text coroutine
                var textMeshes = currDialogueBox.GetComponentsInChildren<TextMeshProUGUI>();
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
                readingText = true;
        
                while (visibleChars < str.Length)
                {
                    // If we should stop reading, complete the text
                    if (!readingText)
                    {
                        textMesh.maxVisibleCharacters = str.Length;
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
                                var sec = Convert.ToInt32(cmd[1]);
                                yield return new WaitForSeconds(sec);
                                break;
                        }
                        
                        str = str.Remove(visibleChars, cmdLength + 1);
                        textMesh.SetText(str);
                    }
                    
                    // Display 1 more character, wait
                    textMesh.maxVisibleCharacters = ++visibleChars;
                    
                    yield return new WaitForSeconds(1f / textSpeed);
                }

                // Do post-dialogue operations
                readingText = false;
                if (choices.Count > 0) ShowChoices(choices);

                // Call back
                FinishedReadingText();
            }
        }

        public void FinishedReadingText()
        {
            // Make the chevron visible 
            var chevron = currDialogueBox.GetComponentsInChildren<Image>();
            chevron[2].enabled = true;
        }

        public void ShowChoices(List<Choice> choices)
        {
            if (choices.Count == 0) return;
            
            int count = choices.Count;
            var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>();


            float yChoiceStart = 70.0f;
            if (currDialogueBox != null)
            {
                var rects = currDialogueBox.GetComponentsInChildren<RectTransform>();

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
                
                currChoices.Add(choiceObject);

                ReadText(textMesh, choice.choiceOption);
            }
        }
        

        private void RequestDialogue(TextAsset script)
        {
            currentTxt = script;
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
            var rectTransforms = currDialogueBox.GetComponentsInChildren<RectTransform>();
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
                dialogueScript.Enqueue(item);
            }
        }
        private Queue<T> ConvertListToQueue<T>(IEnumerable<T> list) => new Queue<T>(list);

        private void ProcessCommand(Dialogue dialogue)
        {
            switch (dialogue.command)
            {
                case Command.Skip:
                    Seek(dialogue.tag);
                    break;
                
                case Command.Increment:
                    IncrementLayers(dialogue.layers, dialogue.magnitude);
                    break;
                
                case Command.Set:
                    SetLayers(dialogue.layers, dialogue.name);
                    
                    // TODO Fix Wait functionality
                    if (dialogue.name != string.Empty)
                    {
                        Debug.Log("Processing delay: " + dialogue.name);
                        if (Int32.TryParse(dialogue.name,  out var result)) Invoke("DisplayNext", result);
                    }
                    break;
                
                case Command.LoadScript:
                    if (dialogue.name == "pop") PopDialogue();
                    else PushDialogue(dialogue.tag, txtFiles.Find(a => a.name == dialogue.name));
                    break;
                
                case Command.Wait:
                    Debug.Log("Wait " + dialogue.magnitude);
                    
                    Invoke("DisplayNext", dialogue.magnitude);
                    isWaiting = true;
                    return;
            }
            
            DisplayNext();
        }
        
        
        private void PopDialogue()
        {
            if (txtStack.Count == 0)
            {
                Debug.LogError("Tried popping empty stack.");
                return;
            }
            
            dialogueScript.Clear();
            TextStackItem item = txtStack.Pop();
            RequestDialogue(txtStack.Peek().textAsset);
            if (item.returnAddress != String.Empty) Seek(item.returnAddress, 1);
        }
        
        
        private void PushDialogue(string returnAddress, TextAsset script)
        {
            dialogueScript.Clear();
            RequestDialogue(script);
            txtStack.Push(new TextStackItem(returnAddress, script));
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

        public void ClearChoices()
        {
            foreach (var option in currChoices)
                Destroy(option);
            
            currChoices.Clear();
        }

        public void Seek(string target, int offset = 0)
        {
            if (target != string.Empty && dialogueScript.Count > 1)
            {
                while (dialogueScript.Count > 0 && dialogueScript.Peek().tag != target)
                    dialogueScript.Dequeue();
                
                Seek(offset);
            }
        }

        public void Seek(int offset)
        {
            if (dialogueScript.Count > 1) while (offset-- > 0)
                dialogueScript.Dequeue();
        }

        public Dialogue DequeueScript()
        {
            dialogueDistance++;
            return dialogueScript.Dequeue();
        }
    }
}



[Serializable]
struct TextStackItem
{
    public TextStackItem(string returnAddress, TextAsset textAsset)
    {
        this.returnAddress = returnAddress;
        this.textAsset = textAsset;
    }

    public string returnAddress;
    public TextAsset textAsset;
}