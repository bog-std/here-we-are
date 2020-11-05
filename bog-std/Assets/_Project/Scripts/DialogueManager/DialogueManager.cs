using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Scripts.DialogueData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Scripts.DialogueManager
{
    public class DialogueManager : MonoBehaviour
    {
        // Injected from Editor
        [SerializeField] private GameObject dialoguePrefab;
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private float textSpeed = 1f;
        [SerializeField] private bool useParser = false;

        public bool InDialogue => currDialogueBox != null;

        private GameObject currDialogueBox = null;
        private List<GameObject> currChoices;
        private bool readingText;
        private bool finished;
        private int lineIndex = 0;
        private bool done = false;
        public LayeredScene scene;
        

        // Temporary script array. Will hold the dialogue received later
        private readonly string[] script =
        {
            "Formatted string containing a pattern and a value representing the <size=120%>text</size> to be displayed.",
            "using UnityEngine;",
            "Note: You may wish to use this function instead of TextMeshPro.text if you need to concatenate a string with values and trying to avoid unnecessary garbage collection."
        };

        private Queue<Dialogue> dialogueScript;

        void Awake()
        {
            dialogueScript = new Queue<Dialogue>();
            currChoices = new List<GameObject>();
            
        }

        void Start()
        {
            scene = FindObjectOfType<LayeredScene>();
        }

        void Update()
        {
            try
            {
                if (Input.GetButtonDown("Jump"))
                {
                    if (currChoices.Count > 0)
                    {
   
                    }
                    else
                    {
                        DisplayNext();
                    }
                }
                
                
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void DisplayNext()
        {
            if (readingText)
            {
                readingText = false;
            }
            else
            {
                // Otherwise try and display the next DialougeBox

                // If one is already being display, destroy it 
                if (currDialogueBox != null)
                    Destroy(currDialogueBox);

                // If we are not at the end of the script
                if (useParser)
                {
                    Debug.Log("Count: " + dialogueScript.Count);
                    if (dialogueScript.Count == 0 && !finished)
                    {
                        RequestDialogue();
                    }
                        
                    if(!done)
                        DisplayTextBox_Parser();
                }
                else
                {
                    if (lineIndex < script.Length)
                    {
                        DisplayTextBox();
                    }
                    else
                    {
                        // For testing 
                        lineIndex = 0;
                    }
                }
            }
        }

        public void DisplayTextBox()
        {
            if (dialoguePrefab == null) return;
            Debug.Log("Displaying TextBox");
            try
            {
                // Get a copy of the prefab to instantiate
                var boxToDisplay = dialoguePrefab;
                var str = script[lineIndex++];
            
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
                Debug.Log(currDialogueBox.transform.position.ToString());

                // Get the TextMeshPro Component and start the read text coroutine
                var textMesh = currDialogueBox.GetComponentInChildren<TextMeshProUGUI>();
                ReadText(textMesh, str);
                finished = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void DisplayTextBox_Parser()
        {
            if (dialoguePrefab == null || dialogueScript.Count == 0) return;
            // Debug.Log("Displaying TextBox Parser");
            try
            {
                // Get a copy of the prefab to instantiate
                var boxToDisplay = dialoguePrefab;

                var dialogue = dialogueScript.Dequeue();
                if(dialogueScript.Count == 0) done = true;
                
                if (dialogue.command != Command.None)
                {
                    ProcessCommand(dialogue);
                    return;
                }
            
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
                Debug.Log(currDialogueBox.transform.position.ToString());

                // Get the TextMeshPro Components and start the read text coroutine
                var textMeshes = currDialogueBox.GetComponentsInChildren<TextMeshProUGUI>();
                var mainTextMesh = textMeshes[0];
                var nameTextMesh = textMeshes[1];

                // Set initial values
                mainTextMesh.maxVisibleCharacters = 0;
                mainTextMesh.SetText(dialogue.line);

                CalculateUI();

                nameTextMesh.text = dialogue.name;
                switch (dialogue.name) 
                {
                    case "Jordan":
                        nameTextMesh.color = Color.cyan;
                        break;
                    case "Waiter":
                        nameTextMesh.color = Color.magenta;
                        break;
                    default:
                        nameTextMesh.color = Color.yellow;
                        break;
                }
                
                ReadText(mainTextMesh, dialogue);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        // Same as ReadText, but takes dialogue and prints options after dialogue if there is any.
        public void ReadDialogue(TextMeshProUGUI textMesh, ref Dialogue dialogue)
        {
            if (textMesh == null) return;

            var str = dialogue.line;
            List<Choice> choices = dialogue.choices;
            
            // TODO not sure how to have this wait for ReadText coroutine finishes before displaying choices - could replace following code with these 2 lines
            // ReadText(textMesh, dialogue.line);
            // if (choices.Count > 0) ShowChoices(choices);

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
            
                // Finished reading text 
                readingText = false;
                FinishedReadingText();

                // TODO: Create 'FinishedReading' Unity Event
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

                Debug.Log("BRUH: " + backgroundBox.rect.width + " " + backgroundBox.rect.height);
                rects[0].anchoredPosition = new Vector2((canvas.rect.width / 2) - (backgroundBox.rect.width / 2), (i * (backgroundBox.rect.height + 5) - 70));

                ChoiceScript choiceScript = choiceObject.GetComponentInChildren<ChoiceScript>();
                choiceScript.dialogueManager = this;
                choiceScript.choice = choice;
                
                currChoices.Add(choiceObject);

                ReadText(textMesh, choice.choiceOption);
            }
        }

        private void RequestDialogue() => EnqueueAll(DialogueParser.GetDialogue());

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

            nameBox.anchoredPosition = new Vector2((mainBox.rect.width / -2) + (nameBox.rect.width / 2) + 5, (mainBox.rect.height / 2) + 5);
            chevron.anchoredPosition = new Vector2((mainBox.rect.width / 2) - (chevron.rect.width / 2) - 8, (mainBox.rect.height / -2) + (chevron.rect.height / 2) + 2);
        }

        private void EnqueueAll(IEnumerable<Dialogue> list)
        {
            foreach (var item in list)
            {
                // Debug.Log(item.line);
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
                    UpdateLayers(dialogue.layers, dialogue.magnitude);
                    break;
            }
            
            DisplayNext();
        }
        
        public void ProcessChoice(Choice choice)
        {
            UpdateLayers(choice.layers, choice.magnitude);
            Seek(choice.target);
            ClearChoices();
            DisplayNext();
        }

        private void UpdateLayers(List<LayerName> layers, int magnitude)
        {
            foreach(var layer in layers)
                scene.IncrementLayer(layer, magnitude);
        }

        public void ClearChoices()
        {
            foreach (var option in currChoices)
                Destroy(option);
            
            currChoices.Clear();
        }

        public void Seek(string target)
        {
            if (target != string.Empty && dialogueScript.Count > 0)
            {
                while (dialogueScript.Peek().tag != target)
                {
                    
                    dialogueScript.Dequeue();

                    if (dialogueScript.Count == 0)
                        RequestDialogue();

                }
            }
        }
    }
}
