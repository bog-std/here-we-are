using System.Collections;
using System.Collections.Generic;
using Assets._Project.Scripts.DialogueManager;
using FMOD;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

#region Enums

public enum Scene : ushort
{
    Beach,
    Garden,
    Rooftop
}

public enum SceneState : ushort
{
    Unlocked = 0,
    Locked = 1,
    Visited = 2
}

#endregion

public class PhoneHubController : MonoBehaviour
{

    private DialogueManager _dialogueManager;

    #region Scene Fact Variables

    private int currMessageIndex = 0;
    private Scene currFeatured = Scene.Beach;

    private SceneState BeachState = SceneState.Unlocked;
    private SceneState GardenState = SceneState.Locked;
    private SceneState RooftopState = SceneState.Locked;

    [SerializeField] public Sprite[] featuredImages;
    [SerializeField] public string[] featuredText;
    [SerializeField] public Sprite[] beachImages;
    [SerializeField] public Sprite[] gardenImages;
    [SerializeField] public Sprite[] rooftopImages;
    [SerializeField] public List<Sprite> sceneImages;
    [SerializeField] public List<Sprite> messageImages;

    #endregion

    private Animator _animator;

    private Image _imgScreenOverlay;
    private Button _btnHome;

    #region Memory Selection Screen UI
    
    // Memory Selection UI Components
    private GameObject _grpMemorySelection;

    private TextMeshProUGUI _txtFeatured;

    private Image _imgFeatured;
    private Image _imgScene1;
    private Image _imgScene2;
    private Image _imgScene3;
    
    private Button _btnFeatured;
    private Button _btnScene1;
    private Button _btnScene2;
    private Button _btnScene3;

    #endregion

    #region Memory Entrance Screen UI

    // Memory Selection UI Components
    private GameObject _grpMemoryEntrance;

    private Image _imgMemoryDisplay;

    private Button _btnReturnToSelection;
    private Button _btnEnterMemory;

    #endregion

    #region Message Screen UI

    private GameObject _grpMessageScreen;

    private Image _imgMessageScreen;

    private Button _btnAdvanceMessage;

    #endregion

    void Awake()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();
        _animator = GetComponent<Animator>();

        // Set Group References;
        _grpMemorySelection = transform.Find("Memory-Selection").gameObject;
        _grpMemoryEntrance = transform.Find("Memory-Entrance").gameObject;
        _grpMessageScreen = transform.Find("Messages").gameObject;

        // Get TextMeshPro Component References
        var textMeshes = GetComponentsInChildren<TextMeshProUGUI>();
        _txtFeatured = textMeshes[0];

        // Set Image Component References
        var images = GetComponentsInChildren<Image>();
        _imgFeatured = images[1];
        _imgScene1 = images[2];
        _imgScene2 = images[3];
        _imgScene3 = images[4];
        _imgScreenOverlay = images[5];
        _imgMemoryDisplay = images[6];
        _imgMessageScreen = images[9];

        // Set Button Component References
        var buttons = GetComponentsInChildren<Button>();
        _btnHome = buttons[0];
        _btnFeatured = buttons[1];
        _btnScene1 = buttons[2];
        _btnScene2 = buttons[3];
        _btnScene3 = buttons[4];
        _btnReturnToSelection = buttons[5];
        _btnEnterMemory = buttons[6];
        _btnAdvanceMessage = buttons[7];

         // Set up initial state
         _grpMemorySelection.SetActive(false);
         _grpMemoryEntrance.SetActive(false);
         _grpMessageScreen.SetActive(true);

         UpdateDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) { DisplayPhone(); } 
        else if (Input.GetKeyDown(KeyCode.H)) { HidePhone(); }
        else if (Input.GetKeyDown(KeyCode.F)) { DisplayEnterMemoryScene(Scene.Beach); }
        else if (Input.GetKeyDown(KeyCode.G)) { DisplayMemorySelectionScreen(); }
        else if (Input.GetKeyDown(KeyCode.R)) { UpdateDisplay(); }
    }

    #region Updating Phone Display

    public void SetFact(Scene scene, SceneState state)
    {
        switch (scene)
        {
            case Scene.Beach:
                BeachState = state;
                break;
            case Scene.Garden:
                GardenState = state;
                break;
            case Scene.Rooftop:
                RooftopState = state;
                break;
        }

        UpdateDisplay();
    }

    // Update the display according to the facts 
    public void UpdateDisplay()
    {
        // Set featured image
        _imgFeatured.sprite = featuredImages[(int) currFeatured];
        _txtFeatured.text = "Memory from\n" + featuredText[(int) currFeatured];

        // Update the bottom memory images
        _imgScene1.sprite = beachImages[(int) BeachState];
        _btnScene1.enabled = BeachState == SceneState.Unlocked;

        _imgScene2.sprite = gardenImages[(int) GardenState];
        _btnScene2.enabled = GardenState == SceneState.Unlocked;

        _imgScene3.sprite = rooftopImages[(int) RooftopState];
        _btnScene3.enabled = GardenState == SceneState.Unlocked;
        
    }

    public void DisplayPhone()
    {
        _animator.SetBool("IsOpen", true);
        _dialogueManager.NotificationOrPhoneOpen = true;
    }

    public void HidePhone()
    {
        _animator.SetBool("IsOpen", false);
        _dialogueManager.NotificationOrPhoneOpen = false;
    }

    public void DisplayMessages()
    {
        _grpMemoryEntrance.SetActive(false);
        _grpMemorySelection.SetActive(false);
        _grpMessageScreen.SetActive(true);

        currMessageIndex = 0;
        _imgMessageScreen.sprite = messageImages[currMessageIndex];
    }

    public void DisplayMemorySelectionScreen()
    {
        _grpMemoryEntrance.SetActive(false);
        _grpMessageScreen.SetActive(false);
        _grpMemorySelection.SetActive(true);
    }

    public void DisplayEnterMemoryScene(Scene memory)
    {
        _grpMemorySelection.SetActive(false);
        _grpMessageScreen.SetActive(false);
        _grpMemoryEntrance.SetActive(true);

        _imgMemoryDisplay.sprite = sceneImages[(int) memory];
    }

    #endregion

    #region On Click Events

    public void HomeButton_Clicked()
    {
        // Hide phone and continue the dialogue
        Debug.Log("Home Button Clicked!");
        HidePhone();
        
        _dialogueManager.DisplayNext();
    }

    public void FeaturedScene_Clicked()
    {
        Debug.Log("Featured scene clicked!");
        
        // Transition to curr features scene
        DisplayEnterMemoryScene(currFeatured);
    }

    public void Scene1_Clicked()
    {
        Debug.Log("Scene 1 clicked!");

        // Transition to beach scene
        DisplayEnterMemoryScene(Scene.Beach);
    }

    public void Scene2_Clicked()
    {
        Debug.Log("Scene 2 clicked!");

        // Transition to garden scene
        DisplayEnterMemoryScene(Scene.Garden);
    }

    public void Scene3_Clicked()
    {
        Debug.Log("Scene 3 clicked!");

        // Transition to rooftop scene
        DisplayEnterMemoryScene(Scene.Rooftop);
    }

    public void ReturnToSelection_Clicked()
    {
        Debug.Log("Return to Selection clicked!");

        DisplayMemorySelectionScreen();
    }

    public void EnterMemory_Clicked()
    {
        Debug.Log("Enter Memory clicked!");

        switch (currFeatured)
        {
            case Scene.Beach:
                // TODO: Go to beach scene
                break;
            case Scene.Garden:
                // TODO: Go to garden scene
                break;
            case Scene.Rooftop:
                // TODO: Go to rooftop scene
                break;
        }
    }

    public void AdvanceMessage_Clicked()
    {
        Debug.Log("Advance Message Clicked!");
        ++currMessageIndex;
        if (currMessageIndex >= messageImages.Count)
        {
            HidePhone();
            _dialogueManager.DisplayNext();
        }
        else
        {
            // Advance the image 
            _imgMessageScreen.sprite = messageImages[currMessageIndex];
        }
    }

    #endregion

}
