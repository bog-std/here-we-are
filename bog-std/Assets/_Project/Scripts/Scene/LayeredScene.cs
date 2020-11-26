using System;
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
            layers[i].SetLevel(0);
        }
    }
    
    // Sets layer to level index
    public void SetLayer(LayerName layerName, int level)
    {
        Debug.Log("Set Layer");
        if (layerName == LayerName.None) return;
        
        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
            layer.SetLevel(level);
    }
    
    // Sets layer to level string name
    public void SetLayer(LayerName layerName, string layerTag)
    {
        Debug.Log("Set " + layerName + " by String " + layerTag);
        if (layerName == LayerName.None) return;

        foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
        {
            int index = layer.levelsNames.FindIndex(s => s == layerTag);
            if (index != -1) layer.spriteRenderer.sprite = layer.levels[index];
        }
    }

    // For debug, attached to num keys
    void IncrementLayer(int layer)
    {
        layers[layer].IncrementLevel(1);
    }

    
    public void IncrementLayer(LayerName layerName, int amount)
    {
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
}

[Serializable]
public class Layer
{
    [HideInInspector] public SpriteRenderer spriteRenderer;
    public int currentLevel;
    public LayerName name;
    public string audioTrack;
    public float currentAudioLevel;
    [Min(1)] public int levelScale = 1;
    public List<Sprite> levels;
    public List<string> levelsNames;
    public void SetLevel(int level)
    {
        Debug.Log("SetLevel: " + level + " Scale: " + levelScale);
        level %= levels.Count * levelScale;

        currentLevel = level; 
        spriteRenderer.sprite = levels[level / levelScale];
        Debug.Log("Set level to " + level + ", Image to " + level / levelScale);
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