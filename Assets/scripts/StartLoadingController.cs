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
        renderDistance *= 1.5f;
        bool visible = false;
        for (int i = 0; i < checkingPoints.Length; i++)
        {
            visible = visible || Vector3.Distance(checkingPoints[i].position, glider.position) <= renderDistance;
            Vector2 fakePoint = new Vector2(checkingPoints[i].position.x, checkingPoints[i].position.z);
            Vector2 fakeGlider = new Vector2(glider.position.x, glider.position.z);
        }
        startTerrain.SetActive(visible);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SlowUpdate());
    }
}
