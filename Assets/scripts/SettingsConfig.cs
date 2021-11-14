using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Settings Config", menuName = "Settings Config")]
public class SettingsConfig : ScriptableObject
{
    [Range(0, 5000)]
    public float renderDistance;
    public int seed = 1;

    private void OnValidate()
    {
        seed = seed == 0 ? 48 : seed;
    }
}
