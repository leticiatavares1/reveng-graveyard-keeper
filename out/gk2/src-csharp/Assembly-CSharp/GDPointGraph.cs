using System;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Serialization;
using Pathfinding.Util;
using Unity.Jobs;
using UnityEngine;

[JsonOptIn]
[Preserve]
public class GDPointGraph : PointGraph, IUpdatableGraph
{
	private class GDPointGraphScanPromise : IGraphUpdatePromise
	{
		private GDPointGraph graph;

		public float Progress => 1f;

		public GDPointGraphScanPromise(GDPointGraph graph)
		{
			this.graph = graph;
		}

		public IEnumerator<JobHandle> Prepare()
		{
			yield break;
		}

		public void Apply(IGraphUpdateContext ctx)
		{
			graph.DestroyAllNodes();
			if (MainGame.Instance == null)
			{
				Debug.Log("GDPointGraph: no MainGame instance, skipping scan");
				return;
			}
			List<GDPointData> points = MainGame.Instance.GameSave.worldData.gdPointsData.Points;
			if (points == null || points.Count == 0)
			{
				Debug.Log("GDPointGraph: no points data, skipping scan");
				return;
			}
			foreach (GDPointData item in points)
			{
				item.IsGraphPoint = false;
				item.DeInitNode();
			}
			foreach (GDPointData item2 in points)
			{
				if (!item2.Enabled || !item2.IsWaypoint)
				{
					continue;
				}
				if (item2.NextPointData.Count > 0)
				{
					item2.IsGraphPoint = true;
				}
				foreach (GDPointData nextPointDatum in item2.NextPointData)
				{
					if (nextPointDatum.Enabled)
					{
						nextPointDatum.IsGraphPoint = true;
					}
				}
			}
			foreach (GDPointData item3 in points)
			{
				if (item3.IsGraphPoint)
				{
					Int3 position = (Int3)item3.Position;
					item3.InitNode();
					graph.AddNode(item3.Node, position);
				}
			}
			MainGame.Instance.GraphHelper.ResumePathFinding();
			Debug.Log($"GDPointGraph: scan complete. nodeCount = {graph.nodeCount}");
		}
	}

	protected override IGraphUpdatePromise ScanInternal(bool async)
	{
		return new GDPointGraphScanPromise(this);
	}

	public override void GetNodes(Action<GraphNode> action)
	{
		if (nodes != null)
		{
			for (int i = 0; i < base.nodeCount; i++)
			{
				action(nodes[i]);
			}
		}
	}

	protected override void DestroyAllNodes()
	{
		base.DestroyAllNodes();
		base.nodeCount = 0;
	}
}
