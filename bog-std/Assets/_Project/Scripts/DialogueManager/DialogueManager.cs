using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    // Injected from Editor
    [SerializeField] private GameObject dialoguePrefab;
    [SerializeField] private float textSpeed = 1f;

    public bool InDialogue => currDialogueBox != null;

    private GameObject currDialogueBox = null;
    private bool readingText;
    private int lineIndex = 0;

    // Temporary script array. Will hold the dialogue received later
    private readonly string[] script =
        {
            "Formatted string containing a pattern and a value representing the <size=120%>text</size> to be displayed.",
            "using UnityEngine;",
            "Note: You may wish to use this function instead of TextMeshPro.text if you need to concatenate a string with values and trying to avoid unnecessary garbage collection."
        };

    void Awake()
    {
        // Unused currently
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
                    // Otherwise try and display the next Dialouge Box

                    // If one is already being display, destroy it 
                    if (currDialogueBox != null)
                        Destroy(currDialogueBox);

                    // If we are not at the end of the script
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
        catch (Exception e)
        {
            // ignored
        }
    }

    public void DisplayTextBox()
    {
        if (dialoguePrefab == null) return;

        try
        {
            // Get a copy of the prefab to instantiate
            var boxToDisplay = dialoguePrefab;
            var str = script[lineIndex++];
            
            // TODO: Calculate position on screen according to who is speaking

            // Get the UI Canvas and instantiate it in the scene 
            var canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            currDialogueBox = Instantiate(boxToDisplay, Vector3.zero, Quaternion.identity, canvas);
            currDialogueBox.transform.position -= new Vector3(0f, 150f, 0f); // Temporary positioning

            // Get the TextMeshPro Component and start the read text coroutine
            var textMesh = currDialogueBox.GetComponentInChildren<TextMeshProUGUI>();
            ReadText(textMesh, str);
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

                // Display 1 more character, wait
                textMesh.maxVisibleCharacters = ++visibleChars;
                yield return new WaitForSeconds(1f / textSpeed);
            }

            // Finished reading text 
            readingText = false;
            // TODO: Create 'FinishedReading' Unity Event
        }
    }
    

    public void RequestDialogue()
    {
        // Maybe how dialogue is read in from a file..?
    }

    private Vector3 GetTextBoxTarget()
    {
        // TODO: Find the location in world space with respect to a target
        return Vector3.zero;
    }

}
