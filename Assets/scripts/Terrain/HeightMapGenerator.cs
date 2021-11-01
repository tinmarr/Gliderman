using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
	public static HeightMap Generate(int width, int height, HeightMapSettings settings, Vector2 sampleCenter, bool flat)
	{
		if (flat) return new HeightMap(new float[width, height], 0, 0);

		float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);
		AnimationCurve heightCurve = new AnimationCurve(settings.heightCurve.keys);

		float max = 0, min = 0;
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				values[i,j] *= heightCurve.Evaluate(values[i,j])*settings.heightMultiplier*settings.noiseSettings.scale;

				if (values[i,j] > max) max = values[i,j];
				if (values[i,j] < min) min = values[i,j];
			}
		}

		return new HeightMap(values, min, max);
	}
}

public struct HeightMap {
	public readonly float[,] values;
	public readonly float minValue;
	public readonly float maxValue;

	public HeightMap (float[,] values, float minValue, float maxValue)
	{
		this.values = values;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}
}

