using System.Collections.Generic;
using UnityEngine;

public class TeleportPointGraph
{
	private readonly List<Vector3> positions = new List<Vector3>();

	private readonly List<WgoData> nodeWgos = new List<WgoData>();

	private readonly List<List<int>> edges = new List<List<int>>();

	private readonly List<List<int>> partners = new List<List<int>>();

	private readonly List<int> areaIndex = new List<int>();

	private readonly List<IndoorAreaData> indoorAreas = new List<IndoorAreaData>();

	private readonly List<List<int>> outdoorMouthsByArea = new List<List<int>>();

	private readonly Dictionary<string, int> indexById = new Dictionary<string, int>();

	public static TeleportPointGraph Instance { get; private set; }

	public static void Build(WorldData worldData)
	{
		Instance = new TeleportPointGraph();
		Instance.BuildInternal(worldData);
	}

	public bool TryResolveDoor(Vector3 playerPos, Vector3 objectPos, out WgoData doorWgo)
	{
		return TryResolveDoor(playerPos, objectPos, null, out doorWgo);
	}

	public bool TryResolveDoor(Vector3 playerPos, Vector3 objectPos, WgoData targetObject, out WgoData doorWgo)
	{
		doorWgo = null;
		int playerArea;
		int objectArea;
		string branch;
		int num = ResolveDoorIndex(playerPos, objectPos, out playerArea, out objectArea, out branch);
		if (num < 0)
		{
			return false;
		}
		doorWgo = nodeWgos[num];
		return doorWgo != null;
	}

	private int ResolveDoorIndex(Vector3 playerPos, Vector3 objectPos, out int playerArea, out int objectArea, out string branch)
	{
		playerArea = -1;
		objectArea = -1;
		branch = "empty-graph";
		if (positions.Count == 0)
		{
			return -1;
		}
		playerArea = FindAreaIndex(playerPos);
		objectArea = FindAreaIndex(objectPos);
		if (playerArea >= 0 && playerArea == objectArea)
		{
			branch = "same-indoor-area";
			return -1;
		}
		int result;
		if (playerArea >= 0)
		{
			int num = ((objectArea >= 0) ? BfsIndoorOnlyToArea(playerPos, objectArea, playerArea) : (-1));
			if (num >= 0)
			{
				result = num;
				branch = "indoor-connected";
			}
			else
			{
				result = FindDoorTowardStreet(playerPos, playerArea);
				branch = "indoor-to-street";
			}
		}
		else
		{
			if (objectArea < 0)
			{
				branch = "outdoor-to-outdoor";
				return -1;
			}
			result = FindOutdoorEntranceForArea(playerPos, objectArea);
			branch = "outdoor-to-object-entrance";
		}
		return result;
	}

	private void BuildInternal(WorldData worldData)
	{
		if (worldData == null || GameBalance.Me == null)
		{
			return;
		}
		LoadIndoorAreas();
		List<WGODef> dataCollection = GameBalance.Me.GetDataCollection<WGODef>();
		if (dataCollection == null)
		{
			return;
		}
		int num = 0;
		foreach (WGODef item in dataCollection)
		{
			if (item == null || string.IsNullOrEmpty(item.id) || item.teleportDestinationWgoIds == null || item.teleportDestinationWgoIds.Count == 0)
			{
				continue;
			}
			WgoData wgoData = ResolveWgo(worldData, item.id);
			if (wgoData == null)
			{
				continue;
			}
			int orAddNode = GetOrAddNode(wgoData);
			foreach (string teleportDestinationWgoId in item.teleportDestinationWgoIds)
			{
				if (string.IsNullOrEmpty(teleportDestinationWgoId) || teleportDestinationWgoId == item.id)
				{
					continue;
				}
				WgoData wgoData2 = ResolveWgo(worldData, teleportDestinationWgoId);
				if (wgoData2 == null)
				{
					continue;
				}
				int orAddNode2 = GetOrAddNode(wgoData2);
				if (orAddNode != orAddNode2)
				{
					List<int> list = edges[orAddNode];
					if (!list.Contains(orAddNode2))
					{
						list.Add(orAddNode2);
						num++;
					}
				}
			}
		}
		BuildPartnerLists();
		AssignNodeAreas();
	}

