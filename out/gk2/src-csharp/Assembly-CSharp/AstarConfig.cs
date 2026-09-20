using Pathfinding;
using UnityEngine;

public class AstarConfig : MonoBehaviour
{
	[SerializeField]
	private AstarPath astarPath;

	[SerializeField]
	private string defaultGraphId;

	[Space(20f)]
	[Space(10f)]
	[SerializeField]
	private string newGraphId = string.Empty;

	[SerializeField]
	private string graphToCopyId = string.Empty;

	[Space(10f)]
	[Space(10f)]
	[SerializeField]
	private string copyFromId = string.Empty;

	[SerializeField]
	private string copyToId = string.Empty;

	private AstarData data;

	private RecastGraph newGraph;

	private void CreateGraph()
	{
		if ((Object)(object)astarPath == null)
		{
			Debug.LogError("Empty AstarPath field");
		}
		else if (string.IsNullOrEmpty(defaultGraphId))
		{
			Debug.LogError("Empty DefaultGraphId field");
		}
		else if (SearchGraph(defaultGraphId) != -1)
		{
			data = astarPath.data;
			newGraph = data.AddGraph(typeof(RecastGraph)) as RecastGraph;
			newGraph.name = newGraphId;
			Copy(newGraph, data.graphs[SearchGraph(defaultGraphId)] as RecastGraph);
			Debug.Log(newGraphId + " graph created with default parameters");
		}
	}

	private void CopyGraph()
	{
		if ((Object)(object)astarPath == null)
		{
			Debug.LogError("Empty AstarPath field");
			return;
		}
		data = astarPath.data;
		if (SearchGraph(graphToCopyId) != -1)
		{
			newGraph = data.AddGraph(typeof(RecastGraph)) as RecastGraph;
			newGraph.name = newGraphId;
			Copy(newGraph, data.graphs[SearchGraph(graphToCopyId)] as RecastGraph);
			Debug.Log(newGraphId + " graph created with parameters of " + graphToCopyId);
		}
	}

	private void CopyParams()
	{
		if ((Object)(object)astarPath == null)
		{
			Debug.LogError("Empty AstarPath field");
			return;
		}
		data = astarPath.data;
		if (SearchGraph(copyToId) != -1 && SearchGraph(copyFromId) != -1)
		{
			Copy(data.graphs[SearchGraph(copyToId)] as RecastGraph, data.graphs[SearchGraph(copyFromId)] as RecastGraph);
			Debug.Log(copyToId + " parameters from " + copyFromId);
		}
	}

	private int SearchGraph(string graphId)
	{
		if ((Object)(object)astarPath == null)
		{
			Debug.LogError("Empty AstarPath field");
			return -1;
		}
		data = astarPath.data;
		for (int i = 0; i < data.graphs.Length; i++)
		{
			if (data.graphs[i].name == graphId)
			{
				return i;
			}
		}
		Debug.LogError(graphId + " graph not found");
		return -1;
	}

	private void Copy(RecastGraph copyTo, RecastGraph copyFrom)
	{
		copyTo.cellSize = copyFrom.cellSize;
		copyTo.useTiles = copyFrom.useTiles;
		copyTo.editorTileSize = copyFrom.editorTileSize;
		copyTo.minRegionSize = copyFrom.minRegionSize;
		copyTo.walkableHeight = copyFrom.walkableHeight;
		copyTo.walkableClimb = copyFrom.walkableClimb;
		copyTo.characterRadius = copyFrom.characterRadius;
		copyTo.maxSlope = copyFrom.maxSlope;
		copyTo.maxEdgeLength = copyFrom.maxEdgeLength;
		copyTo.contourMaxError = copyFrom.contourMaxError;
		copyTo.rasterizeTerrain = copyFrom.rasterizeTerrain;
		copyTo.rasterizeMeshes = copyFrom.rasterizeMeshes;
		copyTo.rasterizeColliders = copyFrom.rasterizeColliders;
		copyTo.colliderRasterizeDetail = copyFrom.colliderRasterizeDetail;
		copyTo.mask = copyFrom.mask;
		copyTo.enableNavmeshCutting = copyFrom.enableNavmeshCutting;
		copyTo.nearestSearchOnlyXZ = copyFrom.nearestSearchOnlyXZ;
		copyTo.initialPenalty = copyFrom.initialPenalty;
	}
}
