using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour
{
    private DialogueManager _dialogueManager;

    void Start()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();

        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if(_dialogueManager.IsActive) 
            _dialogueManager.DisplayNext();
    }
}
