using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

public class AStarScanScheduler : LazySingleton<AStarScanScheduler>
{
	private class ScanItem
	{
		public NavGraph graph;

		public Action onStarted;

		public Action onCompleted;

		private IEnumerable<Progress> scanProgress;

		public ScanItem(NavGraph graph, Action onCompleted, Action onStarted = null)
		{
			this.graph = graph;
			this.onStarted = onStarted;
			this.onCompleted = onCompleted;
		}

		public void StartScan()
		{
			scanProgress = AstarPath.active.ScanAsync(graph);
			onStarted?.Invoke();
		}

		public IEnumerable<Progress> GetScanProgress()
		{
			return scanProgress ?? Enumerable.Empty<Progress>();
		}
	}

	private Queue<ScanItem> scanQueue = new Queue<ScanItem>();

	private bool isScanningInProgress;

	public IEnumerable<Progress> ScanGraphAsync(NavGraph graph)
	{
		bool isCompleted = false;
		bool hasStartedScanning = false;
		ScanItem item = new ScanItem(graph, delegate
		{
			isCompleted = true;
		}, delegate
		{
			hasStartedScanning = true;
		});
		scanQueue.Enqueue(item);
		if (!isScanningInProgress)
		{
			StartCoroutine(ProcessScanQueue());
		}
		while (!hasStartedScanning && !isCompleted)
		{
			yield return new Progress(0f, ScanningStage.PreProcessingGraphs);
		}
		if (!hasStartedScanning || isCompleted)
		{
			yield break;
		}
		foreach (Progress item2 in item.GetScanProgress())
		{
			yield return item2;
		}
	}

	public void ScanGraph(NavGraph graph)
	{
		AstarPath.active.Scan(graph);
	}

	public void ClearScanQueue()
	{
		int count = scanQueue.Count;
		scanQueue.Clear();
		Debug.LogWarning($"[SCAN QUEUE] Cleared {count} pending scans from queue");
	}

	private IEnumerator ProcessScanQueue()
	{
		isScanningInProgress = true;
		while (scanQueue.Count > 0)
		{
			if (AstarPath.active.isScanning)
			{
				yield return null;
			}
			ScanItem request = scanQueue.Dequeue();
			Debug.Log($"[SCAN QUEUE] Processing scan for graph {request.graph}. Queue remaining: {scanQueue.Count}");
			request.StartScan();
			foreach (Progress item in request.GetScanProgress())
			{
				_ = item;
				yield return null;
			}
			request.onCompleted?.Invoke();
			Debug.Log($"[SCAN QUEUE] Completed scan for graph {request.graph}");
			yield return new WaitForEndOfFrame();
		}
		isScanningInProgress = false;
		Debug.Log("[SCAN QUEUE] All scans completed. Queue is empty.");
	}
}
