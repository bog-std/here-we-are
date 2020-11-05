using System;
using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueData;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceScript : MonoBehaviour
{
    public Choice choice;

    public DialogueManager dialogueManager;

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        dialogueManager.ProcessChoice(choice);
    }
    
}
