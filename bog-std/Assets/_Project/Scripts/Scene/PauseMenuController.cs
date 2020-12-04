using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{

    private DialogueManager _dialogueManager;
    private TitleMenuController _titleMenu;

    // Start is called before the first frame update
    void Start()
    {
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
        // Return to Menu
    }

    public void QuitToDesktop_Clicked()
    {
        Application.Quit();
    }
}
