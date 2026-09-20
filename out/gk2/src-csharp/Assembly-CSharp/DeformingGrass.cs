using LazyBearTechnology;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class DeformingGrass : MonoBehaviour
{
	private static readonly int idMainTex = Shader.PropertyToID("_MainTex");

	[SerializeField]
	private Mesh fullMesh;

	public DeformingGrassAtlas atlas;

	public LazyTerrain lazyTerrain;

	private MaterialPropertyBlock matProp;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private MeshFilter meshFilter;

	private static bool isEnabled = true;

	public Mesh Mesh => fullMesh;

	private MaterialPropertyBlock MatProp => matProp ?? (matProp = new MaterialPropertyBlock());

	private MeshRenderer MeshRenderer
	{
		get
		{
			if (meshRenderer == null)
			{
				meshRenderer = GetComponent<MeshRenderer>();
			}
			return meshRenderer;
		}
	}

	private MeshFilter MeshFilter
	{
		get
		{
			if (meshFilter == null)
			{
				meshFilter = GetComponent<MeshFilter>();
			}
			return meshFilter;
		}
	}

	public static bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			DeformingGrass[] array = Object.FindObjectsOfType<DeformingGrass>(includeInactive: true);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value);
			}
			isEnabled = value;
		}
	}

	public void ApplyMaterial()
	{
		if (!(atlas == null))
		{
			MatProp.SetTexture(idMainTex, atlas.texture);
			MeshRenderer.SetPropertyBlock(MatProp);
		}
	}

	private void OnEnable()
	{
		ApplyMaterial();
		ApplyShadowSettings();
	}

	public void ApplyShadowSettings()
	{
		MeshRenderer.shadowCastingMode = (LazySingletonSO<DeformingGrassSettings>.Instance.grassShadow ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off);
	}

	public void DrawFromData(LazyTerrainMesh lazyTerrainMesh, DeformingGrassData data, LazyTerrain lazyTerrain)
	{
		if (Application.isPlaying)
		{
			LazyTerrainMeshCollection.Runtime_StripCpuMeshData(data.mesh);
		}
		MeshFilter.mesh = data.mesh;
		fullMesh = data.mesh;
		this.lazyTerrain = lazyTerrain;
		base.transform.SetParent(lazyTerrainMesh.transform);
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = quaternion.identity;
	}
}
