using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[Serializable]
public class GDPointData
{
	[SerializeField]
	private string id;

	[SerializeField]
	private string customTag;

	[SerializeField]
	private int instanceId;

	[SerializeField]
	private List<int> nextNodesInstanceId = new List<int>();

	[NonSerialized]
	private List<GDPointData> nextGdPointsData = new List<GDPointData>();

	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Direction direction;

	[SerializeField]
	private bool isTransitPoint;

	[SerializeField]
	private string transitToGdPointId;

	[SerializeField]
	private string worldIdToTransit;

	[SerializeField]
	private string gameSceneDataId;

	[SerializeField]
	[HideInInspector]
	private bool enabled;

	[SerializeField]
	[HideInInspector]
	private bool isWaypoint;

	private GDPointNode node;

	private bool nodeInited;

	private static AstarPath astarPath;

	private bool isGraphPoint;

	public string Id => id;

	public int InstanceId => instanceId;

	public string CustomTag => customTag;

	public Vector3 Position => position;

	public Direction Direction => direction;

	public bool IsWaypoint => isWaypoint;

	public string GameSceneDataIdToTransit => worldIdToTransit;

	public static AstarPath AStarPath
	{
		get
		{
			if ((UnityEngine.Object)(object)astarPath == null)
			{
				astarPath = AstarPath.active;
			}
			return astarPath;
		}
	}

	public GDPointNode Node => node;

	public bool IsGraphPoint
	{
		get
		{
			return isGraphPoint;
		}
		set
		{
			isGraphPoint = value;
		}
	}

	public string GameSceneDataId => gameSceneDataId;

	public List<GDPointData> NextPointData => nextGdPointsData;

	public IReadOnlyList<int> NextNodeInstanceIds => nextNodesInstanceId;

	public bool IsTransitPoint => isTransitPoint;

