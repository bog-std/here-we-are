using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
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
            if(currDialogueBox != null)
                Destroy(currDialogueBox);

            if (line < script.Length)
            {
                Debug.Log(line + " < " + script.Length);
                // ++line;
                DisplayTextBox();
            }

            // DisplayTextBox();
        }
    }

    public void DisplayTextBox()
    {

        var boxToDisplay = dialoguePrefab;
        boxToDisplay.GetComponentInChildren<TextMeshProUGUI>().SetText(script[line++]);

        var canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
        currDialogueBox = Instantiate(boxToDisplay, canvas);
    }
}
