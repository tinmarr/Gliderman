using UnityEngine;
using System.Collections;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter) {
		float[,] noiseMap = new float[mapWidth, mapHeight];

		float maxHeight = (Mathf.Pow(settings.persistance, settings.octaves) - 1) / (settings.persistance - 1);

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				float noiseHeight = 0;

				for (int i = 0; i < settings.octaves; i++)
				{
					float offx = 10000 * (2 * Mathf.PerlinNoise(sampleCenter.x * i, settings.seed) - 1) + settings.offset.x + sampleCenter.x,
						offy = 10000 * (2 * Mathf.PerlinNoise(settings.seed, sampleCenter.y * i) - 1) - settings.offset.y - sampleCenter.y;
					float dx = Mathf.Pow(settings.lacunarity, i) * (x - (mapWidth / 2f) + offx),
						dy = Mathf.Pow(settings.lacunarity, i) * (y - (mapHeight / 2f) + offy);

					noiseHeight += (2 * Mathf.PerlinNoise(dx / settings.scale, dy / settings.scale) - 1) * Mathf.Pow(settings.persistance, i);
				}

				noiseMap[x, y] = Mathf.Clamp((noiseHeight + 1) / (maxHeight / 0.9f), 0, int.MaxValue);
			}
		}

		return noiseMap;
	}

}

[System.Serializable]
public class NoiseSettings {
	public Noise.NormalizeMode normalizeMode;

	public float scale = 50;

	public int octaves = 6;
	[Range(0,1)]
	public float persistance =.6f;
	public float lacunarity = 2;

	public int seed;
	public Vector2 offset;

	public void ValidateValues() {
		scale = Mathf.Max (scale, 0.01f);
		octaves = Mathf.Max (octaves, 1);
		lacunarity = Mathf.Max (lacunarity, 1);
		persistance = Mathf.Clamp01 (persistance);
		seed = (int)Mathf.Clamp(seed, 1, Mathf.Infinity);
	}
}