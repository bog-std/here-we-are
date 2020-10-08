using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LayeredScene : MonoBehaviour
{
    public List<Layer> layers;
    public AudioSource audio;
    // Start is called before the first frame update
    void Start()
    {
       Invoke("InitializeSprites", 3.0f);

       audio = GetComponent<AudioSource>();
       
       InvokeRepeating("LoopAudio", 0.0f, 10.0f);
    }

    void InitializeSprites()
    {
        foreach (var layer in layers)
            layer.SetCurrentLevel(0);
    }

    void LoopAudio()
    {
        foreach (var layer in layers)
        {
            // layer.currentTrack.
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            layers[0].SetCurrentLevel((layers[0].currentLevel + 1) % 3);
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
            layers[1].SetCurrentLevel((layers[1].currentLevel + 1) % 3);
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
            layers[2].SetCurrentLevel((layers[2].currentLevel + 1) % 3);
    }
    
    
}
