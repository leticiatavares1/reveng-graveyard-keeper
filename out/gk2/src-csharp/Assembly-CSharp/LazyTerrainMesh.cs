using UnityEngine;
using UnityEngine.Rendering;

public class LazyTerrainMesh : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private MeshFilter meshFilter;

	public Vector3 center;

	public Bounds bounds;

	public DeformingGrass deformingGrass;

	public Mesh Mesh => meshFilter.sharedMesh;

	public Vector3 VertexOffset => base.transform.localPosition - center;

	public void DrawFromData(LazyTerrainMeshData data, LazyTerrain lazyTerrain, DeformingGrass grass = null)
	{
		center = data.center;
		bounds = data.bounds;
		if (Application.isPlaying)
		{
			LazyTerrainMeshCollection.Runtime_StripCpuMeshData(data.mesh);
		}
		meshFilter.sharedMesh = data.mesh;
		meshRenderer.sharedMaterial = data.lazyTerrain.material;
		meshRenderer.shadowCastingMode = (data.lazyTerrain.castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
		base.transform.SetParent(data.lazyTerrain.transform);
		base.transform.localPosition = data.localPosition;
		if (grass != null)
		{
			grass.DrawFromData(this, data.deformingGrassData, lazyTerrain);
			deformingGrass = grass;
		}
	}
}
