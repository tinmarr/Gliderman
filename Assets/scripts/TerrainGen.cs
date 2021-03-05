using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    [Range(32,4096)]
    public int width = 256;
    [Range(32, 4096)]
    public int length = 256;

    public float depth = 20;

    public float scale = 200f;

    [Range(0,15)]
    public int octaves = 1;
    [Range(0,1)]
    public float persitance = 0.5f;
    public float lacunarity = 2f;


    public int seed = 1;
    private Vector2[] octaveOffsets;

    public Vector2 scrollValue = new Vector2(0, 0);

    public bool update = false;

    private void Start()
    {
        System.Random prng = new System.Random(seed);
        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float x_offset = prng.Next(-100000, 100000);
            float y_offset = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(x_offset, y_offset);
        }

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    private void Update()
    {
        if (update)
        {
            System.Random prng = new System.Random(seed);
            octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float x_offset = prng.Next(-100000, 100000);
                float y_offset = prng.Next(-100000, 100000);
                octaveOffsets[i] = new Vector2(x_offset, y_offset);
            }

            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
        }
    }

    private void OnValidate()
    {
        if (Mathf.Log(width) % Mathf.Log(2) != 0)
        {
            width = (int) Mathf.Pow(2, Mathf.Floor(Mathf.Log(width) / Mathf.Log(2)));
        }
        if (Mathf.Log(length) % Mathf.Log(2) != 0)
        {
            length = (int)Mathf.Pow(2, Mathf.Floor(Mathf.Log(length) / Mathf.Log(2)));
        }
        if (lacunarity < 1) { lacunarity = 1; }
    }
    TerrainData GenerateTerrain(TerrainData data)
    {
        data.heightmapResolution = Mathf.Max(width, length) + 1;

        data.size = new Vector3(width, depth, length);

        data.SetHeights(0, 0, GenerateHeights());

        return data;
    }

    float[,] GenerateHeights()
    {
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float[,] heights = new float[width, length];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < length; j++)
            {
                float noiseHeight = CalculateHeight(i, j);
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                heights[i, j] = noiseHeight;
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < length; j++)
            {
                heights[i, j] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heights[i, j]);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {

        float noiseHeight = 0;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float x_coord = (x - width / 2) / scale * frequency + octaveOffsets[i].x + scrollValue.x;
            float y_coord = (y - length / 2) / scale * frequency + octaveOffsets[i].y + scrollValue.y;

            float perlinNoiseValue = Mathf.PerlinNoise(x_coord, y_coord) * 2 - 1;
            noiseHeight +=  perlinNoiseValue * amplitude;

            amplitude *= persitance;
            frequency *= lacunarity;
        }
        

        return noiseHeight;
    }
}
