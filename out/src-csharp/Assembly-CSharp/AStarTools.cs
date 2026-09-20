using Pathfinding;
using UnityEngine;

public static class AStarTools
{
	private const int SMALL_ASTAR_NODE_SIZE = 8;

	private const float SMALL_ASTAR_PLAYER_DIAMETER = 3.4f;

	public const int PLAYER_GRAPH_N = 2;

	public static void Rescan()
	{
		AstarPath.active.Scan();
	}

	public static void RescanTile(Vector2 pos, int size = 1)
	{
		UpdateAstarBounds(new Bounds(pos, Vector2.one * (size * 96)));
	}

	public static Vector2 ToVector2(this Int3 value)
	{
		return new Vector2((float)value.x / 1000f, (float)value.y / 1000f);
	}

	public static void RefreshPlayerGraph(Vector2 from, Vector2 to)
	{
		RefreshGraph(2, from, to);
	}

	private static void RefreshGraph(int graph_n, Vector2 from, Vector2 to)
	{
		Vector2 vector = (from + to) / 2f;
		Vector2 size = new Vector2(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
		DebugDraw.DrawBox(vector, size, Color.magenta);
		if (AstarPath.active.graphs.Length <= graph_n)
		{
			Debug.LogWarning("Can't refresh graph #" + graph_n + " because current graphs length = " + AstarPath.active.graphs.Length);
			return;
		}
		GridGraph gridGraph = AstarPath.active.graphs[graph_n] as GridGraph;
		gridGraph.nodeSize = 8f;
		size += Vector2.one * 25f * gridGraph.nodeSize;
		vector.x = Mathf.Round(vector.x / (gridGraph.nodeSize * 2f)) * gridGraph.nodeSize * 2f;
		vector.y = Mathf.Round(vector.y / (gridGraph.nodeSize * 2f)) * gridGraph.nodeSize * 2f;
		gridGraph.center = vector;
		gridGraph.width = Mathf.CeilToInt(size.x / gridGraph.nodeSize / 2f) * 2;
		gridGraph.depth = Mathf.CeilToInt(size.y / gridGraph.nodeSize / 2f) * 2;
		gridGraph.collision.diameter = 3.4f;
		gridGraph.UpdateSizeFromWidthDepth();
		AstarPath.active.Scan(graph_n);
	}

	public static void UpdateAstarBounds(Vector2 coord1, Vector2 coord2)
	{
		float num = ((coord1.x > coord2.x) ? (coord1.x - coord2.x) : (coord2.x - coord1.x));
		float num2 = ((coord1.y > coord2.y) ? (coord1.y - coord2.y) : (coord2.y - coord1.y));
		Vector2 obj = ((coord1.x < coord2.x) ? coord1 : coord2);
		float x = obj.x + num / 2f;
		Vector2 obj2 = ((coord1.y < coord2.y) ? coord1 : coord2);
		float y = obj2.y + num2 / 2f;
		num += 192f;
		num2 += 192f;
		UpdateAstarBounds(new Bounds(new Vector2(x, y), new Vector2(num, num2)));
	}

	public static void UpdateAstarBounds(Bounds bounds)
	{
		GraphUpdateObject ob = new GraphUpdateObject(bounds)
		{
			only_specific_graph = 0
		};
		AstarPath.active.UpdateGraphs(ob);
		GraphUpdateObject ob2 = new GraphUpdateObject(bounds)
		{
			only_specific_graph = 2
		};
		AstarPath.active.UpdateGraphs(ob2);
	}

	public static void InitialAstarScan()
	{
		if (!(AstarPath.active.graphs[0] is GridGraph gridGraph))
		{
			Debug.LogError("GridGraph not found, total graphs = " + AstarPath.active.graphs.Length);
			return;
		}
		_ = gridGraph.Width;
		_ = gridGraph.Depth;
		gridGraph.collision.collisionCheck = false;
		AstarPath.active.graphs[0] = gridGraph;
		for (int i = 0; i < AstarPath.active.graphs.Length; i++)
		{
			AstarPath.active.Scan(i);
		}
		gridGraph.collision.collisionCheck = true;
		AstarPath.active.graphs[0] = gridGraph;
	}
}
