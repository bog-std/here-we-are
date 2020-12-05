using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Scene
{
    public class LayeredScene : MonoBehaviour
    {
        // Set in editor 
        [FMODUnity.EventRef]
        public string fmodEvent;
        public List<string> audioTrackNames;
        
        public Text debugText;
        public Text debugHistoryText;
        
        public GameObject layerPrefab;
        public List<Layer> layers;
        
        private EventInstance _fmodEventInstance;
        private List<GameObject> _layerObjects;
        private Dictionary<string, AudioState> _audioStates;
        
        // Debug info
        private Queue<string> _changeHistory;
        private bool _showDebugText;
        
        
        private void Start()
        {
            // Setup audio tracks
            _audioStates = new Dictionary<string, AudioState>();
            foreach (var track in audioTrackNames)
                _audioStates.Add(track, new AudioState());
        
            
            _changeHistory = new Queue<string>();
        
            _fmodEventInstance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
            _fmodEventInstance.start();
        
            InitializeLayers();
        }


        private void Update()
        {
            UpdateAudio();
            UpdateLayers();
            
            // For testing
            if (Input.GetKeyDown(KeyCode.Alpha1)) IncrementLayer(LayerName.Jordan, 1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) IncrementLayer(LayerName.ExistentialismSelfish, 1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) IncrementLayer(LayerName.ExistentialismSelfless,1);
            if (Input.GetKeyDown(KeyCode.Alpha4)) IncrementLayer(LayerName.RelationshipSelfish,1);
            if (Input.GetKeyDown(KeyCode.Alpha5)) IncrementLayer(LayerName.RelationshipSelfless,1);
            if (Input.GetKeyDown(KeyCode.Alpha6)) IncrementLayer(LayerName.GriefSelfish,1);
            if (Input.GetKeyDown(KeyCode.Alpha7)) IncrementLayer(LayerName.GriefSelfless,1);
            if (Input.GetKeyDown(KeyCode.Alpha8)) IncrementLayer(LayerName.Scene,1);
            if (Input.GetKeyDown(KeyCode.Alpha9)) IncrementLayer(LayerName.Extra1,1);
            if (Input.GetKeyDown(KeyCode.Alpha0)) IncrementLayer(LayerName.Extra2,1);
            if (Input.GetKeyDown(KeyCode.Minus)) IncrementLayer(LayerName.Extra3,1);

            if (Input.GetKeyDown(KeyCode.I)) ToggleDebug();
            UpdateDebugInfo();
        }

        private void InitializeLayers()
        {
            if (layers.Count == 0)
                Debug.LogError("No Layers Set in LayeredScene.");

            if (layerPrefab == null)
            {
                Debug.LogError("Layer prefab NOT Set in LayeredScene.");
                return;
            }
        
            _layerObjects = new List<GameObject>();
        
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
                _layerObjects.Add(layerObject);
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
            // Debug.Log("Set Layer: " + layerName + " to " + level + ".");
        
            if (layerName == LayerName.None) return;
        
            foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
                layer.SetLevel(level);
        
            AddHistory(layerName + " = " + level);
        }
    
        // Sets layer to level string name
        public void SetLayer(LayerName layerName, string layerTag)
        {
            // Debug.Log("Set Layer: " + layerName + " to " + layerTag + ".");
        
            if (layerName == LayerName.None) return;
        
            foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
                layer.SetLevel(layerTag);
        
            AddHistory(layerName + " = " + layerTag);
        }
    
        // Increment layer level by amount
        public void IncrementLayer(LayerName layerName, int amount)
        {
            // Debug.Log("Increment Layer: " + layerName + " by " + amount + ".");
        
            if (layerName == LayerName.None) return;

            foreach (Layer layer in layers.FindAll(layer => layer.name == layerName))
                layer.IncrementLevel(amount);
        
            AddHistory(layerName + " + " + amount);
        }

        public void SetAudio(string name, float magnitude)
        {
            if (name == "all") foreach (var track in _audioStates)
            {
                track.Value.target = magnitude;
            }
            else if (_audioStates.ContainsKey(name))
            {
                _audioStates[name].target = magnitude;
            }
            else Debug.LogError("Audio track" + name + "does not exist.");
        }

        void UpdateAudio()
        {
            foreach (var track in _audioStates)
            {
                if (track.Value.current - track.Value.target != 0.0f)
                {
                    track.Value.current = FuncLib.FInterpConstantTo(track.Value.current, track.Value.target, Time.deltaTime, 0.5f);
                    _fmodEventInstance.setParameterByName(track.Key, track.Value.current);
                }
            }
        }
        
        public Dictionary<LayerName, List<string>> GetImageOptions()
        {
            Dictionary<LayerName, List<string>> result = new Dictionary<LayerName, List<string>>();
            foreach (var layer in layers)
            {
                for (int i = 0; i < layer.levels.Count; i++)
                {
                    if (!result.ContainsKey(layer.name)) result.Add(layer.name, new List<string>());
                    result[layer.name].Add(layer.levels[i].name);
                }
            }

            return result;
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
    
        private string GetLayerDebugInfo()
        {
            string result = String.Empty;
            foreach (Layer layer in layers)
            {
                if (layer.name == LayerName.None) continue;
                result += layer.name + "\n  Level: " + layer.currentLevel + ", Scale: " + layer.levelScale + "\n  Image: " + layer.currentLevel / layer.levelScale + " (" + layer.spriteBuffer.sprite.name + ")\n";
            }

            return result;
        }

        private void AddHistory(string history)
        {
            if (_changeHistory.Count == 10) _changeHistory.Dequeue();
            _changeHistory.Enqueue(history);
        }
        
        private void ToggleDebug()
        {
            _showDebugText = !_showDebugText;
            var cam = GameObject.FindWithTag("MainCamera");
            if (_showDebugText)
            {
                cam.GetComponent<Camera>().orthographicSize = 7;
                cam.transform.position += new Vector3(-2.5f, -1.9f, 0);
            }
            else
            {
                debugText.text = String.Empty;
                debugHistoryText.text = String.Empty;
                cam.GetComponent<Camera>().orthographicSize = 5;
                cam.transform.position = new Vector3(0, 0, -1);
            }
        }

        private void UpdateDebugInfo()
        {
            if (_showDebugText)
            {
                debugText.text = GetLayerDebugInfo();

                var historyText = "History: (Most recent at top)\n";
                foreach (var str in _changeHistory.Reverse())
                {
                    historyText += str + "\n";
                }
                debugHistoryText.text = historyText;
            }
        }
    }



    [Serializable]
    public class AudioState
    {
        public AudioState()
        {
            current = 0.0f;
            target = 0.0f;
        }
    
        public float current;
        public float target;
    }


    [Serializable]
    public class Layer
    {
        // Inject from editor
        public LayerName name;
        [Min(1)] public int levelScale = 1;
        public List<Sprite> levels;
    
        // For debug
        public int currentLevel;

        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public SpriteRenderer spriteBuffer;
    
        // Fade animation
        public FadeMode fadeMode;
        [Min(0.5f)] public float fadeSpeed;
        [HideInInspector] public float bufferState;
        [HideInInspector] public bool buffering = true;

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

            //Debug.Log("Set level to " + level + ", Image to " + level / levelScale);
        }

        public void SetLevel(string layerTag)
        {
            int index = levels.FindIndex(level => level.name == layerTag);
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
        Coffee,
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
}