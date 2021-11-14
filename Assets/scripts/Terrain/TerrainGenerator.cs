using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour {
	public float loadingDistance = 2000f;
	public int maxLoadedPerFrame = 100;
	Queue<Vector2> chunksToGenerate = new Queue<Vector2>();

	public GameObject windAreaPrefab;
	public GameObject speedAreaPrefab;

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public SettingsConfig settings;

	public LODInfo detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;

	public GameObject defaultTerrainObject;

	public Transform viewer;
	public Material mapMaterial;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
	int chunksVisibleInViewDst;
	int chunkLoadingRange;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	public GliderController player;

    void Start() {
		defaultTerrainObject = new GameObject("Terrain Chunk");
		MeshRenderer meshRenderer = defaultTerrainObject.AddComponent<MeshRenderer>();
		defaultTerrainObject.AddComponent<MeshFilter>();
		this.defaultTerrainObject.AddComponent<MeshCollider>();
		CollisionLogic collisionLogic = defaultTerrainObject.AddComponent<CollisionLogic>();
		meshRenderer.material = mapMaterial;
		collisionLogic.player = player;
		defaultTerrainObject.tag = "Terrain";
		defaultTerrainObject.layer = 3;
		defaultTerrainObject.SetActive(false);

		heightMapSettings.noiseSettings.seed = settings.seed;
		
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(settings.renderDistance / meshWorldSize);
		chunkLoadingRange = Mathf.RoundToInt(loadingDistance / meshWorldSize);
	}

	void Update() {
		detailLevels.visibleDstThreshold = settings.renderDistance;
		detailLevels.lod = settings.mapQuality;

		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);

		Vector2Int oldChunkCoord = new Vector2Int(Mathf.RoundToInt(viewerPositionOld.x / meshWorldSize), Mathf.RoundToInt(viewerPositionOld.y / meshWorldSize));
		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(viewerPosition.x / meshWorldSize), Mathf.RoundToInt(viewerPosition.y / meshWorldSize));

		if (oldChunkCoord != currentChunkCoord)
		{
            UpdateVisibleChunks();
			viewerPositionOld = viewerPosition;
		}
		GenerateChunks();
	}

	void UpdateVisibleChunks() {
		for (int i=0; i<visibleTerrainChunks.Count; i++)
        {
			visibleTerrainChunks[i].UpdateTerrainChunk();
			visibleTerrainChunks[i].UpdateCollisionMesh();
		}

		Vector2Int currentChunkCoord = new Vector2Int(Mathf.RoundToInt(viewerPosition.x / meshWorldSize), Mathf.RoundToInt(viewerPosition.y / meshWorldSize));

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = currentChunkCoord + new Vector2(xOffset, yOffset);
				if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
				} else
                {
					chunksToGenerate.Enqueue(viewedChunkCoord);
                }
			}
		}
	}

	void GenerateChunks()
	{
		for (int loadedChunks = 0; loadedChunks < maxLoadedPerFrame; loadedChunks++)
        {
			if (chunksToGenerate.Count > 0)
            {
				Vector2 viewedChunkCoord = chunksToGenerate.Dequeue();
				if (!terrainChunkDictionary.ContainsKey(viewedChunkCoord))
				{
					TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, viewer, Instantiate(defaultTerrainObject));
					terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
					newChunk.gameObject.transform.parent = transform;
					newChunk.windPrefab = windAreaPrefab;
					newChunk.speedPrefab = speedAreaPrefab;
					newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
					newChunk.Load();
				}
			} else
            {
				return;
            }
		}
	}

	public TerrainChunk GetChunk(Vector2 pos)
    {
		Vector2 chunkPos = pos / meshWorldSize;
		chunkPos = new Vector2(Mathf.RoundToInt(chunkPos.x), Mathf.RoundToInt(chunkPos.y));

		try
        {
			return terrainChunkDictionary[chunkPos];
		} catch
        {
			return null;
        }
		
	}

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
		if (!chunk.gameObject) return;
		chunk.gameObject.SetActive(isVisible);
		if (isVisible) {
			visibleTerrainChunks.Add(chunk);
		} else {
			visibleTerrainChunks.Remove(chunk);
		}
	}

	public void ClearAllTerrain()
    {
		Random.InitState(settings.seed);
		HeightMapGenerator.NewSeed(settings.seed);
		int childs = transform.childCount;

		for (int i = childs - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        heightMapSettings.noiseSettings.seed = settings.seed;
		chunksVisibleInViewDst = Mathf.RoundToInt(settings.renderDistance / meshWorldSize);
		terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
		visibleTerrainChunks = new List<TerrainChunk>();
	}
}


[System.Serializable]
public struct LODInfo {
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float visibleDstThreshold;


	public float sqrVisibleDstThreshold {
		get {
			return visibleDstThreshold * visibleDstThreshold;
		}
	}
}
