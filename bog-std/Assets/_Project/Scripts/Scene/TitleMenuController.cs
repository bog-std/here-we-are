using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class TitleMenuController : MonoBehaviour
{
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private PhoneHubController _phoneHub;   

    private GameObject _grpMenuButtons;
    private Image _imgTitle;


    // Start is called before the first frame update
    void Start()
    {
        if(_dialogueManager == null)
            _dialogueManager = FindObjectOfType<DialogueManager>();

        if (_phoneHub == null)
            _phoneHub = FindObjectOfType<PhoneHubController>();

        _grpMenuButtons = transform.Find("MenuButtons").gameObject;
        _imgTitle = transform.Find("TitleLogo").GetComponent<Image>();
    }

    public void Display()
    {
        gameObject.SetActive(true);
        _dialogueManager.IsActive = false;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _dialogueManager.IsActive = true;
    }

    public void Start_Clicked()
    {
        _phoneHub.ResetState();

        // Fade out title and buttons
        Hide();

        // Trigger the script
        _dialogueManager.DisplayNext();
    }

    public void SceneSelect_Clicked()
    {
        Hide();

        _phoneHub.UnlockAll();
        _phoneHub.DisplayMemorySelectionScreen();
        _phoneHub.DisplayPhone();
    }

    public void Credits_Clicked()
    {
        Hide();
        _dialogueManager.RollCredits();
    }
    

    public void Quit_Clicked() => Application.Quit();
}
