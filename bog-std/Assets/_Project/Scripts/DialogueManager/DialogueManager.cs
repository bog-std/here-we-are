using System;
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
        [SerializeField] private float textSpeed = 1f;
        [SerializeField] private bool useParser = false;

        public bool InDialogue => currDialogueBox != null;

        private GameObject currDialogueBox = null;
        private bool readingText;
        private bool finished;
        private int lineIndex = 0;
        private bool done = false;

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
        }

        void Update()
        {
            try
            {
                if (Input.GetButtonDown("Jump"))
                {
                    // If text is still being read out, complete it
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
                            if(dialogueScript.Count == 0 && !finished)
                                RequestDialogue();
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
            }
            catch (Exception e)
            {
                Debug.LogError(e);
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

            try
            {
                // Get a copy of the prefab to instantiate
                var boxToDisplay = dialoguePrefab;

                var dialogue = dialogueScript.Dequeue();
                if(dialogueScript.Count == 0) done = true;

                if (dialogue.hasCommand) return;
            
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
                    default:
                        textMesh.color = Color.yellow;
                        break;
                }
                ReadText(textMesh, dialogue.line);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
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
                        var endbrack = str.IndexOf(']', visibleChars);
                        var diff = endbrack - visibleChars - 1;
                        var cmd = str.Substring(visibleChars + 1, diff).Split(' ');
                        if (cmd[0] == "wait")
                        {
                            Debug.Log(cmd[1]);
                            var sec = Convert.ToInt32(cmd[1]);
                            str = str.Remove(visibleChars, endbrack + 1);
                            textMesh.SetText(str);
                            yield return new WaitForSeconds(sec);
                        }
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
                Debug.Log(item.line);
                dialogueScript.Enqueue(item);
            }
        }
        private Queue<T> ConvertListToQueue<T>(IEnumerable<T> list) => new Queue<T>(list);

    }
}
