using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Settings Config", menuName = "Settings Config")]
public class SettingsConfig : ScriptableObject
{
    [Range(0, 5000)]
    public float renderDistance;
    [Range(0, MeshSettings.numSupportedLODs-1)]
    [Tooltip("A higher value results in a lower resolution. Don't ask why")]
    public int mapQuality = 1;
    public int seed = 1;

    private void OnValidate()
    {
        seed = seed == 0 ? 48 : seed;
    }
}
