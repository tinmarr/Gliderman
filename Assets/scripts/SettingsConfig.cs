using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QualityModes { High, Medium, Low }

[CreateAssetMenu(fileName = "New Settings Config", menuName = "Settings Config")]
public class SettingsConfig : ScriptableObject
{
    [Range(0, 5000)]
    public float renderDistance;
    public QualityModes quality;
    [Range(0, 4)]
    public int mapQuality = 1;
    public bool noobMode = false;
    public int seed = 1;

    private void OnValidate()
    {
        seed = (int)Mathf.Clamp(seed, 1, Mathf.Infinity);
    }
}