	public string TransitToGdPointId => transitToGdPointId;

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
			if (isWaypoint)
			{
				MainGame.Instance.GraphHelper.RescanGDPointGraph();
			}
			if (this.OnActiveStateChanged == null)
			{
				Debug.LogWarning("GDPoint [" + id + "] has no subscribers for OnActiveStateChanged. Scene may not be loaded yet.");
			}
			this.OnActiveStateChanged?.Invoke(enabled);
			Debug.Log($"GDPoint [{id}] Changed Enabled [{enabled}]");
		}
	}

	public event Action<bool> OnActiveStateChanged;

	public GDPointData(GDPoint gdPoint, string gameSceneDataId, Vector3 offset, bool isWaypoint = true)
	{
		id = gdPoint.Id;
		instanceId = gdPoint.GetInstanceID();
		customTag = gdPoint.CustomTag;
		direction = gdPoint.Direction;
		position = gdPoint.transform.position + offset;
		isTransitPoint = gdPoint.IsTransitPoint;
		transitToGdPointId = gdPoint.TransitToGdPointId;
		this.isWaypoint = isWaypoint;
		enabled = gdPoint.gameObject.activeSelf;
		if (isTransitPoint)
		{
			worldIdToTransit = gdPoint.WorldIdToTransit;
			transitToGdPointId = gdPoint.TransitToGdPointId;
		}
		for (int i = 0; i < gdPoint.NextGdPoints.Count; i++)
		{
			GDPoint gDPoint = gdPoint.NextGdPoints[i];
			if (gDPoint == null)
			{
				Debug.LogError("GD point with id " + gdPoint.Id + " has an empty next point");
			}
			else
			{
				nextNodesInstanceId.Add(gDPoint.GetInstanceID());
			}
		}
		this.gameSceneDataId = gameSceneDataId;
	}

	public GDPointData(WgoPartBakedData.GDPointBakedData bakedData, string gameSceneDataId, Vector3 worldPosition, Vector3 sceneOffset, int syntheticInstanceId)
	{
		id = bakedData.id;
		instanceId = syntheticInstanceId;
		customTag = bakedData.customTag;
		direction = bakedData.direction;
		position = worldPosition + bakedData.localPosition + sceneOffset;
		isTransitPoint = bakedData.isTransitPoint;
		transitToGdPointId = bakedData.transitToGdPointId;
		worldIdToTransit = bakedData.worldIdToTransit;
		isWaypoint = false;
		enabled = bakedData.enabled;
		this.gameSceneDataId = gameSceneDataId;
	}

	public GDPointData(GDPointData otherData)
	{
		id = otherData.Id;
		instanceId = otherData.InstanceId;
		customTag = otherData.CustomTag;
		direction = otherData.Direction;
		position = otherData.Position;
		isTransitPoint = otherData.IsTransitPoint;
		transitToGdPointId = otherData.TransitToGdPointId;
		isWaypoint = otherData.isWaypoint;
		enabled = otherData.Enabled;
		if (isTransitPoint)
		{
			worldIdToTransit = otherData.worldIdToTransit;
			transitToGdPointId = otherData.TransitToGdPointId;
		}
		for (int i = 0; i < otherData.nextNodesInstanceId.Count; i++)
		{
			int item = otherData.nextNodesInstanceId[i];
			nextNodesInstanceId.Add(item);
		}
		gameSceneDataId = otherData.GameSceneDataId;
	}

	public GDPointData(string id, string customTag, int instanceId, Vector3 position, Direction direction, string gameSceneDataId, bool isWaypoint, bool enabled)
	{
		this.id = id;
		this.customTag = customTag;
		this.instanceId = instanceId;
		this.position = position;
		this.direction = direction;
		this.gameSceneDataId = gameSceneDataId;
		this.isWaypoint = isWaypoint;
		this.enabled = enabled;
		nextNodesInstanceId = new List<int>();
		nextGdPointsData = new List<GDPointData>();
	}

	public void SetEnabledStateSilent(bool enabled)
	{
		this.enabled = enabled;
	}

	public void SetNextNodeInstanceIds(List<int> ids)
	{
		nextNodesInstanceId = ids ?? new List<int>();
		nextGdPointsData = new List<GDPointData>();
	}

	public void AddNextNodeInstanceId(int nextInstanceId)
	{
		if (nextNodesInstanceId == null)
		{
			nextNodesInstanceId = new List<int>();
		}
		if (nextNodesInstanceId.Contains(nextInstanceId))
		{
			return;
		}
		nextNodesInstanceId.Add(nextInstanceId);
		GDPointData gDPointData = MainGame.Instance?.GameSave?.worldData?.gdPointsData?.GetGDPointDataByInstanceId(nextInstanceId);
		if (gDPointData != null)
		{
			if (nextGdPointsData == null)
			{
				nextGdPointsData = new List<GDPointData>();
			}
			if (!nextGdPointsData.Contains(gDPointData))
			{
				nextGdPointsData.Add(gDPointData);
			}
		}
	}

	public void RemoveNextNodeInstanceId(int nextInstanceId)
	{
		nextNodesInstanceId?.Remove(nextInstanceId);
		nextGdPointsData?.RemoveAll((GDPointData point) => point.InstanceId == nextInstanceId);
	}

	public void LinkNextGdPointsData()
	{
		nextGdPointsData = new List<GDPointData>();
		for (int i = 0; i < nextNodesInstanceId.Count; i++)
		{
			int num = nextNodesInstanceId[i];
			GDPointData gDPointDataByInstanceId = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataByInstanceId(num);
			if (gDPointDataByInstanceId != null)
			{
				nextGdPointsData.Add(gDPointDataByInstanceId);
			}
		}
		if (isTransitPoint)
		{
			GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(transitToGdPointId);
			if (gDPointDataById != null)
			{
				nextGdPointsData.Add(gDPointDataById);
			}
		}
	}

	public void InitNode()
	{
		if (nodeInited)
		{
			return;
		}
		nodeInited = true;
		node = new GDPointNode(AStarPath, this);
		node.GraphIndex = 2u;
		node.position = (Int3)position;
		if (nextGdPointsData.Count == 0)
		{
			return;
		}
		foreach (GDPointData nextGdPointsDatum in nextGdPointsData)
		{
			if (nextGdPointsDatum.IsGraphPoint)
			{
				nextGdPointsDatum.InitNode();
				node.AddConnection(nextGdPointsDatum.node, (uint)Mathf.CeilToInt(Distance(nextGdPointsDatum)));
			}
		}
	}

	public void DeInitNode()
	{
		nodeInited = false;
		node = null;
	}

	public void TryCreateOutsideConnections()
	{
		if (isTransitPoint && !string.IsNullOrEmpty(transitToGdPointId))
		{
			GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(transitToGdPointId);
			if (gDPointDataById != null)
			{
				node.AddConnection(gDPointDataById.Node, (uint)Mathf.CeilToInt(Distance(gDPointDataById)));
			}
		}
	}

	public static float Distance(GDPointData from, GDPointData to)
	{
		if (from == null || to == null)
		{
			return 0f;
		}
		return Vector3.Distance(from.position, to.position);
	}

	public float Distance(GDPointData to)
	{
		return Distance(this, to);
	}
}
