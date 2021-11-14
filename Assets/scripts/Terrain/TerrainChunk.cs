using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk {

	public GameObject windPrefab;
	public GameObject speedPrefab;

	const float colliderGenerationDistanceThreshold = 5;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	 
	public GameObject gameObject;
	Vector2 sampleCentre;
	Bounds bounds;

    readonly MeshFilter meshFilter;
    readonly MeshCollider meshCollider;

	LODInfo detailLevels;
	readonly LODMesh lodMesh;

	public HeightMap heightMap;
	bool heightMapReceived = false;
	bool hasSetCollider;
	readonly float maxViewDst;

	public readonly HeightMapSettings heightMapSettings;
	readonly MeshSettings meshSettings;
	readonly Transform viewer;

	bool isVisible = false;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo detailLevels, Transform viewer, GameObject gameObject) {
		this.detailLevels = detailLevels;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.viewer = viewer;
		this.gameObject = gameObject;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );

        meshFilter = this.gameObject.GetComponent<MeshFilter>();
        meshCollider = this.gameObject.GetComponent<MeshCollider>();

		gameObject.transform.position = new Vector3(position.x, 0, position.y);

		lodMesh = new LODMesh(detailLevels.lod);
		lodMesh.updateCallback += UpdateTerrainChunk;
		lodMesh.updateCallback += UpdateCollisionMesh;


		maxViewDst = detailLevels.visibleDstThreshold;
	}

	public void Load() {
		ThreadedDataRequester.RequestData(() => HeightMapGenerator.Generate(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
	}

	void OnHeightMapReceived(object heightMapObject) {
		this.heightMap = (HeightMap)heightMapObject;
		heightMapReceived = true;

		//LoadStructures();

		UpdateTerrainChunk();
	}

	Vector2 viewerPosition {
		get {
			return new Vector2 (viewer.position.x, viewer.position.z);
		}
	}


	public void UpdateTerrainChunk() {
		if (heightMapReceived) {
			float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

			bool wasVisible = isVisible;
			isVisible = viewerDstFromNearestEdge <= maxViewDst;

			if (wasVisible != isVisible) {
                onVisibilityChanged?.Invoke(this, isVisible);
            }
		}
	}

	public void UpdateCollisionMesh() {
		if (!hasSetCollider) {
			float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

			if (sqrDstFromViewerToEdge < detailLevels.sqrVisibleDstThreshold) {
				if (!lodMesh.hasRequestedMesh) {
					lodMesh.RequestMesh(heightMap, meshSettings);
				}
			}

			if (lodMesh.hasMesh)
            {
				meshFilter.mesh = lodMesh.mesh;
				if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
				{
						meshCollider.sharedMesh = lodMesh.mesh;
						hasSetCollider = true;
				}
			}
			
		}
	}
}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
    readonly int lod;
	public event System.Action updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh();
		hasMesh = true;

		updateCallback();
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
		hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
	}

}