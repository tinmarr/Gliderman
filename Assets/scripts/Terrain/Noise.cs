using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public static class Noise {
    /* DELETE ME HARD DADDY */
    /* DELETE ME HARD DADDY uwu */
    /* DELETE ME HARD DADDY */
    /* DELETE ME HARD DADDY */
    public enum NormalizeMode {Local, Global};

    public static OpenSimplexNoise noise;

    public static void newSeed(int seed)
    {
        Noise.noise = new OpenSimplexNoise(seed);
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, HeightMapSettings config, Vector2 sampleCenter)
    {
        /* DELETE ME HARD DADDY */
        return null;
    }

}