using System;
using System.Collections;
using System.Collections.Generic;
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

        InitializeScenes();
    }

    void InitializeScenes()
    {
        GameObject layerObject = Instantiate(layerPrefab, transform);
        layerObject.name = "layer";
            
        layerObjects.Add(layerObject);
            
        SpriteRenderer spriteRenderer = layerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Error: layerPrefab must have SpriteRenderer component.");
            return;
        }
            
        // spriteRenderer.sortingOrder = layers.Count - i;
        // layers[i].spriteRenderer = spriteRenderer;
        // layers[i].SetLevel(0);
    }
    
    
    void Update()
    {
        // For testing
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

        if (animate) Animate();
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
            GameObject layerObject = Instantiate(layerPrefab, transform);
            layerObject.name = "layer" + i;
            
            layerObjects.Add(layerObject);
            
            SpriteRenderer spriteRenderer = layerObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("Error: layerPrefab must have SpriteRenderer component.");
                return;
            }
            
            spriteRenderer.sortingOrder = layers.Count - i;
            layers[i].spriteRenderer = spriteRenderer;
            layers[i].SetLevel(0, 1);
        }
    }
    
    
    public void SetLayer(LayerName layerName, int level)
    {
        Debug.Log("Set Layer");
        if (layerName == LayerName.None) return;
        
        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
            layer.SetLevel(level,1);
    }

    void IncrementLayer(int layer)
    {
        layers[layer].SetLevel(layers[layer].currentLevel + 1, 3);
    }

    
    public void IncrementLayer(LayerName layerName, int amount)
    {
        if (layerName == LayerName.None) return;
        
        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
            layer.SetLevel(layer.currentLevel + amount, 3);
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
}

[Serializable]
public class Layer
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    public int currentLevel;
    public LayerName name;
    public string audioTrack;
    public float currentAudioLevel;
    public List<Sprite> levels;

    public void SetLevel(int level, int factor)
    {
        Debug.Log("SetLevel: " + level + factor);
        level %= levels.Count * factor;

        currentLevel = level; 
        spriteRenderer.sprite = levels[level / factor];
        Debug.Log("Set level to " + level / factor);
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
    Jordan
}