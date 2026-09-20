using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ElevationGridQuad : MonoBehaviour
{
	private static readonly int dataTexId = Shader.PropertyToID("_DataTex");

	private static readonly int selectionTexId = Shader.PropertyToID("_SelectionTex");

	private static readonly int objectScaleId = Shader.PropertyToID("_ObjectScale");

	private static readonly int buildModeId = Shader.PropertyToID("_BuildMode");

	private static Mesh sharedMesh;

	private MeshRenderer meshRenderer;

	private Material materialInstance;

	private Texture2D dataTexture;

	private Texture2D selectionTexture;

	private int textureSize;

	public void Setup(Material gridMaterial)
	{
		MeshFilter component = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		component.sharedMesh = GetSharedMesh();
		materialInstance = new Material(gridMaterial);
		meshRenderer.material = materialInstance;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
	}

	public void Draw(Color[] data, Color[] selection, int size, Vector3 worldCenter, Vector2 worldSize, int buildMode)
	{
		EnsureTextures(size);
		dataTexture.SetPixels(data);
		dataTexture.Apply();
		selectionTexture.SetPixels(selection);
		selectionTexture.Apply();
		Transform parent = base.transform.parent;
		Vector3 vector = ((parent != null) ? parent.lossyScale : Vector3.one);
		base.transform.localScale = new Vector3(worldSize.x / vector.x, 1f / vector.y, worldSize.y / vector.z);
		base.transform.position = worldCenter;
		materialInstance.SetTexture(dataTexId, dataTexture);
		materialInstance.SetTexture(selectionTexId, selectionTexture);
		materialInstance.SetInt(buildModeId, buildMode);
		materialInstance.SetVector(objectScaleId, base.transform.lossyScale);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void EnsureTextures(int size)
	{
		if (!(dataTexture != null) || textureSize != size)
		{
			if (dataTexture != null)
			{
				Object.Destroy(dataTexture);
			}
			if (selectionTexture != null)
			{
				Object.Destroy(selectionTexture);
			}
			dataTexture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			selectionTexture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			textureSize = size;
		}
	}

	private void OnDestroy()
	{
		if (dataTexture != null)
		{
			Object.Destroy(dataTexture);
		}
		if (selectionTexture != null)
		{
			Object.Destroy(selectionTexture);
		}
		if (materialInstance != null)
		{
			Object.Destroy(materialInstance);
		}
	}

	private static Mesh GetSharedMesh()
	{
		if (sharedMesh != null)
		{
			return sharedMesh;
		}
		sharedMesh = new Mesh
		{
			name = "ElevationGridQuad"
		};
		sharedMesh.vertices = new Vector3[4]
		{
			new Vector3(-0.5f, 0f, -0.5f),
			new Vector3(0.5f, 0f, -0.5f),
			new Vector3(0.5f, 0f, 0.5f),
			new Vector3(-0.5f, 0f, 0.5f)
		};
		sharedMesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		sharedMesh.triangles = new int[6] { 0, 2, 1, 0, 3, 2 };
		sharedMesh.normals = new Vector3[4]
		{
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};
		sharedMesh.RecalculateBounds();
		return sharedMesh;
	}
}
