using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneHubController : MonoBehaviour
{
    public bool BeachSceneVisited { get; set; }
    public bool GardenSceneVisited { get; set; }
    public bool RooftopSceneVisited { get; set; }

    public bool BeachSceneUnlocked { get; set; }
    public bool GardenSceneUnlocked { get; set; }
    public bool RooftopSceneUnlocked { get; set; }


    private Animator _animator;


    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            _animator.SetBool("IsOpen", true);
        } else if (Input.GetKeyDown(KeyCode.H))
        {
            _animator.SetBool("IsOpen", false);
        }
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



}
