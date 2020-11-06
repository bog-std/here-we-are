using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour
{
    public DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();

        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        dialogueManager.DisplayNext();
    }
}
