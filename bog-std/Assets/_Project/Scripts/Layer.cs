using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Layer// : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public string audioTrack;
    public int currentLevel;
    public float currentAudioLevel;
    public List<Sprite> levels;

    public Layer()
    {
        currentLevel = 0;
    }

    public void SetRenderer(ref SpriteRenderer spriteRenderer)
    {
        _spriteRenderer = spriteRenderer;
    }
    
    public void SetLevel(int level)
    {
        if (level < levels.Count)
        {
            currentLevel = level;
            _spriteRenderer.sprite = levels[level];
        }
    }
}
