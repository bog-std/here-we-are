using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    // Injected from Editor
    [SerializeField] private GameObject dialoguePrefab;

    public bool InDialogue => currDialogueBox != null;

    private GameObject currDialogueBox = null;
    private int line = 0;
    private readonly string[] script =
        {
            "Formatted string containing a pattern and a value representing the text to be displayed.",
            "using UnityEngine;",
            "Note: You may wish to use this function instead of TextMeshPro.text if you need to concatenate a string with values and trying to avoid unnecessary garbage collection."
        };

    void Awake()
    {
        
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (currDialogueBox != null)
            {
                try
                {
                    Destroy(currDialogueBox);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            if (line < script.Length)
            {
                DisplayTextBox();
            }
            else
            {
                line = 0;
            }

            // DisplayTextBox();
        }
    }

    public void DisplayTextBox()
    {
        if (dialoguePrefab == null) return;

        try
        {
            var boxToDisplay = dialoguePrefab;
            boxToDisplay.GetComponentInChildren<TextMeshProUGUI>().SetText(script[line++]);

            // var target = GetTextBoxTarget();

            var canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
            currDialogueBox = Instantiate(boxToDisplay, canvas);
        }
        catch (Exception e)
        {
            // ignored
            Debug.LogError(e);
        }

        // Start text display coroutine 
    }

    public void RequestDialogue()
    {

    }

    private Vector3 GetTextBoxTarget()
    {

        return Vector3.zero;
    }

}
