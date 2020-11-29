using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
    private DialogueManager _dialogueManager;
    private PhoneHubController _phoneHub;

    public void Awake()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();
        _phoneHub = FindObjectOfType<PhoneHubController>();
    }

    public void DisplayNotification() => gameObject.SetActive(true);
    public void HideNotification() => gameObject.SetActive(false);

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