	private void LoadIndoorAreas()
	{
		indoorAreas.Clear();
		List<GameSceneConfig> list = ((MainGame.Instance != null) ? MainGame.Instance.gameSceneConfigs : null);
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			GameSceneConfig gameSceneConfig = list[i];
			IReadOnlyList<IndoorAreaData> readOnlyList = ((gameSceneConfig != null) ? gameSceneConfig.IndoorAreas : null);
			if (readOnlyList == null)
			{
				continue;
			}
			for (int j = 0; j < readOnlyList.Count; j++)
			{
				IndoorAreaData indoorAreaData = readOnlyList[j];
				if (indoorAreaData != null)
				{
					indoorAreas.Add(indoorAreaData);
				}
			}
		}
	}

	private void AssignNodeAreas()
	{
		for (int i = 0; i < positions.Count; i++)
		{
			areaIndex[i] = FindAreaIndex(positions[i]);
		}
		CacheOutdoorMouths();
	}

	private void CacheOutdoorMouths()
	{
		outdoorMouthsByArea.Clear();
		for (int i = 0; i < indoorAreas.Count; i++)
		{
			List<int> list = new List<int>();
			CollectOutdoorMouths(i, list);
			outdoorMouthsByArea.Add(list);
		}
	}

	private void CollectOutdoorMouths(int area, List<int> mouths)
	{
		int count = positions.Count;
		bool[] array = new bool[count];
		Queue<int> queue = new Queue<int>();
		for (int i = 0; i < count; i++)
		{
			if (areaIndex[i] == area)
			{
				array[i] = true;
				queue.Enqueue(i);
			}
		}
		while (queue.Count > 0)
		{
			int index = queue.Dequeue();
			List<int> list = partners[index];
			for (int j = 0; j < list.Count; j++)
			{
				int num = list[j];
				if (!array[num])
				{
					array[num] = true;
					if (areaIndex[num] < 0)
					{
						mouths.Add(num);
					}
					else
					{
						queue.Enqueue(num);
					}
				}
			}
		}
	}

	private int FindAreaIndex(Vector3 pos)
	{
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < indoorAreas.Count; i++)
		{
			IndoorAreaData indoorAreaData = indoorAreas[i];
			if (indoorAreaData != null && indoorAreaData.ContainsXZ(pos))
			{
				float xZArea = indoorAreaData.GetXZArea();
				if (!(xZArea >= num))
				{
					num = xZArea;
					result = i;
				}
			}
		}
		return result;
	}

	private int BfsIndoorOnlyToArea(Vector3 playerPos, int startArea, int targetArea)
	{
		int count = positions.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = -1;
		}
		Queue<int> queue = new Queue<int>();
		for (int j = 0; j < count; j++)
		{
			if (areaIndex[j] == startArea)
			{
				array[j] = 0;
				queue.Enqueue(j);
			}
		}
		return BfsIndoorOnlyFromQueue(playerPos, targetArea, array, queue);
	}

	private int FindDoorTowardStreet(Vector3 playerPos, int playerArea)
	{
		int count = positions.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = -1;
		}
		Queue<int> queue = new Queue<int>();
		for (int j = 0; j < count; j++)
		{
			if (areaIndex[j] >= 0 && HasOutdoorPartner(j))
			{
				array[j] = 0;
				queue.Enqueue(j);
			}
		}
		return BfsIndoorOnlyFromQueue(playerPos, playerArea, array, queue);
	}

	private int BfsIndoorOnlyFromQueue(Vector3 playerPos, int targetArea, int[] hops, Queue<int> queue)
	{
		int result = -1;
		int num = int.MaxValue;
		float num2 = float.PositiveInfinity;
		while (queue.Count > 0)
		{
			int num3 = queue.Dequeue();
			if (areaIndex[num3] == targetArea)
			{
				float num4 = DistXZ(playerPos, positions[num3]);
				if (hops[num3] < num || (hops[num3] == num && num4 < num2))
				{
					num = hops[num3];
					num2 = num4;
					result = num3;
				}
			}
			int num5 = hops[num3] + 1;
			List<int> list = partners[num3];
			for (int i = 0; i < list.Count; i++)
			{
				int num6 = list[i];
				if (hops[num6] < 0 && areaIndex[num6] >= 0)
				{
					hops[num6] = num5;
					queue.Enqueue(num6);
				}
			}
		}
		return result;
	}

	private bool HasOutdoorPartner(int node)
	{
		List<int> list = partners[node];
		for (int i = 0; i < list.Count; i++)
		{
			if (areaIndex[list[i]] < 0)
			{
				return true;
			}
		}
		return false;
	}

	private int FindOutdoorEntranceForArea(Vector3 playerPos, int area)
	{
		if (area < 0 || area >= outdoorMouthsByArea.Count)
		{
			return -1;
		}
		return FindNearestAmong(playerPos, outdoorMouthsByArea[area]);
	}

	private int FindNearestAmong(Vector3 pos, List<int> nodes)
	{
		int result = -1;
		float num = float.PositiveInfinity;
		if (nodes == null)
		{
			return result;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			int num2 = nodes[i];
			float num3 = DistXZ(pos, positions[num2]);
			if (!(num3 >= num))
			{
				num = num3;
				result = num2;
			}
		}
		return result;
	}

	private int GetOrAddNode(WgoData wgoData)
	{
		string id = wgoData.id;
		if (indexById.TryGetValue(id, out var value))
		{
			return value;
		}
		value = positions.Count;
		indexById.Add(id, value);
		positions.Add(wgoData.GetTeleportPointPosition());
		nodeWgos.Add(wgoData);
		edges.Add(new List<int>());
		partners.Add(new List<int>());
		areaIndex.Add(-1);
		return value;
	}

	private void BuildPartnerLists()
	{
		for (int i = 0; i < edges.Count; i++)
		{
			List<int> list = edges[i];
			for (int j = 0; j < list.Count; j++)
			{
				int num = list[j];
				AddUniquePartner(i, num);
				AddUniquePartner(num, i);
			}
		}
	}

	private void AddUniquePartner(int from, int to)
	{
		if (from != to)
		{
			List<int> list = partners[from];
			if (!list.Contains(to))
			{
				list.Add(to);
			}
		}
	}

	private static WgoData ResolveWgo(WorldData worldData, string key)
	{
		WgoData wgoData = worldData.GetWgoData(key);
		if (wgoData != null)
		{
			return wgoData;
		}
		return worldData.GetWgoDataByCustomTag(key);
	}

	private static float DistXZ(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return Mathf.Sqrt(num * num + num2 * num2);
	}
}
