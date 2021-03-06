﻿using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;

public class NotificationController : MonoBehaviour
{
    
    public AudioClip NotificationSound;

    [SerializeField] private DialogueManager _dialogueManager;
    private PhoneHubController _phoneHub;

    private AudioSource _audioSource;

    private GameObject _grpNotification;
    private GameObject _grpMessageNotification;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _grpNotification = transform.Find("Default").gameObject;
        _grpMessageNotification = transform.Find("Messages").gameObject;

        _grpNotification.SetActive(true);
        _grpMessageNotification.SetActive(false);
    }

    void Start()
    {
        if(_dialogueManager == null)
            _dialogueManager = FindObjectOfType<DialogueManager>();
        _phoneHub = FindObjectOfType<PhoneHubController>();
    }

    public void DisplayNotification()
    {
        gameObject.SetActive(true);

        _grpNotification.SetActive(true);
        _grpMessageNotification.SetActive(false);

        _audioSource.PlayOneShot(NotificationSound); 
        _dialogueManager.IsActive = false;    
    }

    public void DisplayMessagesNotification()
    {
        gameObject.SetActive(true);

        _grpNotification.SetActive(false);
        _grpMessageNotification.SetActive(true);

        _audioSource.PlayOneShot(NotificationSound); 
        _dialogueManager.IsActive = false;    
    }

    public void HideNotification()
    {
        gameObject.SetActive(false);
        _dialogueManager.IsActive = true;
    }

    public void ViewPhone_OnClick()
    {
        _phoneHub.DisplayMemorySelectionScreen();
        
        HideNotification();
        _phoneHub.DisplayPhone();
    }

    public void ViewPhoneMessages_OnClick()
    {
        _phoneHub.DisplayMessages();
        
        HideNotification();
        _phoneHub.DisplayPhone();
    }

    public void IgnorePhone_OnClick()
    {
        HideNotification();

        // Continue with the narrative
        _dialogueManager.DisplayNext();
    }

}
