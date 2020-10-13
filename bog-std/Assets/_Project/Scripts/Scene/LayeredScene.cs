using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LayeredScene : MonoBehaviour
{

    public GameObject layerPrefab;
    
    [FMODUnity.EventRef] 
    public string fmodEvent;
    private FMOD.Studio.EventInstance _instance;
    
    // Should set Images for each Level of any layers we want manually 
    public List<Layer> layers;
    

    
    // Start is called before the first frame update
    void Start()
    {
        _instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        _instance.start();
        
        InitializeLayers();
    }
    
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UpdateLayer(0, layers[0].currentLevel + 1);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UpdateLayer(1, layers[1].currentLevel + 1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            UpdateLayer(2, layers[2].currentLevel + 1);

        UpdateAudio();
    }
    

    void InitializeLayers()
    {
        if (layers.Count == 0)
            Debug.LogError("No Layers Set in LayeredScene.");

        if (layerPrefab == null)
        {
            Debug.LogError("Layer prefab NOT Set in LayeredScene.");
            return;
        }
        
        // Instantiate layer objects and levels
        for (int i = 0; i < layers.Count; i++)
        {
            GameObject layerObject = Instantiate(layerPrefab, transform);
            layerObject.name = "layer" + i;
            
            SpriteRenderer spriteRenderer = layerObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("Error: layerPrefab must have SpriteRenderer component.");
                return;
            }
            
            spriteRenderer.sortingOrder = layers.Count - i;
            layers[i].SetRenderer(ref spriteRenderer);
            layers[i].SetLevel(0);
        }
    }
    
    
    void UpdateLayer(int layer, int level)
    {
        layers[layer].SetLevel(level % layers[layer].levels.Count);
    }
    
    
    void UpdateAudio()
    {
        foreach (var layer in layers)
        {
            if (layer.audioTrack == null) continue;
            
            if (Math.Abs(layer.currentAudioLevel- layer.currentLevel) > 10e-8)
            {
                layer.currentAudioLevel = FuncLib.FInterpConstantTo(layer.currentAudioLevel, layer.currentLevel, Time.deltaTime, 0.5f);
                _instance.setParameterByName(layer.audioTrack, layer.currentAudioLevel);
            }
        }
    }
    
}
