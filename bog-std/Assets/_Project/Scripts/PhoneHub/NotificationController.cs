using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
    
    public AudioClip NotificationSound;

    private DialogueManager _dialogueManager;
    private PhoneHubController _phoneHub;

    private AudioSource _audioSource;

    public void Awake()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();
        _phoneHub = FindObjectOfType<PhoneHubController>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void DisplayNotification()
    {
        gameObject.SetActive(true);
        _audioSource.PlayOneShot(NotificationSound); 
        _dialogueManager.NotificationOrPhoneOpen = true;    
    }

    public void HideNotification()
    {
        gameObject.SetActive(false);
        _dialogueManager.NotificationOrPhoneOpen = false;
    }

    public void ViewPhone_OnClick()
    {
        _phoneHub.DisplayMemorySelectionScreen();
        
        HideNotification();
        _phoneHub.DisplayPhone();
    }

    public void IgnorePhone_OnClick()
    {
        HideNotification();

        // Continue with the narrative
    }

}
