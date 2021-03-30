using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public SettingsConfig settings;
    public Text renderDistanceText;
    public Slider mapQuality;
    public Slider renderDistance;
    public Toggle reefMode;

    private void Start()
    {
        renderDistance.value = settings.renderDistance;
        renderDistanceText.text = $"{settings.renderDistance}";
        mapQuality.value = settings.mapQuality;
        reefMode.isOn = settings.noobMode;
    }

    public void UpdateMapQuality(float value)
    {
        settings.mapQuality = (int)value;
    }

    public void UpdateRenderDistance(float value)
    {
        settings.renderDistance = value;
        renderDistanceText.text = $"{value}";
    }

    public void ToggleReefMode(bool value)
    {
        settings.noobMode = value;
    }
}
