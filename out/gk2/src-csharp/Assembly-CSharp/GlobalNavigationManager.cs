using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using Pathfinding;
using Pathfinding.Graphs.Navmesh;
using UnityEngine;

public class GlobalNavigationManager : LazySingleton<GlobalNavigationManager>
{
	private AstarPath astarPath;

	[SerializeField]
	private GraphCutUnit graphCutUnitPrefab;

	[SerializeField]
	private GraphCustomNavMeshCutUnit graphCustomNavMeshCutUnitPrefab;

	[SerializeField]
	private GraphUpdateSceneUnit graphUpdateSceneUnitPrefab;

	[SerializeField]
	private RecastFloor recastFloorPrefab;

	[SerializeField]
	private PathCalculationUnit pathCalculationUnitPrefab;

	private Pool obstacleUnitPool;

	private Dictionary<SGuid, GraphCutUnit> usedUnits = new Dictionary<SGuid, GraphCutUnit>();

	private Pool graphUpdateSceneUnitPool;

	private Dictionary<SGuid, GraphUpdateSceneUnit> usedGraphUpdateSceneUnits = new Dictionary<SGuid, GraphUpdateSceneUnit>();

	private Pool customNavMeshCutUnitPool;

	private Dictionary<SGuid, GraphCustomNavMeshCutUnit> usedCustomNavMeshCutUnits = new Dictionary<SGuid, GraphCustomNavMeshCutUnit>();

	private Pool recastFloorPool;

	private Pool pathCalculationUnitPool;

