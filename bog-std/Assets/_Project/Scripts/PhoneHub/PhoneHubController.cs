using System.Collections;
using System.Collections.Generic;
using FMOD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PhoneHubController : MonoBehaviour
{

    #region Scene Fact Variables

    public bool BeachSceneVisited { get; set; }
    public bool GardenSceneVisited { get; set; }
    public bool RooftopSceneVisited { get; set; }

    public bool BeachSceneUnlocked { get; set; }
    public bool GardenSceneUnlocked { get; set; }
    public bool RooftopSceneUnlocked { get; set; }

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

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();

        // Set Group References;
        _grpMemorySelection = transform.Find("Memory-Selection").gameObject;
        _grpMemoryEntrance = transform.Find("Memory-Entrance").gameObject;

        // Get TextMeshPro Component References
        var textMeshes = GetComponentsInChildren<TextMeshProUGUI>();
        _txtFeatured = textMeshes[0];

        // Set Image Component References
        var images = GetComponentsInChildren<Image>();
        _imgFeatured = images[0];
        _imgScene1 = images[1];
        _imgScene2 = images[2];
        _imgScene3 = images[3];
        _imgScreenOverlay = images[4];
        _imgMemoryDisplay = images[5];

        // Set Button Component References
        var buttons = GetComponentsInChildren<Button>();
        _btnHome = buttons[0];
        _btnFeatured = buttons[1];
        _btnScene1 = buttons[2];
        _btnScene2 = buttons[3];
        _btnScene3 = buttons[4];
        _btnReturnToSelection = buttons[5];
        _btnEnterMemory = buttons[6];

         // Set up initial state
         _grpMemorySelection.SetActive(true);
         _grpMemoryEntrance.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) { DisplayPhone(); } 
        else if (Input.GetKeyDown(KeyCode.H)) { HidePhone(); }
        else if (Input.GetKeyDown(KeyCode.F)) { GoToMemory(0); }
        else if (Input.GetKeyDown(KeyCode.G)) { ReturnToSelection(); }
    }

    #region Updating Phone Display

    public void DisplayPhone() => _animator.SetBool("IsOpen", true);
    public void HidePhone() => _animator.SetBool("IsOpen", false);

    public void GoToMemory(int mem)
    {
        _grpMemorySelection.SetActive(false);
        _grpMemoryEntrance.SetActive(true);
    }

    public void ReturnToSelection()
    {
        _grpMemoryEntrance.SetActive(false);
        _grpMemorySelection.SetActive(true);
    }

    // Update the display according to the facts 
    public void UpdateDisplay()
    {

    }
    
    #endregion

    #region On Click Events

    public void HomeButton_Clicked()
    {
        // Hide phone and continue the dialogue
        HidePhone();
    }

    public void FeaturedScene_Clicked()
    {
        Debug.Log("Featured scene clicked!");
    }

    public void Scene1_Clicked()
    {
        Debug.Log("Scene 1 clicked!");

        if (BeachSceneUnlocked && !BeachSceneVisited)
        {
            // Transition to beach scene
        }
    }

    public void Scene2_Clicked()
    {
        Debug.Log("Scene 2 clicked!");

        if (GardenSceneUnlocked && !GardenSceneVisited)
        {
            // Transition to garden scene
        }
    }

    public void Scene3_Clicked()
    {
        Debug.Log("Scene 3 clicked!");

        if (RooftopSceneUnlocked && !RooftopSceneVisited)
        {
            // Transition to rooftop scene
        }
    }

    public void ReturnToSelection_Clicked()
    {
        Debug.Log("Return to Selection clicked!");
    }

    public void EnterMemory_Clicked()
    {
        Debug.Log("Enter Memory clicked!");
    }

    #endregion

}
