using System;
using Pathfinding;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class GraphHelper : MonoBehaviour
{
	private const string GRAPH_PATH = "/AddressableAssets/AStarGraphs/graphReady.bytes";

	private const string CONFIG_LOAD_KEY = "AStarGraphs/SceneGraphsData.asset";

	private const string ASTAR_PREFAB_PATH = "Assets/Prefabs/Systems/AStar.prefab";

	private PathProcessor.GraphUpdateLock graphUpdateLock;

	private bool gdPointGraphRescanQueued;

	private static GameObject astarPrefab;

	private static GraphHelper instance;

	private SceneGraphsData sceneGraphsData;

	public static GraphHelper Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindFirstObjectByType<GraphHelper>();
			}
			return instance;
		}
	}

	public SceneGraphsData SceneGraphsData
	{
		get
		{
			if (sceneGraphsData == null)
			{
				sceneGraphsData = Addressables.LoadAssetAsync<SceneGraphsData>("AStarGraphs/SceneGraphsData.asset").WaitForCompletion();
			}
			return sceneGraphsData;
		}
	}

	public void ScanGDPointGraph()
	{
		GDPointGraph gDPointGraph = (GDPointGraph)AstarPath.active.data.FindGraphOfType(typeof(GDPointGraph));
		if (gDPointGraph != null)
		{
			AstarPath.active.Scan(gDPointGraph);
		}
	}

	public void RescanGDPointGraph()
	{
		PausePathFinding();
		ScanGDPointGraph();
	}

	public void QueueRescanGDPointGraph()
	{
		if (!((UnityEngine.Object)(object)AstarPath.active == null))
		{
			gdPointGraphRescanQueued = true;
		}
	}

	private void LateUpdate()
	{
		if (gdPointGraphRescanQueued)
		{
			gdPointGraphRescanQueued = false;
			if ((UnityEngine.Object)(object)AstarPath.active != null)
			{
				RescanGDPointGraph();
			}
		}
	}

	public void PausePathFinding()
	{
		if (!graphUpdateLock.Held)
		{
			graphUpdateLock = AstarPath.active.PausePathfinding();
		}
	}

	public void ResumePathFinding()
	{
		if (graphUpdateLock.Held)
		{
			graphUpdateLock.Release();
		}
	}
}
