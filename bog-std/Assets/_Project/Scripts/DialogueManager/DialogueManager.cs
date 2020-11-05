﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Scripts.DialogueData;
using TMPro;
using UnityEngine;

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

                // Get the TextMeshPro Component and start the read text coroutine
                var textMesh = currDialogueBox.GetComponentInChildren<TextMeshProUGUI>();
                switch (dialogue.name) 
                {
                    case "Jordan":
                        textMesh.color = Color.cyan;
                        break;
                    case "Waiter":
                        textMesh.color = Color.magenta;
                        break;
                    default:
                        textMesh.color = Color.yellow;
                        break;
                }
                // ReadText(textMesh, dialogue.line);
                ReadDialogue(textMesh, ref dialogue);

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
            
                // Finished reading text 
                readingText = false;
            
                if (choices.Count > 0) ShowChoices(choices);
                
                // TODO: Create 'FinishedReading' Unity Event
            }
        }
        
        public void ShowChoices(List<Choice> choices)
        {
            if (choices.Count == 0) return;
            
            int count = choices.Count;
            var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>();
            
            for (int i = 0; i < count; i++)
            {
                Choice choice = choices[i];
                
                GameObject choiceObject = Instantiate(optionPrefab, new Vector3(-173f, i * 50 + 35), Quaternion.identity, canvas);
                RectTransform t = choiceObject.GetComponent<RectTransform>();
                t.anchorMin = new Vector2(1, 0);
                t.anchorMax = new Vector2(1, 0);
                t.anchoredPosition = new Vector2(0, 35 + 50 * i);
                
                ChoiceScript choiceScript = choiceObject.GetComponentInChildren<ChoiceScript>();
                choiceScript.dialogueManager = this;
                choiceScript.choice = choice;
                
                currChoices.Add(choiceObject);

                // Get the TextMeshPro Component and start the read text coroutine
                var textMesh = choiceObject.GetComponentInChildren<TextMeshProUGUI>();
                textMesh.color = Color.red;
     
                ReadText(textMesh, choice.choiceOption);
            }
        }
        
        public void ReadText(TextMeshProUGUI textMesh, string str)
        {
            if (textMesh == null) return;
        
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
        
                // Finished reading text 
                readingText = false;
        
                // TODO: Create 'FinishedReading' Unity Event
            }
        }

        private void RequestDialogue() => EnqueueAll(DialogueParser.GetDialogue());

        private Vector3 GetTextBoxTarget()
        {
            // TODO: Find the location in world space with respect to a target
            return Vector3.zero;
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
