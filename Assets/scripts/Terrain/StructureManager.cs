using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StructureManager
{
    static float windSpawnChance = 1e-1f;

    public static Dictionary<string, Vector2> GenerateStructures(TerrainChunk chunk)
    {
        System.Random rand = new System.Random();
        Dictionary<string, Vector2> structures = new Dictionary<string, Vector2>();

        float[,] hm = chunk.heightMap.values;
        for (int i = 0; i < hm.GetLength(0); i++)
        {
            for (int j = 0; j < hm.GetLength(1); j++)
            {
                Vector2 coords = new Vector2(i > hm.GetLength(0)/2 ? i - hm.GetLength(0) : i, j > hm.GetLength(0) / 2 ? j - hm.GetLength(0) : j);
                if (hm[i,j] == chunk.heightMap.minValue &&  (float)rand.NextDouble() < windSpawnChance && !chunk.flat)
                {
                    structures.Add($"{structures.Count}{nameof(WindArea)}", coords);
                }
            }
        }

        return structures;
    }
}
