using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class WgoPartData
{
	public string id;

	public string variationId;

	public int rotationIndex = -1;

	[SerializeField]
	private List<DockPointData> dockPointDataList;

	[SerializeField]
	private List<int> dockPointDataHashes;

	private Dictionary<int, List<DockPointData>> dockPointDataDict;

	public List<WgoPartStateData> AvailableVariations { get; set; }

	public WgoPartBakedData BakedData => LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.Get(id);

	public int DockPointsCount => dockPointDataList.Count;

	public List<DockPointData> DockPointDataList => dockPointDataList;

	public event Action<string, int> OnStateChange;

	private WgoPartData()
	{
	}

	public WgoPartData(string id)
	{
		this.id = id;
		CreateDockPointData();
	}

	public WgoPartData(string id, string variationId, int rotationIndex)
	{
		this.id = id;
		this.variationId = variationId;
		this.rotationIndex = rotationIndex;
		CreateDockPointData();
	}

	public WgoPartData(WgoPartData other)
	{
		id = other.id;
		variationId = other.variationId;
		rotationIndex = other.rotationIndex;
		CreateDockPointData();
	}

	public void PrepareForGame()
	{
		Dictionary<int, List<DockPointData.Baked>> bakedDockPointsByHash = GetBakedDockPointsByHash();
		if (dockPointDataHashes == null || dockPointDataList == null || dockPointDataHashes.Count != dockPointDataList.Count || !DoesDockPointStructureMatch(bakedDockPointsByHash))
		{
			Dictionary<int, List<SGuid>> occupationByHash = CaptureDockPointOccupationByHash();
			CreateDockPointData();
			RestoreDockPointOccupation(occupationByHash);
		}
		else
		{
			BindBakedDataByHash(bakedDockPointsByHash);
		}
		RebuildDockPointDict();
	}

	private Dictionary<int, List<SGuid>> CaptureDockPointOccupationByHash()
	{
		Dictionary<int, List<SGuid>> dictionary = new Dictionary<int, List<SGuid>>();
		if (dockPointDataHashes == null || dockPointDataList == null)
		{
			return dictionary;
		}
		int num = Math.Min(dockPointDataHashes.Count, dockPointDataList.Count);
		for (int i = 0; i < num; i++)
		{
			int key = dockPointDataHashes[i];
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = (dictionary[key] = new List<SGuid>());
			}
			DockPointData dockPointData = dockPointDataList[i];
			value.Add((dockPointData != null) ? dockPointData.OccupiedBy : SGuid.Empty);
		}
		return dictionary;
	}

	private Dictionary<int, List<DockPointData.Baked>> GetBakedDockPointsByHash()
	{
		Dictionary<int, List<DockPointData.Baked>> dictionary = new Dictionary<int, List<DockPointData.Baked>>();
		IReadOnlyList<WgoPartBakedData.WgoPartDockPointsBakedData> readOnlyList = BakedData?.PointsList;
		if (readOnlyList == null)
		{
			return dictionary;
		}
		foreach (WgoPartBakedData.WgoPartDockPointsBakedData item in readOnlyList)
		{
			if (item?.dockPoints == null)
			{
				continue;
			}
			if (!dictionary.TryGetValue(item.hash, out var value))
			{
				value = new List<DockPointData.Baked>();
				dictionary[item.hash] = value;
			}
			for (int i = 0; i < item.dockPoints.Count; i++)
			{
				if (item.dockPoints[i] != null)
				{
					value.Add(item.dockPoints[i]);
				}
			}
		}
		return dictionary;
	}

	private bool DoesDockPointStructureMatch(Dictionary<int, List<DockPointData.Baked>> bakedByHash)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < dockPointDataHashes.Count; i++)
		{
			int key = dockPointDataHashes[i];
			dictionary[key] = ((!dictionary.TryGetValue(key, out var value)) ? 1 : (value + 1));
		}
		if (dictionary.Count != bakedByHash.Count)
		{
			return false;
		}
		foreach (KeyValuePair<int, List<DockPointData.Baked>> item in bakedByHash)
		{
			if (!dictionary.TryGetValue(item.Key, out var value2) || value2 != item.Value.Count)
			{
				return false;
			}
		}
		return true;
	}

	private void BindBakedDataByHash(Dictionary<int, List<DockPointData.Baked>> bakedByHash)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < dockPointDataHashes.Count; i++)
		{
			int key = dockPointDataHashes[i];
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = 0;
			}
			dictionary[key] = value + 1;
			DockPointData dockPointData = dockPointDataList[i];
			if (dockPointData != null && dockPointData.BakedData == null && bakedByHash.TryGetValue(key, out var value2) && value < value2.Count)
			{
				dockPointData.BakedData = value2[value];
			}
		}
	}

	private void RestoreDockPointOccupation(Dictionary<int, List<SGuid>> occupationByHash)
	{
		if (occupationByHash == null || occupationByHash.Count == 0 || dockPointDataHashes == null || dockPointDataList == null)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		int num = Math.Min(dockPointDataHashes.Count, dockPointDataList.Count);
		for (int i = 0; i < num; i++)
		{
			int key = dockPointDataHashes[i];
			if (occupationByHash.TryGetValue(key, out var value))
			{
				if (!dictionary.TryGetValue(key, out var value2))
				{
					value2 = 0;
				}
				dictionary[key] = value2 + 1;
				DockPointData dockPointData = dockPointDataList[i];
				if (dockPointData != null && value2 < value.Count && value[value2] != SGuid.Empty)
				{
					dockPointData.Occupy(value[value2]);
				}
			}
		}
	}

	private void RebuildDockPointDict()
	{
		dockPointDataDict = new Dictionary<int, List<DockPointData>>();
		if (dockPointDataHashes == null || dockPointDataList == null)
		{
			return;
		}
		int num = Math.Min(dockPointDataHashes.Count, dockPointDataList.Count);
		for (int i = 0; i < num; i++)
		{
			DockPointData dockPointData = dockPointDataList[i];
			if (dockPointData != null)
			{
				if (!dockPointDataDict.TryGetValue(dockPointDataHashes[i], out var value))
				{
					value = new List<DockPointData>();
					dockPointDataDict.Add(dockPointDataHashes[i], value);
				}
				value.Add(dockPointData);
			}
		}
	}

	public override string ToString()
	{
		return $"[id={id}, rotation index={rotationIndex}]";
	}

	public bool TryApplyState(SGuid parent, string variationId, int rotationIndex = -1)
	{
		if (AvailableVariations == null || AvailableVariations.Count == 0)
		{
			return false;
		}
		int num = ((rotationIndex != -1) ? AvailableVariations.FindIndex((WgoPartStateData x) => x.id == variationId && x.rotationIndex == rotationIndex) : AvailableVariations.FindIndex((WgoPartStateData x) => x.id == variationId));
		if (num == -1)
		{
			if (rotationIndex == -1)
			{
				Debug.LogError("Can not find variation [id:" + variationId + "] for part [" + id + "]");
			}
			else
			{
				Debug.LogError($"Can not find variation [id:{variationId}, rotationIndex:[{rotationIndex}]] for part [{id}]");
			}
			return false;
		}
		this.variationId = AvailableVariations[num].id;
		this.rotationIndex = AvailableVariations[num].rotationIndex;
		TryDisableDockPoints(parent);
		this.OnStateChange?.Invoke(this.variationId, this.rotationIndex);
		return true;
	}

	public int GetStateHash()
	{
		return GetStateHash(variationId, rotationIndex);
	}

	public static int GetStateHash(string variationId, int rotationIndex)
	{
		return (17 * 31 + ((!string.IsNullOrEmpty(variationId)) ? variationId.GetHashCode() : 0)) * 31 + rotationIndex;
	}

	public void TryDisableDockPoints(SGuid parent)
	{
		if (!dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			return;
		}
		WorldData worldData = MainGame.Instance?.GameSave?.worldData;
		foreach (DockPointData item in value)
		{
			if (item.IsOccupied)
			{
				SGuid occupiedBy = item.OccupiedBy;
				WgoData wgoData = worldData?.GetWgoData(occupiedBy);
				if (wgoData != null && wgoData.takenDockPointsParentSGuid == parent)
				{
					wgoData.UnOccupyDockPoint(item);
				}
				else
				{
					item.UnOccupy();
				}
				worldData?.NotifyDockPointHasToBeDisabled(parent, occupiedBy);
			}
		}
	}

	public void TryFreeDockPoint(SGuid parent, SGuid occupant)
	{
		if (!dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			return;
		}
		foreach (DockPointData item in value)
		{
			if (item.OccupiedBy == occupant)
			{
				item.UnOccupy();
				MainGame.Instance.GameSave.worldData.NotifyDockPointFreed(parent, occupant);
			}
		}
	}

	[CanBeNull]
	public DockPointData GetNearestDockPoint(WgoData parent, Vector3 positionFrom, out Vector3 dockPointPosition, DockPointData.Availability availability = DockPointData.Availability.All, DockPointData.Filter filter = DockPointData.Filter.All, Func<DockPointData, Vector3, bool> additionalCheck = null)
	{
		dockPointPosition = default(Vector3);
		DockPointData dockPointData = null;
		float num = float.MaxValue;
		if (dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			foreach (DockPointData item in value)
			{
				if (item?.BakedData == null)
				{
					continue;
				}
				switch (availability)
				{
				case DockPointData.Availability.OnlyNotOccupied:
					if (item.IsOccupied)
					{
						continue;
					}
					break;
				case DockPointData.Availability.OnlyOccupied:
					if (!item.IsOccupied)
					{
						continue;
					}
					break;
				}
				switch (filter)
				{
				case DockPointData.Filter.OnlyZombie:
					if (!item.BakedData.IsForZombie)
					{
						continue;
					}
					break;
				case DockPointData.Filter.OnlyNotZombie:
					if (item.BakedData.IsForZombie)
					{
						continue;
					}
					break;
				}
				float magnitude = (parent.GetDockPointDataWorldPosition(item) - positionFrom).magnitude;
				if (magnitude < num && (additionalCheck == null || additionalCheck(item, parent.Position)))
				{
					num = magnitude;
					dockPointData = item;
				}
			}
		}
		if (dockPointData != null)
		{
			dockPointPosition = parent.GetDockPointDataWorldPosition(dockPointData);
		}
		return dockPointData;
	}

	[CanBeNull]
	public DockPointData GetNearestDockPoint(WgoData parent, Vector3 positionFrom, DockPointData.Availability availability = DockPointData.Availability.All, DockPointData.Filter filter = DockPointData.Filter.All, Func<DockPointData, Vector3, bool> additionalCheck = null)
	{
		Vector3 dockPointPosition;
		return GetNearestDockPoint(parent, positionFrom, out dockPointPosition, availability, filter, additionalCheck);
	}

	public DockPointData GetDockPointById(SGuid sGuid)
	{
		if (dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			foreach (DockPointData item in value)
			{
				if (item.OccupiedBy == sGuid)
				{
					return item;
				}
			}
		}
		return null;
	}

	[CanBeNull]
	public DockPointData GetOccupiedDockPointBy(SGuid occupant)
	{
		if (dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			foreach (DockPointData item in value)
			{
				if (item.OccupiedBy == occupant)
				{
					return item;
				}
			}
		}
		return null;
	}

	public List<DockPointData> GetDockPoints(DockPointData.Availability availability = DockPointData.Availability.All, DockPointData.Filter filter = DockPointData.Filter.All)
	{
		List<DockPointData> list = new List<DockPointData>();
		if (dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			foreach (DockPointData item in value)
			{
				if (item?.BakedData == null)
				{
					continue;
				}
				switch (availability)
				{
				case DockPointData.Availability.OnlyNotOccupied:
					if (item.IsOccupied)
					{
						continue;
					}
					break;
				case DockPointData.Availability.OnlyOccupied:
					if (!item.IsOccupied)
					{
						continue;
					}
					break;
				}
				switch (filter)
				{
				case DockPointData.Filter.OnlyZombie:
					if (!item.BakedData.IsForZombie)
					{
						continue;
					}
					break;
				case DockPointData.Filter.OnlyNotZombie:
					if (item.BakedData.IsForZombie)
					{
						continue;
					}
					break;
				}
				list.Add(item);
			}
		}
		return list;
	}

	public int GetOccupiedDockPointIndex(SGuid occupant)
	{
		if (dockPointDataDict.TryGetValue(GetStateHash(), out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].OccupiedBy == occupant)
				{
					return i;
				}
			}
		}
		return -1;
	}

	[CanBeNull]
	public DockPointData GetDockPointByIndex(int index)
	{
		if (dockPointDataDict.TryGetValue(GetStateHash(), out var value) && value.Count - 1 >= index)
		{
			return value[index];
		}
		return null;
	}

	public bool HasAnyAvailableDockPoint(DockPointData.Filter filter = DockPointData.Filter.All)
	{
		foreach (DockPointData dockPoint in GetDockPoints(DockPointData.Availability.All, filter))
		{
			if (!dockPoint.IsOccupied)
			{
				return true;
			}
		}
		return false;
	}

	public Rect GetCollisionBoundsRect(Vector3 objGlobalPos)
	{
		Rect value;
		Rect rect = (BakedData.VariationCollisionBoundsRectDict.TryGetValue(GetStateHash(), out value) ? value : Rect.zero);
		return new Rect(rect.x + objGlobalPos.x, rect.y + objGlobalPos.z, rect.width, rect.height);
	}

	private void CreateDockPointData()
	{
		dockPointDataList = new List<DockPointData>();
		dockPointDataHashes = new List<int>();
		IReadOnlyList<WgoPartBakedData.WgoPartDockPointsBakedData> readOnlyList = BakedData?.PointsList;
		if (readOnlyList == null)
		{
			return;
		}
		foreach (WgoPartBakedData.WgoPartDockPointsBakedData item in readOnlyList)
		{
			if (item?.dockPoints == null)
			{
				continue;
			}
			for (int i = 0; i < item.dockPoints.Count; i++)
			{
				DockPointData.Baked baked = item.dockPoints[i];
				if (baked != null)
				{
					dockPointDataList.Add(new DockPointData
					{
						BakedData = baked
					});
					dockPointDataHashes.Add(item.hash);
				}
			}
		}
	}
}
