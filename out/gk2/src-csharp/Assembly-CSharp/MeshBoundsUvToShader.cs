using UnityEngine;

[ExecuteAlways]
public class MeshBoundsUvToShader : MonoBehaviour
{
	private enum MeshUvPlane
	{
		XZ,
		XY,
		YZ
	}

	[SerializeField]
	private Renderer targetRenderer;

	[SerializeField]
	private MeshFilter targetMeshFilter;

	[SerializeField]
	private Vector2 meshUvScale = Vector2.one;

	[SerializeField]
	private Vector2 meshUvOffset = Vector2.zero;

	[SerializeField]
	private MeshUvPlane meshUvPlane;

	[SerializeField]
	private bool updateEveryFrame = true;

	private readonly int shaderIdIconTexture = Shader.PropertyToID("_IconTexture");

	private readonly int shaderIdGridTexture = Shader.PropertyToID("_GridTexture");

	private readonly int shaderIdCellsCount = Shader.PropertyToID("_CellsCount");

	private MaterialPropertyBlock propertyBlock;

	private Vector3 lastScale;

	private Vector3 lastPosition;

	private void OnEnable()
	{
		Apply();
	}

	private void OnValidate()
	{
		Apply();
	}

	private void Update()
	{
		if (updateEveryFrame && (base.transform.lossyScale != lastScale || base.transform.position != lastPosition))
		{
			Apply();
			lastScale = base.transform.lossyScale;
			lastPosition = base.transform.position;
		}
	}

	public void SetTextures(Texture2D iconTexture, Texture2D gridTexture)
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetTexture(shaderIdIconTexture, iconTexture);
		propertyBlock.SetTexture(shaderIdGridTexture, gridTexture);
		targetRenderer.SetPropertyBlock(propertyBlock);
	}

	public void SetCellsCount(Vector2Int cellsCount)
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		targetRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetVector(shaderIdCellsCount, new Vector4(cellsCount.x, cellsCount.y, 0f, 0f));
		targetRenderer.SetPropertyBlock(propertyBlock);
	}

	public void Apply()
	{
		if (targetRenderer == null)
		{
			targetRenderer = GetComponent<Renderer>();
		}
		if (targetMeshFilter == null)
		{
			targetMeshFilter = GetComponent<MeshFilter>();
		}
		if (targetRenderer == null || targetMeshFilter == null)
		{
			return;
		}
		Mesh sharedMesh = targetMeshFilter.sharedMesh;
		if (!(sharedMesh == null))
		{
			Bounds bounds = sharedMesh.bounds;
			Vector3 vector = Vector3.Scale(bounds.size, targetRenderer.transform.lossyScale);
			Vector2 worldSizeUv = GetWorldSizeUv(vector);
			Vector2 worldSizeUv2 = GetWorldSizeUv(bounds.size);
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			targetRenderer.GetPropertyBlock(propertyBlock);
			propertyBlock.SetVector("_MeshBoundsMin", bounds.min);
			propertyBlock.SetVector("_MeshBoundsSize", bounds.size);
			propertyBlock.SetVector("_MeshWorldSize", vector);
			propertyBlock.SetVector("_MeshWorldSizeUV", new Vector4(worldSizeUv.x, worldSizeUv.y, 0f, 0f));
			propertyBlock.SetVector("_MeshBoundsSizeUV", new Vector4(worldSizeUv2.x, worldSizeUv2.y, 0f, 0f));
			propertyBlock.SetVector("_MeshUVScale", new Vector4(meshUvScale.x, meshUvScale.y, 0f, 0f));
			propertyBlock.SetVector("_MeshUVOffset", new Vector4(meshUvOffset.x, meshUvOffset.y, 0f, 0f));
			targetRenderer.SetPropertyBlock(propertyBlock);
		}
	}

	private Vector2 GetWorldSizeUv(Vector3 worldSize)
	{
		return meshUvPlane switch
		{
			MeshUvPlane.XY => new Vector2(worldSize.x, worldSize.y), 
			MeshUvPlane.YZ => new Vector2(worldSize.y, worldSize.z), 
			_ => new Vector2(worldSize.x, worldSize.z), 
		};
	}
}
