using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLoadingController : MonoBehaviour
{
    public Transform glider;
    public TerrainGenerator terrainGen;
    public GameObject startTerrain;

    public Transform[] checkingPoints;

    private void Start()
    {
        StartCoroutine(SlowUpdate());
    }

    public IEnumerator SlowUpdate()
    {
        float renderDistance = terrainGen.detailLevels[terrainGen.detailLevels.Length - 1].visibleDstThreshold;
        bool visible = true;
        for (int i = 0; i < checkingPoints.Length; i++)
        {
            visible |= Vector3.Distance(checkingPoints[i].position, glider.position) <= renderDistance;
        }
        startTerrain.SetActive(visible);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SlowUpdate());
    }
}
