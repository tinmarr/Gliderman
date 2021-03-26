using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLoadingController : MonoBehaviour
{
    public Transform glider;
    public TerrainGenerator terrainGen;
    public GameObject startTerrain;

    private void Start()
    {
        StartCoroutine(SlowUpdate());
    }

    public IEnumerator SlowUpdate()
    {
        float renderDistance = terrainGen.detailLevels[terrainGen.detailLevels.Length - 1].visibleDstThreshold;
        if (Vector3.Distance(transform.position, glider.position) > renderDistance)
        {
            startTerrain.SetActive(false);
        } else
        {
            startTerrain.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        StartCoroutine(SlowUpdate());
    }
}
