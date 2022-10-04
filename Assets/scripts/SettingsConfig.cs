using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RenderTier { Low, Medium, High }

[CreateAssetMenu(fileName = "New Settings Config", menuName = "Settings Config")]
public class SettingsConfig : ScriptableObject
{
    [Header("Graphics")]
    [SerializeField] private bool maxRes = false;
    [SerializeField] private int resWidth = 1920;
    [SerializeField] private int resHeight = 1080;
    public Resolution resolution;
    public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
    public RenderTier renderTier = RenderTier.High;

    [Header("Terrain")]
    public int renderDistance = 500;
    public int seed = 1;
    public bool randomSeed = true;

    private void OnEnable()
    {
        resWidth = resolution.width;
        resHeight = resolution.height;
    }

    private void OnValidate()
    {
        resolution.width = resWidth;
        resolution.height = resHeight;

        if (maxRes)
        {
            resolution = Screen.resolutions[Screen.resolutions.Length - 1];
            resWidth = resolution.width;
            resHeight = resolution.height;
        }

        renderDistance = Mathf.Clamp(renderDistance, 1, int.MaxValue);
        seed = seed == 0 ? 48 : seed;
    }

    //public void UpdateSettings(MapMagicObject magicObject, Transform water)
    //{
    //    //Screen.SetResolution(resolution.width, resolution.height, fullScreenMode);

    //    magicObject.graph.random.Seed = seed;

    //    float tileSize = magicObject.tileSize.x / 10;
    //    magicObject.mainRange = Mathf.FloorToInt((renderDistance / tileSize) / 2);
    //    magicObject.tiles.generateRange = Mathf.FloorToInt(renderDistance / tileSize);

    //    magicObject.StartGenerate();

    //    water.localScale = new Vector3(renderDistance * 2, 1, renderDistance * 2);
    //}
}
