using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{

    [SerializeField] private DialogueManager _dialogueManager;

    private TitleMenuController _titleMenu;

    // Start is called before the first frame update
    void Start()
    {
        if(_dialogueManager == null)
            _dialogueManager = FindObjectOfType<DialogueManager>();
        _titleMenu = FindObjectOfType<TitleMenuController>();
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

    public void Resume_Clicked()
    {
        Hide(); 
    }

    public void ReturnToMenu_Clicked()
    {
        Hide();
        _dialogueManager.ReturnToMenu();
    }

    public void QuitToDesktop_Clicked()
    {
        Application.Quit();
    }
}