	private static readonly Vector3[] UnitBoxVerts = new Vector3[8]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(-1f, 1f, 1f)
	};

	private static readonly int[] UnitBoxTris = new int[36]
	{
		0, 1, 2, 0, 2, 3, 6, 5, 4, 7,
		6, 4, 0, 5, 1, 0, 4, 5, 1, 6,
		2, 1, 5, 6, 2, 7, 3, 2, 6, 7,
		3, 4, 0, 3, 7, 4
	};

	private const float RecastFloorHalfHeight = 0.05f;

	public void InitRecastGraph(LazyConsts.Navigation.Graph graph, Vector3 center, Vector2 size, bool dontUseRecastFloor = false, float height = 1f, IReadOnlyList<WorldZoneNavigationHoleBakedData> navigationHoles = null)
	{
		if (graph == LazyConsts.Navigation.Graph.None)
		{
			return;
		}
		if (astarPath.graphs.Length < (int)(graph + 1))
		{
			Debug.LogWarning($"Graph {graph} not specified in {astarPath}", (UnityEngine.Object)(object)astarPath);
			return;
		}
		if (!(astarPath.graphs[(int)graph] is RecastGraph recastGraph))
		{
			Debug.LogWarning($"Graph {graph} is not RecastGraph, it's {astarPath.graphs[(int)graph].GetType()}", (UnityEngine.Object)(object)astarPath);
			return;
		}
		recastGraph.forcedBoundsCenter = center;
		recastGraph.forcedBoundsSize = new Vector3(size.x, height, size.y);
		Action<RecastMeshGatherer> action = CreateCollectMeshesCallback(center, size, !dontUseRecastFloor, navigationHoles);
		if (action != null)
		{
			RecastGraph.CollectionSettings collectionSettings = recastGraph.collectionSettings;
			collectionSettings.onCollectMeshes = (Action<RecastMeshGatherer>)Delegate.Combine(collectionSettings.onCollectMeshes, action);
		}
		try
		{
			astarPath.Scan(recastGraph);
		}
		finally
		{
			if (action != null)
			{
				RecastGraph.CollectionSettings collectionSettings2 = recastGraph.collectionSettings;
				collectionSettings2.onCollectMeshes = (Action<RecastMeshGatherer>)Delegate.Remove(collectionSettings2.onCollectMeshes, action);
			}
		}
		int walkableNodesCount = 0;
		recastGraph.GetNodes(delegate(GraphNode node)
		{
			if (node.Walkable)
			{
				walkableNodesCount++;
			}
		});
		Debug.Log($"[GlobalNavigationManager] InitRecastGraph graph: {graph}, scanned: {recastGraph.isScanned}, walkable: {walkableNodesCount}, center: {center:F3}, size: {size:F3}");
	}

	public IEnumerator InitRecastGraphAsync(LazyConsts.Navigation.Graph graph, Vector3 center, Vector2 size, bool dontUseRecastFloor = false, float height = 1f, IReadOnlyList<WorldZoneNavigationHoleBakedData> navigationHoles = null)
	{
		if (graph == LazyConsts.Navigation.Graph.None)
		{
			yield return null;
		}
		if (astarPath.graphs.Length < (int)(graph + 1))
		{
			Debug.LogWarning($"Graph {graph} not specified in {astarPath}", (UnityEngine.Object)(object)astarPath);
			yield return null;
		}
		if (!(astarPath.graphs[(int)graph] is RecastGraph recastGraph))
		{
			Debug.LogWarning($"Graph {graph} is not RecastGraph, it's {astarPath.graphs[(int)graph].GetType()}", (UnityEngine.Object)(object)astarPath);
			yield break;
		}
		recastGraph.forcedBoundsCenter = center;
		recastGraph.forcedBoundsSize = new Vector3(size.x, height, size.y);
		Action<RecastMeshGatherer> collectMeshes = CreateCollectMeshesCallback(center, size, !dontUseRecastFloor, navigationHoles);
		if (collectMeshes != null)
		{
			RecastGraph.CollectionSettings collectionSettings = recastGraph.collectionSettings;
			collectionSettings.onCollectMeshes = (Action<RecastMeshGatherer>)Delegate.Combine(collectionSettings.onCollectMeshes, collectMeshes);
		}
		try
		{
			foreach (Progress item in LazySingleton<AStarScanScheduler>.Instance.ScanGraphAsync(recastGraph))
			{
				Debug.Log($"Scanning Graph {graph}: {item}");
				yield return null;
			}
		}
		finally
		{
			if (collectMeshes != null)
			{
				RecastGraph.CollectionSettings collectionSettings2 = recastGraph.collectionSettings;
				collectionSettings2.onCollectMeshes = (Action<RecastMeshGatherer>)Delegate.Remove(collectionSettings2.onCollectMeshes, collectMeshes);
			}
		}
		int walkableNodesCount = 0;
		recastGraph.GetNodes(delegate(GraphNode node)
		{
			if (node.Walkable)
			{
				walkableNodesCount++;
			}
		});
		Debug.Log($"[GlobalNavigationManager] InitRecastGraph graph: {graph}, scanned: {recastGraph.isScanned}, walkable: {walkableNodesCount}, center: {center:F3}, size: {size:F3}");
	}

	private static Action<RecastMeshGatherer> CreateCollectMeshesCallback(Vector3 center, Vector2 size, bool addFloor, IReadOnlyList<WorldZoneNavigationHoleBakedData> navigationHoles)
	{
		bool flag = navigationHoles != null && navigationHoles.Count > 0;
		if (!addFloor && !flag)
		{
			return null;
		}
		return delegate(RecastMeshGatherer gatherer)
		{
			AddNavigationGeometryToGatherer(gatherer, center, size, addFloor, navigationHoles);
		};
	}

	private static void AddNavigationGeometryToGatherer(RecastMeshGatherer gatherer, Vector3 center, Vector2 size, bool addFloor, IReadOnlyList<WorldZoneNavigationHoleBakedData> navigationHoles)
	{
		int meshDataIndex = gatherer.AddMeshBuffers(UnitBoxVerts, UnitBoxTris);
		if (addFloor)
		{
			AddRecastFloorToGatherer(gatherer, meshDataIndex, center, size);
		}
		if (navigationHoles != null && navigationHoles.Count > 0)
		{
			AddNavigationHolesToGatherer(gatherer, meshDataIndex, navigationHoles);
		}
	}

	private static void AddRecastFloorToGatherer(RecastMeshGatherer gatherer, int meshDataIndex, Vector3 center, Vector2 size)
	{
		Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(size.x * 0.5f, 0.05f, size.y * 0.5f));
		RecastMeshGatherer.GatheredMesh gatheredMesh = default(RecastMeshGatherer.GatheredMesh);
		gatheredMesh.meshDataIndex = meshDataIndex;
		gatheredMesh.tagDataIndex = -1;
		gatheredMesh.area = 0;
		gatheredMesh.indexStart = 0;
		gatheredMesh.indexEnd = -1;
		gatheredMesh.bounds = default(Bounds);
		gatheredMesh.matrix = matrix;
		gatheredMesh.solid = false;
		gatheredMesh.doubleSided = true;
		gatheredMesh.flatten = false;
		gatheredMesh.areaIsTag = false;
		RecastMeshGatherer.GatheredMesh gatheredMesh2 = gatheredMesh;
		gatheredMesh2.RecalculateBounds();
		gatherer.AddMesh(gatheredMesh2);
	}

	private static void AddNavigationHolesToGatherer(RecastMeshGatherer gatherer, int meshDataIndex, IReadOnlyList<WorldZoneNavigationHoleBakedData> navigationHoles)
	{
		for (int i = 0; i < navigationHoles.Count; i++)
		{
			WorldZoneNavigationHoleBakedData worldZoneNavigationHoleBakedData = navigationHoles[i];
			Matrix4x4 matrix = Matrix4x4.TRS(worldZoneNavigationHoleBakedData.center, worldZoneNavigationHoleBakedData.rotation, worldZoneNavigationHoleBakedData.size * 0.5f);
			RecastMeshGatherer.GatheredMesh gatheredMesh = default(RecastMeshGatherer.GatheredMesh);
			gatheredMesh.meshDataIndex = meshDataIndex;
			gatheredMesh.tagDataIndex = -1;
			gatheredMesh.area = -1;
			gatheredMesh.indexStart = 0;
			gatheredMesh.indexEnd = -1;
			gatheredMesh.bounds = default(Bounds);
			gatheredMesh.matrix = matrix;
			gatheredMesh.solid = true;
			gatheredMesh.doubleSided = false;
			gatheredMesh.flatten = false;
			gatheredMesh.areaIsTag = false;
			RecastMeshGatherer.GatheredMesh gatheredMesh2 = gatheredMesh;
			gatheredMesh2.RecalculateBounds();
			gatherer.AddMesh(gatheredMesh2);
		}
	}

	public void AddCutUnit(SGuid holder, LazyConsts.Navigation.Graph graph, Vector3 center, Vector2 size)
	{
		AddCutUnit(holder, NavigationGraphMaskUtils.ToCutGraphMask(graph), center, size);
	}

	public void AddCutUnit(SGuid holder, LazyConsts.Navigation.Graph graph, Rect rect, float y)
	{
		AddCutUnit(holder, graph, new Vector3(rect.center.x, y, rect.center.y), rect.size);
	}

	public void AddCutUnit(SGuid holder, Rect rect, float y)
	{
		AddCutUnit(holder, GraphMask.everything, new Vector3(rect.center.x, y, rect.center.y), rect.size);
	}

	public void RemoveCutUnit(SGuid holder)
	{
		bool flag = false;
		if (usedGraphUpdateSceneUnits.TryGetValue(holder, out var value))
		{
			value.Release();
			usedGraphUpdateSceneUnits.Remove(holder);
			graphUpdateSceneUnitPool?.ReleaseObject(value);
			flag = true;
		}
		if (usedUnits.TryGetValue(holder, out var value2))
		{
			value2.Release();
			usedUnits.Remove(holder);
			obstacleUnitPool.ReleaseObject(value2);
			flag = true;
		}
		if (usedCustomNavMeshCutUnits.TryGetValue(holder, out var value3))
		{
			value3.Release();
			usedCustomNavMeshCutUnits.Remove(holder);
			customNavMeshCutUnitPool?.ReleaseObject(value3);
			flag = true;
		}
		if (!flag)
		{
			Debug.LogWarning($"Holder {holder} not found in active units");
		}
	}

	public void UpdateCutUnit(SGuid holder, Vector3 center, Vector2 size)
	{
		if (usedUnits.TryGetValue(holder, out var value))
		{
			value.UpdateParameters(center, size);
		}
		else
		{
			Debug.LogWarning($"Holder {holder} not found in usedUnits");
		}
	}

	public void UpdateCutUnit(SGuid holder, Rect rect, float y)
	{
		UpdateCutUnit(holder, new Vector3(rect.center.x, y, rect.center.y), rect.size);
	}

	public void ClearAll()
	{
		foreach (GraphCutUnit item in new List<GraphCutUnit>(usedUnits.Values))
		{
			RemoveCutUnit(item.holder);
		}
		foreach (GraphUpdateSceneUnit item2 in new List<GraphUpdateSceneUnit>(usedGraphUpdateSceneUnits.Values))
		{
			RemoveCutUnit(item2.holder);
		}
		foreach (GraphCustomNavMeshCutUnit item3 in new List<GraphCustomNavMeshCutUnit>(usedCustomNavMeshCutUnits.Values))
		{
			RemoveCutUnit(item3.holder);
		}
	}

	public void CalculatePath(LazyConsts.Navigation.Graph graph, Vector3 start, Vector3 end, Action<Path> onPathComplete)
	{
		if (graph != LazyConsts.Navigation.Graph.None)
		{
			CalculatePath(GraphMask.FromGraphIndex((uint)graph), start, end, onPathComplete);
		}
	}

	public void CalculatePath(GraphMask graphMask, Vector3 start, Vector3 end, Action<Path> onPathComplete)
	{
		if (!(graphMask == default(GraphMask)))
		{
			PathCalculationUnit pathCalculationUnit = pathCalculationUnitPool.GetOrCreateObject<PathCalculationUnit>();
			pathCalculationUnit.transform.position = start;
			pathCalculationUnit.seeker.graphMask = graphMask;
			pathCalculationUnit.seeker.StartPath(start, end, delegate(Path p)
			{
				pathCalculationUnit.VectorPath = p.vectorPath;
				pathCalculationUnitPool.ReleaseObject(pathCalculationUnit);
				onPathComplete?.Invoke(p);
			}, graphMask);
			Debug.Log($"[GlobalNavigationManager] Path started to calculate for graphs [{GraphMaskToReadableString(graphMask)}] from {start} to {end}");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		astarPath = AstarPath.active;
		obstacleUnitPool = new Pool(graphCutUnitPrefab, graphCutUnitPrefab.transform.parent, 10);
		if (graphCustomNavMeshCutUnitPrefab != null)
		{
			customNavMeshCutUnitPool = new Pool(graphCustomNavMeshCutUnitPrefab, graphCustomNavMeshCutUnitPrefab.transform.parent, 10);
		}
		if (graphUpdateSceneUnitPrefab != null)
		{
			graphUpdateSceneUnitPool = new Pool(graphUpdateSceneUnitPrefab, graphUpdateSceneUnitPrefab.transform.parent, 10);
		}
		recastFloorPool = new Pool(recastFloorPrefab, recastFloorPrefab.transform.parent, 1);
		pathCalculationUnitPool = new Pool(pathCalculationUnitPrefab, pathCalculationUnitPrefab.transform.parent, 10);
		graphCutUnitPrefab.gameObject.SetActive(value: false);
		if (graphCustomNavMeshCutUnitPrefab != null)
		{
			graphCustomNavMeshCutUnitPrefab.gameObject.SetActive(value: false);
		}
		if (graphUpdateSceneUnitPrefab != null)
		{
			graphUpdateSceneUnitPrefab.gameObject.SetActive(value: false);
		}
		recastFloorPrefab.gameObject.SetActive(value: false);
		pathCalculationUnitPrefab.gameObject.SetActive(value: false);
	}

	public void AddCutUnit(SGuid holder, LazyConsts.Navigation.Graph graph, Vector3 center, float radius)
	{
		AddCutUnit(holder, NavigationGraphMaskUtils.ToCutGraphMask(graph), center, radius);
	}

	public void AddCutUnit(SGuid holder, LazyConsts.Navigation.Graph graph, Vector3 center, WgoPartBakedData.PlannerMeshData plannerMeshData)
	{
		AddCutUnit(holder, NavigationGraphMaskUtils.ToCutGraphMask(graph), center, plannerMeshData);
	}

	public void AddCutUnit(SGuid holder, Vector3 center, WgoPartBakedData.PlannerMeshData plannerMeshData)
	{
		AddCutUnit(holder, GraphMask.everything, center, plannerMeshData);
	}

	public void AddGraphSceneUpdateUnit(SGuid holder, LazyConsts.Navigation.Graph graph, Vector3 center, WgoPartBakedData.GraphUpdateSceneBoxData graphUpdateSceneBoxData)
	{
		AddGraphSceneUpdateUnit(holder, NavigationGraphMaskUtils.ToCutGraphMask(graph), center, graphUpdateSceneBoxData);
	}

	public void AddGraphSceneUpdateUnit(SGuid holder, Vector3 center, WgoPartBakedData.GraphUpdateSceneBoxData graphUpdateSceneBoxData)
	{
		AddGraphSceneUpdateUnit(holder, GraphMask.everything, center, graphUpdateSceneBoxData);
	}

	public void AddCustomNavMeshCutUnit(SGuid holder, Vector3 center, Vector3 scale, WgoData wgoData)
	{
		if (customNavMeshCutUnitPool == null)
		{
			Debug.LogWarning("CustomNavMeshCutUnit pool is not initialized. Assign graphCustomNavMeshCutUnitPrefab in GlobalNavigationManager.");
		}
		else
		{
			if (!GraphCustomNavMeshCutUnit.HasBakedCustomNavMeshCuts(wgoData))
			{
				return;
			}
			Debug.Log($"[GraphCustomNavMeshCutUnit] Rebuild for WGO [{wgoData.id}] holder [{holder}]");
			if (usedCustomNavMeshCutUnits.TryGetValue(holder, out var value))
			{
				value.UpdateParameters(center, scale, wgoData);
				return;
			}
			GraphCustomNavMeshCutUnit orCreateObject = customNavMeshCutUnitPool.GetOrCreateObject<GraphCustomNavMeshCutUnit>();
			orCreateObject.Assign(holder);
			orCreateObject.UpdateParameters(center, scale, wgoData);
			if (!usedCustomNavMeshCutUnits.TryAdd(holder, orCreateObject))
			{
				Debug.LogWarning($"Holder {holder} already exists in usedCustomNavMeshCutUnits");
				customNavMeshCutUnitPool.ReleaseObject(orCreateObject);
			}
		}
	}

	public void UpdateCustomNavMeshCutUnitTransform(SGuid holder, Vector3 center, Vector3 scale)
	{
		if (usedCustomNavMeshCutUnits.TryGetValue(holder, out var value))
		{
			value.UpdateTransform(center, scale);
		}
	}

	public void RemoveCustomNavMeshCutUnit(SGuid holder)
	{
		if (usedCustomNavMeshCutUnits.TryGetValue(holder, out var value))
		{
			value.Release();
			usedCustomNavMeshCutUnits.Remove(holder);
			customNavMeshCutUnitPool?.ReleaseObject(value);
		}
	}

	private void AddCutUnit(SGuid holder, GraphMask graphMask, Vector3 center, float radius)
	{
		GraphCutUnit orCreateObject = obstacleUnitPool.GetOrCreateObject<GraphCutUnit>();
		orCreateObject.Assign(holder);
		orCreateObject.UpdateParameters(graphMask, center, radius);
		if (!usedUnits.TryAdd(holder, orCreateObject))
		{
			Debug.LogWarning($"Holder {holder} already exists in usedUnits");
			obstacleUnitPool.ReleaseObject(orCreateObject);
		}
	}

	private void AddCutUnit(SGuid holder, GraphMask graphMask, Vector3 center, WgoPartBakedData.PlannerMeshData plannerMeshData)
	{
		GraphCutUnit orCreateObject = obstacleUnitPool.GetOrCreateObject<GraphCutUnit>();
		orCreateObject.Assign(holder);
		orCreateObject.UpdateParameters(graphMask, center, plannerMeshData);
		if (!usedUnits.TryAdd(holder, orCreateObject))
		{
			Debug.LogWarning($"Holder {holder} already exists in usedUnits");
			obstacleUnitPool.ReleaseObject(orCreateObject);
		}
	}

	private void AddGraphSceneUpdateUnit(SGuid holder, GraphMask graphMask, Vector3 center, WgoPartBakedData.GraphUpdateSceneBoxData graphUpdateSceneBoxData)
	{
		if (graphUpdateSceneUnitPool == null)
		{
			Debug.LogWarning("GraphUpdateSceneUnit pool is not initialized. Assign graphUpdateSceneUnitPrefab in GlobalNavigationManager.");
			return;
		}
		GraphUpdateSceneUnit orCreateObject = graphUpdateSceneUnitPool.GetOrCreateObject<GraphUpdateSceneUnit>();
		orCreateObject.Assign(holder);
		orCreateObject.UpdateParameters(graphMask, center, graphUpdateSceneBoxData);
		if (!usedGraphUpdateSceneUnits.TryAdd(holder, orCreateObject))
		{
			Debug.LogWarning($"Holder {holder} already exists in usedUnits");
			graphUpdateSceneUnitPool.ReleaseObject(orCreateObject);
		}
	}

	private void AddCutUnit(SGuid holder, GraphMask graphMask, Vector3 center, Vector2 size)
	{
		GraphCutUnit orCreateObject = obstacleUnitPool.GetOrCreateObject<GraphCutUnit>();
		orCreateObject.Assign(holder);
		orCreateObject.UpdateParameters(graphMask, center, size);
		if (!usedUnits.TryAdd(holder, orCreateObject))
		{
			Debug.LogWarning($"Holder {holder} already exists in usedUnits");
			obstacleUnitPool.ReleaseObject(orCreateObject);
		}
	}

	private string GraphMaskToReadableString(GraphMask graphMask)
	{
		if (astarPath?.graphs == null)
		{
			return graphMask.ToString();
		}
		List<string> list = new List<string>();
		for (int i = 0; i < astarPath.graphs.Length; i++)
		{
			NavGraph navGraph = astarPath.graphs[i];
			if (navGraph != null && graphMask.Contains((uint)i))
			{
				string text;
				if (!string.IsNullOrEmpty(navGraph.name))
				{
					text = navGraph.name;
				}
				else
				{
					LazyConsts.Navigation.Graph graph = (LazyConsts.Navigation.Graph)i;
					text = graph.ToString();
				}
				string arg = text;
				list.Add($"{i}:{arg}");
			}
		}
		if (list.Count != 0)
		{
			return string.Join(", ", list);
		}
		return "<none>";
	}
}
