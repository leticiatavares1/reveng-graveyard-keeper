using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class BuildGrid3D : MonoBehaviour
{
	private class BuffAreaData
	{
		public SGuid holderId;

		public List<Vector3> cellCoords = new List<Vector3>();

		public Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

		public Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		public void AddCell(Vector3 coords)
		{
			cellCoords.Add(coords);
			min = Vector3.Min(min, coords);
			max = Vector3.Max(max, coords);
		}

		public (Texture2D texture, Vector3 position, Vector2 size) CreateTexture()
		{
			Vector2 cELL_SIZE = BuildConsts.CELL_SIZE;
			int num = Mathf.RoundToInt((max.x - min.x) / cELL_SIZE.x) + 1;
			int num2 = Mathf.RoundToInt((max.z - min.z) / cELL_SIZE.y) + 1;
			int num3 = num + 2;
			int num4 = num2 + 2;
			Debug.Log($"base size: {num}x{num2}, expanded: {num3}x{num4}");
			Texture2D texture2D = new Texture2D(num3, num4, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] array = new Color32[num3 * num4];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Color32(0, 0, 0, 0);
			}
			foreach (Vector3 cellCoord in cellCoords)
			{
				int num5 = Mathf.RoundToInt((cellCoord.x - min.x) / cELL_SIZE.x) + 1;
				int num6 = Mathf.RoundToInt((cellCoord.z - min.z) / cELL_SIZE.y) + 1;
				if (num5 >= 0 && num5 < num3 && num6 >= 0 && num6 < num4)
				{
					array[num5 + num6 * num3] = new Color(0.1f, 0f, 0f, 1f);
				}
			}
			texture2D.SetPixels32(array);
			texture2D.Apply();
			Vector3 item = (min + max) * 0.5f;
			Vector2 item2 = new Vector2((float)num3 * cELL_SIZE.x, (float)num4 * cELL_SIZE.y);
			return (texture: texture2D, position: item, size: item2);
		}
	}

	private const int TEXTURE_SIZE = 512;

	private const int HALF_TEXTURE_SIZE = 256;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private Material material;

	[SerializeField]
	private PreSetModuleBuildView preSetModuleBuildViewPrefab;

	private HashSet<PreSetModuleBuildView> preSetModuleBuildViews = new HashSet<PreSetModuleBuildView>();

	private Pool preSetModuleBuildViewPool;

	[SerializeField]
	private BuffArea buffAreaPrefab;

	private List<BuffArea> activeBuffAreas = new List<BuffArea>();

	private Pool buffAreaPool;

	private readonly List<ElevationGridQuad> elevationQuads = new List<ElevationGridQuad>();

	private List<BuildElevationArea> elevationAreas;

	private bool wereModulesDrawn;

	private readonly Color[] gridColorData = new Color[262144];

	private readonly Color[] selectionColorData = new Color[262144];

	private int buildMode;

	private Texture2D texture;

	private Texture2D selectionTexture;

	private Material materialInstance;

	private Dictionary<SGuid, BuffAreaData> buffAreaData = new Dictionary<SGuid, BuffAreaData>();

	private HashSet<string> allowedExtensionWgoIds;

	private static readonly int shaderIdDataTexture = Shader.PropertyToID("_DataTex");

	private static readonly int shaderIdSelectionTexture = Shader.PropertyToID("_SelectionTex");

	private static readonly int shaderIdObjectScale = Shader.PropertyToID("_ObjectScale");

	private static readonly int shaderIdBuildMode = Shader.PropertyToID("_BuildMode");

	public HashSet<PreSetModuleBuildView> PreSetModuleBuildViews => preSetModuleBuildViews;

	public void Init()
	{
		preSetModuleBuildViewPrefab.gameObject.SetActive(value: false);
		buffAreaPrefab.gameObject.SetActive(value: false);
		preSetModuleBuildViewPool = LazyPooler.CreatePool(preSetModuleBuildViewPrefab, 5);
		buffAreaPool = LazyPooler.CreatePool(buffAreaPrefab, 5);
	}

	public void SetAllowedExtensionWgoIds(HashSet<string> allowed)
	{
		allowedExtensionWgoIds = ((allowed != null) ? new HashSet<string>(allowed) : null);
	}

	public void SetElevationAreas(List<BuildElevationArea> areas)
	{
		elevationAreas = areas;
	}

	public void Draw(BuildCellData[,] gridData, BuildCellSelectionData[,] selectionData, BuildCellBuffUsageData[,] buffUsageData, BuildCellData.BuildMode buildMode, bool drawExtensions = true)
	{
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		int num = gridData.GetLength(0) / 2;
		int num2 = gridData.GetLength(1) / 2;
		HashSet<BuildArea> hashSet = new HashSet<BuildArea>();
		foreach (BuffArea activeBuffArea in activeBuffAreas)
		{
			buffAreaPool.ReleaseObject(activeBuffArea);
		}
		activeBuffAreas.Clear();
		buffAreaData.Clear();
		for (int i = -num; i < num; i++)
		{
			for (int j = -num2; j < num2; j++)
			{
				int num3 = i + num;
				int num4 = j + num2;
				BuildCellData buildCellData = gridData[num3, num4];
				float r = (float)((buildCellData.State >> 1) & 1) * 0.1f;
				int num5 = (buildCellData.State >> 2) & 1;
				if (((uint)(buildCellData.State >> 4) & (true ? 1u : 0u)) != 0)
				{
					num5 |= 2;
				}
				if (((uint)(buildCellData.State >> 5) & (true ? 1u : 0u)) != 0)
				{
					num5 |= 4;
				}
				float g = (float)num5 * 0.1f;
				int num6 = 0;
				num6 |= ((((uint)(buildCellData.State >> 3) & (true ? 1u : 0u)) != 0) ? 1 : 0);
				num6 |= ((((uint)(buildCellData.State >> 6) & (true ? 1u : 0u)) != 0) ? 2 : 0);
				float num7 = (float)num6 * 0.1f;
				if (num7 > 0f && drawExtensions)
				{
					foreach (SGuid extension in buildCellData.ExtensionList)
					{
						if (allowedExtensionWgoIds != null)
						{
							Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(extension);
							if (wgoViewGlobal == null || !allowedExtensionWgoIds.Contains(wgoViewGlobal.Data.id))
							{
								continue;
							}
						}
						if (!buffAreaData.TryGetValue(extension, out var value))
						{
							value = new BuffAreaData
							{
								holderId = extension
							};
							buffAreaData[extension] = value;
						}
						value.AddCell(buildCellData.Coords);
					}
				}
				int num8 = 512 - (-i + 256);
				int num9 = 512 - (-j + 256);
				gridColorData[num8 + num9 * 512] = new Color(r, g, num7, 1f);
				int num10 = 0;
				if (selectionData != null)
				{
					num10 |= selectionData[num3, num4].state & 1;
				}
				if (buffUsageData != null && buffUsageData.GetLength(0) == length && buffUsageData.GetLength(1) == length2)
				{
					num10 |= buffUsageData[num3, num4].state & 2;
				}
				else if (((uint)num6 & 2u) != 0)
				{
					num10 |= 2;
				}
				float r2 = (float)num10 * 0.1f;
				float g2 = ((buildCellData.BuildAreaWithCovering != null) ? 0.1f : 0f);
				selectionColorData[num8 + num9 * 512] = new Color(r2, g2, 0f, 1f);
				if (!wereModulesDrawn && (bool)buildCellData.BuildAreaWithCovering && hashSet.Add(buildCellData.BuildAreaWithCovering))
				{
					PreSetModuleBuildView orCreateObject = preSetModuleBuildViewPool.GetOrCreateObject<PreSetModuleBuildView>();
					orCreateObject.transform.SetParent(base.transform);
					orCreateObject.SetPositionAndScaleAs(buildCellData.BuildAreaWithCovering);
					preSetModuleBuildViews.Add(orCreateObject);
				}
			}
		}
		foreach (KeyValuePair<SGuid, BuffAreaData> buffAreaDatum in buffAreaData)
		{
			(Texture2D texture, Vector3 position, Vector2 size) tuple = buffAreaDatum.Value.CreateTexture();
			Texture2D item = tuple.texture;
			Vector3 item2 = tuple.position;
			Vector2 item3 = tuple.size;
			BuffArea orCreateObject2 = buffAreaPool.GetOrCreateObject<BuffArea>();
			orCreateObject2.transform.SetParent(base.transform);
			orCreateObject2.Draw(item, item2, item3);
			activeBuffAreas.Add(orCreateObject2);
		}
		this.buildMode = (int)buildMode;
		CreateMaterial();
		ApplyDataToMaterial();
		DrawElevationQuads(gridData);
		wereModulesDrawn = true;
	}

	public void UpdateSelection(BuildCellSelectionData[,] selectionData, BuildCellBuffUsageData[,] buffUsageData)
	{
		if (selectionData == null && buffUsageData == null)
		{
			return;
		}
		CreateMaterial();
		int num = selectionData?.GetLength(0) ?? buffUsageData.GetLength(0);
		int num2 = selectionData?.GetLength(1) ?? buffUsageData.GetLength(1);
		int num3 = num / 2;
		int num4 = num2 / 2;
		for (int i = -num3; i < num3; i++)
		{
			for (int j = -num4; j < num4; j++)
			{
				int num5 = i + num3;
				int num6 = j + num4;
				int num7 = 512 - (-i + 256);
				int num8 = 512 - (-j + 256);
				int num9 = 0;
				if (selectionData != null)
				{
					num9 |= selectionData[num5, num6].state & 1;
				}
				if (buffUsageData != null)
				{
					num9 |= buffUsageData[num5, num6].state & 2;
				}
				float r = (float)num9 * 0.1f;
				int num10 = num7 + num8 * 512;
				float g = selectionColorData[num10].g;
				selectionColorData[num10] = new Color(r, g, 0f, 1f);
			}
		}
		selectionTexture.SetPixels(selectionColorData);
		selectionTexture.Apply();
	}

	public void Clear()
	{
		foreach (PreSetModuleBuildView preSetModuleBuildView in preSetModuleBuildViews)
		{
			preSetModuleBuildViewPool.ReleaseObject(preSetModuleBuildView);
		}
		foreach (BuffArea activeBuffArea in activeBuffAreas)
		{
			buffAreaPool.ReleaseObject(activeBuffArea);
		}
		activeBuffAreas.Clear();
		buffAreaData.Clear();
		preSetModuleBuildViews.Clear();
		for (int i = 0; i < elevationQuads.Count; i++)
		{
			elevationQuads[i].Hide();
		}
		wereModulesDrawn = false;
	}

	private void CreateMaterial()
	{
		if (materialInstance == null)
		{
			if (meshRenderer == null)
			{
				return;
			}
			materialInstance = new Material(material);
			texture = new Texture2D(512, 512, TextureFormat.RGBA32, mipChain: true)
			{
				filterMode = FilterMode.Point
			};
			selectionTexture = new Texture2D(512, 512, TextureFormat.RGBA32, mipChain: true)
			{
				filterMode = FilterMode.Point
			};
		}
		meshRenderer.material = materialInstance;
	}

	private void DrawElevationQuads(BuildCellData[,] gridData)
	{
		for (int i = 0; i < elevationQuads.Count; i++)
		{
			elevationQuads[i].Hide();
		}
		if (elevationAreas == null || elevationAreas.Count == 0 || gridData == null)
		{
			return;
		}
		int length = gridData.GetLength(0);
		int length2 = gridData.GetLength(1);
		int num = length / 2;
		int num2 = length2 / 2;
		Vector2 cELL_SIZE = BuildConsts.CELL_SIZE;
		int num3 = 0;
		for (int j = 0; j < elevationAreas.Count; j++)
		{
			Rect groundRect = elevationAreas[j].GroundRect;
			int num4 = int.MaxValue;
			int num5 = int.MaxValue;
			int num6 = int.MinValue;
			int num7 = int.MinValue;
			float num8 = float.MaxValue;
			float num9 = float.MaxValue;
			for (int k = 0; k < length; k++)
			{
				for (int l = 0; l < length2; l++)
				{
					if (gridData[k, l].State == 0)
					{
						continue;
					}
					Vector3 coords = gridData[k, l].Coords;
					if (groundRect.Contains(new Vector2(coords.x, coords.z)))
					{
						if (k < num4)
						{
							num4 = k;
						}
						if (l < num5)
						{
							num5 = l;
						}
						if (k > num6)
						{
							num6 = k;
						}
						if (l > num7)
						{
							num7 = l;
						}
						if (coords.x < num8)
						{
							num8 = coords.x;
						}
						if (coords.z < num9)
						{
							num9 = coords.z;
						}
					}
				}
			}
			if (num6 < num4)
			{
				Debug.LogWarning($"[ElevationQuad] area#{j} rect={groundRect} -> NO scanned cells inside footprint (quad skipped)");
				continue;
			}
			int a = num6 - num4 + 1;
			int b = num7 - num5 + 1;
			int num10 = Mathf.Max(a, b) + 2;
			Color[] array = new Color[num10 * num10];
			Color[] array2 = new Color[num10 * num10];
			for (int m = num4; m <= num6; m++)
			{
				for (int n = num5; n <= num7; n++)
				{
					if (gridData[m, n].State != 0)
					{
						Vector3 coords2 = gridData[m, n].Coords;
						if (groundRect.Contains(new Vector2(coords2.x, coords2.z)))
						{
							int num11 = 256 + m - num + (256 + n - num2) * 512;
							int num12 = m - num4 + 1 + (n - num5 + 1) * num10;
							array[num12] = gridColorData[num11];
							array2[num12] = selectionColorData[num11];
						}
					}
				}
			}
			float x = num8 + ((float)num10 * 0.5f - 1.5f) * cELL_SIZE.x;
			float z = num9 + ((float)num10 * 0.5f - 1.5f) * cELL_SIZE.y;
			Vector3 worldCenter = VisualConsts.ProjectGroundPointToElevation(new Vector3(x, elevationAreas[j].GroundY, z), elevationAreas[j].ElevationY) + VisualConsts.GetLayerOffset(2);
			worldCenter.z += cELL_SIZE.y;
			worldCenter.y += 0.005f;
			Vector2 worldSize = new Vector2((float)num10 * cELL_SIZE.x, (float)num10 * cELL_SIZE.y);
			GetOrCreateElevationQuad(num3).Draw(array, array2, num10, worldCenter, worldSize, buildMode);
			num3++;
		}
	}

	private ElevationGridQuad GetOrCreateElevationQuad(int index)
	{
		if (index < elevationQuads.Count)
		{
			return elevationQuads[index];
		}
		Transform parent = ((base.transform.parent != null) ? base.transform.parent : base.transform);
		GameObject obj = new GameObject("ElevationGridQuad");
		obj.transform.SetParent(parent, worldPositionStays: false);
		ElevationGridQuad elevationGridQuad = obj.AddComponent<ElevationGridQuad>();
		elevationGridQuad.Setup(material);
		elevationQuads.Add(elevationGridQuad);
		return elevationGridQuad;
	}

	private void ApplyDataToMaterial()
	{
		texture.SetPixels(gridColorData);
		texture.Apply();
		selectionTexture.SetPixels(selectionColorData);
		selectionTexture.Apply();
		materialInstance.SetTexture(shaderIdDataTexture, texture);
		materialInstance.SetTexture(shaderIdSelectionTexture, selectionTexture);
		materialInstance.SetInt(shaderIdBuildMode, buildMode);
		materialInstance.SetVector(shaderIdObjectScale, base.transform.lossyScale);
	}
}
