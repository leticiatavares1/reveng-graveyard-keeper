using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class GDPointGraph : PointGraph, IUpdatableGraph
{
	public override void ScanInternal(OnScanStatus status_callback)
	{
		nodes = new PointNode[0];
		base.nodeCount = 0;
		if (!Application.isPlaying)
		{
			MainGame.me = Object.FindObjectOfType<MainGame>();
			MainGame.me.world = Object.FindObjectOfType<World>();
		}
		if (MainGame.me == null || MainGame.me.world == null)
		{
			Debug.Log("GDPointGraph: ScanInternal skipping, has no world");
			return;
		}
		GDPoint[] componentsInChildren = MainGame.me.world.GetComponentsInChildren<GDPoint>(includeInactive: true);
		GDPoint[] array = componentsInChildren;
		foreach (GDPoint obj in array)
		{
			obj.is_graph_waypoint = false;
			obj.DeInitNode();
		}
		array = componentsInChildren;
		foreach (GDPoint gDPoint in array)
		{
			if (gDPoint.next_gd_points.Count > 0)
			{
				gDPoint.is_graph_waypoint = true;
			}
			foreach (GDPoint next_gd_point in gDPoint.next_gd_points)
			{
				if (next_gd_point == null)
				{
					Debug.LogError("GDPoint " + gDPoint.name + " has a null next_gd_point!", gDPoint);
				}
				else
				{
					next_gd_point.is_graph_waypoint = true;
				}
			}
		}
		array = componentsInChildren;
		foreach (GDPoint gDPoint2 in array)
		{
			if (gDPoint2.is_graph_waypoint)
			{
				Int3 position = (Int3)gDPoint2.gameObject.transform.position;
				if (position.z < 1000)
				{
					position.z = 0;
				}
				gDPoint2.InitNode();
				AddNode(gDPoint2.node, position);
			}
		}
		Debug.Log("GDPointGraph: ScanInternal finished correctly! Nodes count: " + componentsInChildren.Length + " (" + base.nodeCount + "/" + nodes.Length + ")");
	}

	public new void UpdateArea(GraphUpdateObject o)
	{
	}

	public new void UpdateAreaInit(GraphUpdateObject o)
	{
	}

	public void UpdateGraph(List<GDPoint> gd_points_input)
	{
		nodes = new PointNode[0];
		base.nodeCount = 0;
		foreach (GDPoint item in gd_points_input)
		{
			item.is_graph_waypoint = false;
			item.DeInitNode();
		}
		foreach (GDPoint item2 in gd_points_input)
		{
			if (item2.next_gd_points.Count > 0)
			{
				item2.is_graph_waypoint = true;
			}
			foreach (GDPoint next_gd_point in item2.next_gd_points)
			{
				if (next_gd_point == null)
				{
					Debug.LogError("GDPoint " + item2.name + " has a null next_gd_point!", item2);
				}
				else
				{
					next_gd_point.is_graph_waypoint = true;
				}
			}
		}
		foreach (GDPoint item3 in gd_points_input)
		{
			if (item3.is_graph_waypoint)
			{
				Int3 position = (Int3)item3.gameObject.transform.position;
				if (position.z < 1000)
				{
					position.z = 0;
				}
				item3.InitNode();
				AddNode(item3.node, position);
			}
		}
		Debug.Log("GDPointGraph: UpdateGraph finished correctly! Nodes count: " + gd_points_input.Count + " (" + base.nodeCount + "/" + nodes.Length + ")");
	}
}
