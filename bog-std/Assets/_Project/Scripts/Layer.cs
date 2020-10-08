using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Layer// : MonoBehaviour
{
    public Layer()
    {
        currentLevel = 0;
    }

    [Serializable]
    public class Level
    {
        public AudioClip track;
        public Sprite image;
    }
    
    public AudioClip currentTrack;
    public SpriteRenderer renderer;

    public List<Level> levels;
    
    public int currentLevel;
    
    public void SetCurrentLevel(int level)
    {
        if (level < levels.Count)
        {
            currentLevel = level;
            renderer.sprite = levels[level].image;
        }
    }
    
}
