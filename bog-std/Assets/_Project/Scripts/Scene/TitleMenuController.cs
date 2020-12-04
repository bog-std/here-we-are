﻿using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class TitleMenuController : MonoBehaviour
{
    private DialogueManager _dialogueManager;

    private GameObject _grpMenuButtons;
    private Image _imgTitle;


    // Start is called before the first frame update
    void Start()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();

        _grpMenuButtons = transform.Find("MenuButtons").gameObject;
        _imgTitle = transform.Find("TitleLogo").GetComponent<Image>();
    }

    public void Display()
    {
        gameObject.SetActive(true);
        _dialogueManager.isActive = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _dialogueManager.isActive = true;
    }

    public void Start_Clicked()
    {
        // Fade out title and buttons
        gameObject.SetActive(false);

        // Trigger the script
        _dialogueManager.DisplayNext();
    }

    public void Quit_Clicked() => Application.Quit();
}
