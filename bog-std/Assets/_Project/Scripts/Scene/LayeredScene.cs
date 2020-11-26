﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UIElements;

public class LayeredScene : MonoBehaviour
{
    // Set in editor 
    public bool animate;
    [FMODUnity.EventRef] public string fmodEvent;
    public GameObject layerPrefab;
    public List<Layer> layers;
    private FMOD.Studio.EventInstance fmodEventInstance;
    private List<GameObject> layerObjects;
    
    
    void Start()
    {
        fmodEventInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        fmodEventInstance.start();
        
        InitializeLayers();
    }
    
    
    void Update()
    {
        UpdateAudio();
        UpdateLayers();
        if (animate) Animate();
        
        // For testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
            IncrementLayer(LayerName.Jordan, 1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            IncrementLayer(LayerName.ExistentialismSelfish, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            IncrementLayer(LayerName.ExistentialismSelfless,1);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            IncrementLayer(LayerName.RelationshipSelfish,1);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            IncrementLayer(LayerName.RelationshipSelfless,1);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            IncrementLayer(LayerName.GriefSelfish,1);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            IncrementLayer(LayerName.GriefSelfless,1);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            IncrementLayer(LayerName.Scene,1);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            IncrementLayer(LayerName.Extra1,1);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            IncrementLayer(LayerName.Extra2,1);
        if (Input.GetKeyDown(KeyCode.Minus))
            IncrementLayer(LayerName.Extra3,1);
        
    }
    
    
    void Animate()
    {
        layerObjects[5].transform.position = new Vector3( 0.025f * Mathf.Sin(Time.frameCount / 50.0f), 0.0f);
        layerObjects[11].transform.position = new Vector3( 0.025f * Mathf.Cos(Time.frameCount / 50.0f), 0.0f);
        layerObjects[3].transform.position = new Vector3( 0.0f, 0.02f + 0.025f * Mathf.Cos(Time.frameCount / 50.0f) );
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
        
        layerObjects = new List<GameObject>();
        
        for (int i = 0; i < layers.Count; i++)
        {
            // Setup buffer renderer (for transitions)
            GameObject bufferObject = Instantiate(layerPrefab, transform);
            bufferObject.name = "buffer-" + i;
            SpriteRenderer bufferRenderer = bufferObject.GetComponent<SpriteRenderer>();
            if (bufferRenderer == null)
            {
                Debug.LogError("Error: layerPrefab must have SpriteRenderer component.");
                return;
            }
            bufferRenderer.sortingOrder = (layers.Count - i) * 2 + 1;;
            bufferRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f); //transparent
            layers[i].spriteBuffer = bufferRenderer;
            
            // Setup primary layer
            GameObject layerObject = Instantiate(layerPrefab, transform);
            layerObject.name = "layer-" + i;
            layerObjects.Add(layerObject);
            SpriteRenderer spriteRenderer = layerObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = (layers.Count - i) * 2;;
            layers[i].spriteRenderer = spriteRenderer;
        }

        ResetLayers();
    }

    // Set all layers to level 0
    public void ResetLayers()
    {
        foreach (var layer in layers)
            layer.SetLevel(0);
    }
    
    // Sets layer to level index
    public void SetLayer(LayerName layerName, int level)
    {
        Debug.Log("Set Layer: " + layerName + " to " + level + ".");
        
        if (layerName == LayerName.None) return;
        
        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
            layer.SetLevel(level);
    }
    
    // Sets layer to level string name
    public void SetLayer(LayerName layerName, string layerTag)
    {
        Debug.Log("Set Layer: " + layerName + " to " + layerTag + ".");
        
        if (layerName == LayerName.None) return;
        
        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
            layer.SetLevel(layerTag);
    }
    
    // Increment layer level by amount
    public void IncrementLayer(LayerName layerName, int amount)
    {
        Debug.Log("Increment Layer: " + layerName + " by " + amount + ".");
        
        if (layerName == LayerName.None) return;

        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
            layer.IncrementLevel(amount);
    }


    // Interpolates audio when level changes
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
    
    // Handle layer fade animation
    private void UpdateLayers()
    {
        foreach (var layer in layers)
        {
            if (layer.buffering)
            {
                layer.bufferState = FuncLib.FInterpConstantTo(layer.bufferState, 1.0f, Time.deltaTime, layer.fadeSpeed);
                layer.spriteBuffer.color = new Color(1.0f, 1.0f, 1.0f, layer.bufferState);
                
                // Always fade out if going to null image
                if (layer.fadeMode == FadeMode.Both || layer.spriteBuffer.sprite == layer.levels[0])
                    layer.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f - layer.bufferState);
                
                if (layer.bufferState == 1.0f)
                {
                    layer.buffering = false;
                    layer.bufferState = 0.0f;
                    layer.spriteRenderer.sprite = layer.spriteBuffer.sprite;
                    layer.spriteBuffer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    layer.spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                }
            }
        }
    }
    
}



[Serializable]
public class Layer
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public SpriteRenderer spriteBuffer;
    public int currentLevel;
    public LayerName name;
    public string audioTrack;
    public float currentAudioLevel;

    // Fade animation
    public FadeMode fadeMode;
    [Min(0.5f)] public float fadeSpeed;
    [HideInInspector] public float bufferState;
    [HideInInspector] public bool buffering = true;

    [Min(1)] public int levelScale = 1;
    public List<Sprite> levels;
    public List<string> levelsNames;
    
    public void SetLevel(int level)
    {
        level %= levels.Count * levelScale;
        currentLevel = level;

        if (fadeMode == FadeMode.None)
        {
            if (spriteRenderer.sprite != levels[level / levelScale])
                spriteRenderer.sprite = levels[level / levelScale];
        }
        else if (spriteBuffer.sprite != levels[level / levelScale])
        {
            spriteBuffer.sprite = levels[level / levelScale];
            buffering = true;
            bufferState = 0.0f;
        }

        Debug.Log("Set level to " + level + ", Image to " + level / levelScale);
    }

    public void SetLevel(string layerTag)
    {
        int index = levelsNames.FindIndex(s => s == layerTag);
        if (index != -1) SetLevel(index);
    }

    public void IncrementLevel(int amount = 1)
    {
        SetLevel(currentLevel + amount);
    }
}



[Serializable]
public enum LayerName
{
    None,
    RelationshipSelfish,
    RelationshipSelfless,
    ExistentialismSelfish,
    ExistentialismSelfless,
    GriefSelfish,
    GriefSelfless,
    Audio,
    Scene,
    Jordan,
    Extra1,
    Extra2,
    Extra3
}


[Serializable]
public enum FadeMode
{
    None,
    NewOnly,
    Both
}