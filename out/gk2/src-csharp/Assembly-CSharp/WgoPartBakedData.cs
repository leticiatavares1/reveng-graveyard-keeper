using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class WgoPartBakedData
{
	[Serializable]
	public class InternalBoundsData
	{
		public int hash;

		public Rect rect;
	}

	[Serializable]
	public class WgoPartDockPointsBakedData
	{
		public int hash;

		public List<DockPointData.Baked> dockPoints;
	}

	[Serializable]
	public class PlannerMeshData
	{
		[OdinSerialize]
		public int hash;

		[OdinSerialize]
		public List<Vector3> vertices = new List<Vector3>();

		[OdinSerialize]
		public List<int> triangles = new List<int>();
	}

	[Serializable]
	public class GraphUpdateSceneBoxData
	{
		[OdinSerialize]
		public int hash;

		[OdinSerialize]
		public Vector3 localCenter;

		[OdinSerialize]
		public Vector3 size = Vector3.one;

		[OdinSerialize]
		public bool setWalkability;

		[OdinSerialize]
		public bool updatePhysics;

		[OdinSerialize]
		public int penaltyDelta;

		public void SetPenaltyDelta(int value)
		{
			penaltyDelta = value;
		}
	}

	[Serializable]
	public class GDPointsVariationBakedData
	{
		public int hash;

		public List<GDPointBakedData> gdPoints = new List<GDPointBakedData>();
	}

	[Serializable]
	public class GDPointBakedData
	{
		public string id;

		public string customTag;

		public Direction direction;

		public Vector3 localPosition;

		public bool isTransitPoint;

		public string transitToGdPointId;

		public string worldIdToTransit;

		public bool enabled;

		public List<string> nextGdPointIds = new List<string>();
	}

	[Serializable]
	public class CustomNavMeshCutPrefabsVariationBakedData
	{
		public int hash;

		public List<CustomNavMeshCutPrefabEntryBakedData> entries = new List<CustomNavMeshCutPrefabEntryBakedData>();
	}

	[Serializable]
	public class CustomNavMeshCutPrefabEntryBakedData
	{
		public AssetReferenceGameObject prefabRef;

		public Vector3 localPosition;

		public Quaternion localRotation = Quaternion.identity;

		public Vector3 localScale = Vector3.one;
	}

	public string id;

	[SerializeField]
	public string prefabGuid;

	[SerializeField]
	private List<WgoPartDockPointsBakedData> pointsList;

	[SerializeField]
	private List<InternalBoundsData> boundsData;

	[OdinSerialize]
	private List<PlannerMeshData> plannerMeshesData;

	[OdinSerialize]
	private List<GraphUpdateSceneBoxData> graphUpdateSceneBoxesData;

	[OdinSerialize]
	private List<CustomNavMeshCutPrefabsVariationBakedData> customNavMeshCutPrefabsBakedData = new List<CustomNavMeshCutPrefabsVariationBakedData>();

	[SerializeField]
	private GameRes moduleBuildingTypes;

	[SerializeField]
	private float radiusSpehereCutter;

	[SerializeField]
	private ChunkBoundsPair chunkBounds;

	[SerializeField]
	private bool hasChunkBounds;

	[SerializeField]
	private List<GDPointsVariationBakedData> gdPointsBakedData = new List<GDPointsVariationBakedData>();

	private Dictionary<int, Rect> variationCollisionBoundsRectDict;

	private Dictionary<int, List<DockPointData.Baked>> variationDockPointsDict;

	private Dictionary<int, PlannerMeshData> variationPlannerMeshDict;

	private Dictionary<int, GraphUpdateSceneBoxData> variationGraphUpdateSceneBoxDict;

	private Dictionary<int, List<GDPointBakedData>> variationGDPointsDict;

	private Dictionary<int, List<CustomNavMeshCutPrefabEntryBakedData>> variationCustomNavMeshCutPrefabsDict;

	public IReadOnlyList<WgoPartDockPointsBakedData> PointsList => pointsList;

	public GameRes ModuleBuildingTypes => moduleBuildingTypes;

	public static WgoPartBakedData Empty => new WgoPartBakedData(string.Empty);

	public float RadiusSpehereCutter
	{
		get
		{
			return radiusSpehereCutter;
		}
		set
		{
			radiusSpehereCutter = value;
		}
	}

	public bool HasChunkBounds => hasChunkBounds;

	public ChunkBoundsPair ChunkBounds => chunkBounds;

	public IReadOnlyDictionary<int, List<GDPointBakedData>> VariationGDPointsDict
	{
		get
		{
			if (variationGDPointsDict == null)
			{
				variationGDPointsDict = new Dictionary<int, List<GDPointBakedData>>();
				if (gdPointsBakedData == null)
				{
					gdPointsBakedData = new List<GDPointsVariationBakedData>();
				}
				foreach (GDPointsVariationBakedData gdPointsBakedDatum in gdPointsBakedData)
				{
					if (gdPointsBakedDatum?.gdPoints != null)
					{
						variationGDPointsDict[gdPointsBakedDatum.hash] = gdPointsBakedDatum.gdPoints;
					}
				}
			}
			return variationGDPointsDict;
		}
	}

	public IReadOnlyDictionary<int, Rect> VariationCollisionBoundsRectDict
	{
		get
		{
			if (variationCollisionBoundsRectDict == null)
			{
				variationCollisionBoundsRectDict = new Dictionary<int, Rect>();
				foreach (InternalBoundsData boundsDatum in boundsData)
				{
					variationCollisionBoundsRectDict.Add(boundsDatum.hash, boundsDatum.rect);
				}
			}
			return variationCollisionBoundsRectDict;
		}
	}

	public IReadOnlyDictionary<int, List<DockPointData.Baked>> VariationDockPointsDict
	{
		get
		{
			if (variationDockPointsDict == null)
			{
				variationDockPointsDict = new Dictionary<int, List<DockPointData.Baked>>();
				foreach (WgoPartDockPointsBakedData points in pointsList)
				{
					variationDockPointsDict.Add(points.hash, points.dockPoints);
				}
			}
			return variationDockPointsDict;
		}
	}

	public IReadOnlyDictionary<int, PlannerMeshData> VariationPlannerMeshDict
	{
		get
		{
			if (variationPlannerMeshDict == null)
			{
				goto IL_0028;
			}
			if (variationPlannerMeshDict.Count == 0)
			{
				List<PlannerMeshData> list = plannerMeshesData;
				if (list != null && list.Count > 0)
				{
					goto IL_0028;
				}
			}
			goto IL_008a;
			IL_008a:
			return variationPlannerMeshDict;
			IL_0028:
			variationPlannerMeshDict = new Dictionary<int, PlannerMeshData>();
			if (plannerMeshesData == null)
			{
				plannerMeshesData = new List<PlannerMeshData>();
			}
			foreach (PlannerMeshData plannerMeshesDatum in plannerMeshesData)
			{
				if (plannerMeshesDatum != null)
				{
					variationPlannerMeshDict[plannerMeshesDatum.hash] = plannerMeshesDatum;
				}
			}
			goto IL_008a;
		}
	}

	public IReadOnlyDictionary<int, List<CustomNavMeshCutPrefabEntryBakedData>> VariationCustomNavMeshCutPrefabsDict
	{
		get
		{
			if (variationCustomNavMeshCutPrefabsDict == null)
			{
				variationCustomNavMeshCutPrefabsDict = new Dictionary<int, List<CustomNavMeshCutPrefabEntryBakedData>>();
				if (customNavMeshCutPrefabsBakedData == null)
				{
					customNavMeshCutPrefabsBakedData = new List<CustomNavMeshCutPrefabsVariationBakedData>();
				}
				foreach (CustomNavMeshCutPrefabsVariationBakedData customNavMeshCutPrefabsBakedDatum in customNavMeshCutPrefabsBakedData)
				{
					if (customNavMeshCutPrefabsBakedDatum?.entries != null)
					{
						variationCustomNavMeshCutPrefabsDict[customNavMeshCutPrefabsBakedDatum.hash] = customNavMeshCutPrefabsBakedDatum.entries;
					}
				}
			}
			return variationCustomNavMeshCutPrefabsDict;
		}
	}

	public IReadOnlyDictionary<int, GraphUpdateSceneBoxData> VariationGraphUpdateSceneBoxDict
	{
		get
		{
			if (variationGraphUpdateSceneBoxDict == null)
			{
				goto IL_0028;
			}
			if (variationGraphUpdateSceneBoxDict.Count == 0)
			{
				List<GraphUpdateSceneBoxData> list = graphUpdateSceneBoxesData;
				if (list != null && list.Count > 0)
				{
					goto IL_0028;
				}
			}
			goto IL_008a;
			IL_008a:
			return variationGraphUpdateSceneBoxDict;
			IL_0028:
			variationGraphUpdateSceneBoxDict = new Dictionary<int, GraphUpdateSceneBoxData>();
			if (graphUpdateSceneBoxesData == null)
			{
				graphUpdateSceneBoxesData = new List<GraphUpdateSceneBoxData>();
			}
			foreach (GraphUpdateSceneBoxData graphUpdateSceneBoxesDatum in graphUpdateSceneBoxesData)
			{
				if (graphUpdateSceneBoxesDatum != null)
				{
					variationGraphUpdateSceneBoxDict[graphUpdateSceneBoxesDatum.hash] = graphUpdateSceneBoxesDatum;
				}
			}
			goto IL_008a;
		}
	}

	public void SetChunkBounds(ChunkBoundsPair bounds)
	{
		chunkBounds = bounds;
		hasChunkBounds = bounds.withShadows.size.sqrMagnitude > 0.0001f || bounds.withoutShadows.size.sqrMagnitude > 0.0001f;
	}

	public void SetGDPointsBakedData(List<GDPointBakedData> data, int hash)
	{
		if (gdPointsBakedData == null)
		{
			gdPointsBakedData = new List<GDPointsVariationBakedData>();
		}
		gdPointsBakedData.RemoveAll((GDPointsVariationBakedData el) => el.hash == hash);
		if (data != null && data.Count > 0)
		{
			gdPointsBakedData.Add(new GDPointsVariationBakedData
			{
				hash = hash,
				gdPoints = data
			});
		}
		variationGDPointsDict = null;
	}

	public WgoPartBakedData(string id)
	{
		this.id = id;
		pointsList = new List<WgoPartDockPointsBakedData>();
		boundsData = new List<InternalBoundsData>();
		plannerMeshesData = new List<PlannerMeshData>();
		graphUpdateSceneBoxesData = new List<GraphUpdateSceneBoxData>();
		variationCollisionBoundsRectDict = new Dictionary<int, Rect>();
		variationDockPointsDict = new Dictionary<int, List<DockPointData.Baked>>();
		variationPlannerMeshDict = new Dictionary<int, PlannerMeshData>();
		variationGraphUpdateSceneBoxDict = new Dictionary<int, GraphUpdateSceneBoxData>();
		moduleBuildingTypes = new GameRes();
		radiusSpehereCutter = -1f;
	}

	public void SetCollisionBoundsRect(Rect rect, int hash)
	{
		boundsData.Add(new InternalBoundsData
		{
			hash = hash,
			rect = rect
		});
	}

	public void SetDockPointsData(DockPointData.Baked[] pointDatas, int hash)
	{
		pointsList.Add(new WgoPartDockPointsBakedData
		{
			hash = hash,
			dockPoints = pointDatas.ToList()
		});
	}

	public void SetPlannerMeshData(Vector3[] vertices, int[] triangles, int hash)
	{
		if (vertices != null && triangles != null && vertices.Length != 0 && triangles.Length >= 3)
		{
			if (plannerMeshesData == null)
			{
				plannerMeshesData = new List<PlannerMeshData>();
			}
			PlannerMeshData item = new PlannerMeshData
			{
				hash = hash,
				vertices = vertices.ToList(),
				triangles = triangles.ToList()
			};
			plannerMeshesData.RemoveAll((PlannerMeshData el) => el.hash == hash);
			plannerMeshesData.Add(item);
			variationPlannerMeshDict = null;
		}
	}

	public bool TryGetPlannerMeshData(int hash, out PlannerMeshData data)
	{
		if (VariationPlannerMeshDict.TryGetValue(hash, out data))
		{
			return true;
		}
		return false;
	}

	public void SetGraphUpdateSceneBoxData(GraphUpdateSceneBoxData data, int hash)
	{
		if (data != null && !(data.size.sqrMagnitude <= 0f))
		{
			if (graphUpdateSceneBoxesData == null)
			{
				graphUpdateSceneBoxesData = new List<GraphUpdateSceneBoxData>();
			}
			data.hash = hash;
			graphUpdateSceneBoxesData.RemoveAll((GraphUpdateSceneBoxData el) => el.hash == hash);
			graphUpdateSceneBoxesData.Add(data);
			variationGraphUpdateSceneBoxDict = null;
		}
	}

	public bool TryGetGraphUpdateSceneBoxData(int hash, out GraphUpdateSceneBoxData data)
	{
		if (VariationGraphUpdateSceneBoxDict.TryGetValue(hash, out data))
		{
			return true;
		}
		data = null;
		return false;
	}

	public void SetCustomNavMeshCutPrefabs(List<CustomNavMeshCutPrefabEntryBakedData> entries, int hash)
	{
		if (customNavMeshCutPrefabsBakedData == null)
		{
			customNavMeshCutPrefabsBakedData = new List<CustomNavMeshCutPrefabsVariationBakedData>();
		}
		customNavMeshCutPrefabsBakedData.RemoveAll((CustomNavMeshCutPrefabsVariationBakedData el) => el.hash == hash);
		if (entries != null && entries.Count > 0)
		{
			customNavMeshCutPrefabsBakedData.Add(new CustomNavMeshCutPrefabsVariationBakedData
			{
				hash = hash,
				entries = entries
			});
		}
		variationCustomNavMeshCutPrefabsDict = null;
	}

	public bool TryGetCustomNavMeshCutPrefabs(int hash, out List<CustomNavMeshCutPrefabEntryBakedData> entries)
	{
		if (VariationCustomNavMeshCutPrefabsDict.TryGetValue(hash, out entries))
		{
			return true;
		}
		entries = null;
		return false;
	}

	public static bool TryCreateMesh(PlannerMeshData data, string meshName, out Mesh mesh)
	{
		mesh = null;
		if (data == null || data.vertices == null || data.triangles == null || data.vertices.Count == 0 || data.triangles.Count < 3)
		{
			return false;
		}
		mesh = new Mesh
		{
			name = meshName
		};
		mesh.SetVertices(data.vertices);
		mesh.SetTriangles(data.triangles, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return true;
	}

	public void SetModuleBuildingTypes(GameRes moduleBuildingTypes)
	{
		this.moduleBuildingTypes = moduleBuildingTypes;
	}

	public WgoPartDockPointsBakedData GetDockPointsData(int idx)
	{
		return pointsList[idx];
	}

	public Rect GetCollisionBoundsRect(int idx)
	{
		return boundsData[idx].rect;
	}
}
