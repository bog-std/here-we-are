using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LayeredScene : MonoBehaviour
{
    // Set in editor 
    [FMODUnity.EventRef] public string fmodEvent;
    public GameObject layerPrefab;
    public List<Layer> layers;
    
    private FMOD.Studio.EventInstance fmodEventInstance;

    
    void Start()
    {
        fmodEventInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        fmodEventInstance.start();
        
        InitializeLayers();
    }
    
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            IncrementLayer(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            IncrementLayer(3);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            IncrementLayer(1);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            IncrementLayer(9);
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
            IncrementLayer(5);
        
        if (Input.GetKeyDown(KeyCode.Alpha6))
            IncrementLayer(8);
        
        if (Input.GetKeyDown(KeyCode.Alpha7))
            IncrementLayer(12);
        
        if (Input.GetKeyDown(KeyCode.Alpha8))
            IncrementLayer(11);

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
            layers[i].spriteRenderer = spriteRenderer;
            layers[i].SetLevel(0);
        }
    }
    
    
    void UpdateLayer(int layer, int level)
    {
        layers[layer].SetLevel(level);
    }

    void IncrementLayer(int layer)
    {
        layers[layer].SetLevel(layers[layer].currentLevel + 1);
    }
    
    
    void UpdateAudio()
    {
        foreach (var layer in layers)
        {
            if (layer.audioTrack == null) continue;
            
            if (layer.currentAudioLevel - layer.currentLevel != 0.0f)
            {
                layer.currentAudioLevel = FuncLib.FInterpConstantTo(layer.currentAudioLevel, layer.currentLevel, Time.deltaTime, 0.5f);
                fmodEventInstance.setParameterByName(layer.audioTrack, layer.currentAudioLevel);
            }
        }
    }
    
    
    [Serializable]
    public class Layer
    {
        public string audioTrack;
        public int currentLevel;
        public float currentAudioLevel;
        public List<Sprite> levels;
        [HideInInspector] public SpriteRenderer spriteRenderer;

        public void SetLevel(int level)
        {
            level %= levels.Count;

            currentLevel = level; 
            spriteRenderer.sprite = levels[level];
        }
    }
}
