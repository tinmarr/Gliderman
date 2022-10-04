using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsEditor : MonoBehaviour
{
    public SettingsConfig settings;

    [Header("Terrain")]
    public Transform water;

    public void Updater()
    {
        //settings.UpdateSettings(magicObject, water);
    }
    
    // Graphics
    public void SetResolution(int index) // Add UI
    {
        settings.resolution =  Screen.resolutions[Mathf.Clamp(index, 0, Screen.resolutions.Length - 1)];
    }

    public void SetFullscreenMode(int index) // Add UI
    {
        settings.fullScreenMode = (FullScreenMode)index;
    }

    // Terrain Settings
    public void SetSeed(string seed) // Add UI
    {
        settings.seed = seed.GetHashCode();
    }

    public void SetRenderDistance(int distance) // Add UI
    {
        distance = Mathf.Clamp(distance, 0, int.MaxValue);
        settings.renderDistance = distance;
    }
}
