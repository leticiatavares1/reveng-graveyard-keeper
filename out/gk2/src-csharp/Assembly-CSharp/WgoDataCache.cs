using System;
using System.Collections.Generic;
using UnityEngine;

public class WgoDataCache
{
	public Dictionary<Guid, WgoData> wgoDataByUidCache = new Dictionary<Guid, WgoData>();

	public Dictionary<string, List<WgoData>> wgoDataByIdsCache = new Dictionary<string, List<WgoData>>();

	public Dictionary<string, List<WgoData>> wgoDataByCustomTagsCache = new Dictionary<string, List<WgoData>>();

	public Dictionary<string, List<WgoData>> wgoDataByGroup = new Dictionary<string, List<WgoData>>();

	public Dictionary<Guid, WsoData> wsoDataByUidCache = new Dictionary<Guid, WsoData>();

	public Dictionary<string, List<WsoData>> wsoDataByIdCache = new Dictionary<string, List<WsoData>>();

	public Dictionary<string, List<WsoData>> wsoDataByCustomTagsCache = new Dictionary<string, List<WsoData>>();

	public void AddWgoDataToCache(WgoData wgoData)
	{
		if (!wgoDataByUidCache.TryAdd(wgoData.UniqueId.Guid, wgoData))
		{
			Debug.LogError($"WgoData with [{wgoData.UniqueId}] already exists in cache, id: {wgoData.id}, scene: {wgoData.WorldId}" + ", duplicates: " + wgoDataByUidCache[wgoData.UniqueId.Guid].id + " from scene: " + wgoDataByUidCache[wgoData.UniqueId.Guid].WorldId);
			return;
		}
		if (wgoDataByIdsCache.TryGetValue(wgoData.id, out var value))
		{
			value.Add(wgoData);
		}
		else
		{
			wgoDataByIdsCache.Add(wgoData.id, new List<WgoData> { wgoData });
		}
		if (!string.IsNullOrEmpty(wgoData.CustomTag))
		{
			if (wgoDataByCustomTagsCache.TryGetValue(wgoData.CustomTag, out var value2))
			{
				value2.Add(wgoData);
			}
			else
			{
				wgoDataByCustomTagsCache.Add(wgoData.CustomTag, new List<WgoData> { wgoData });
			}
		}
		if (!string.IsNullOrEmpty(wgoData.Definition.wgoGroup))
		{
			if (wgoDataByGroup.TryGetValue(wgoData.Definition.wgoGroup, out var value3))
			{
				value3.Add(wgoData);
				return;
			}
			wgoDataByGroup.Add(wgoData.Definition.wgoGroup, new List<WgoData> { wgoData });
		}
	}

	public bool RemoveWgoDataFromCache(WgoData wgoData)
	{
		if (!wgoDataByUidCache.Remove(wgoData.UniqueId.Guid))
		{
			return false;
		}
		RemoveFromListCache(wgoDataByIdsCache, wgoData.id, wgoData);
		RemoveFromListCache(wgoDataByCustomTagsCache, wgoData.CustomTag, wgoData);
		RemoveFromListCache(wgoDataByGroup, wgoData.Definition?.wgoGroup, wgoData);
		return true;
	}

	public void AddWsoDataToCache(WsoData wsoData)
	{
		if (!wsoDataByUidCache.TryAdd(wsoData.UniqueId.Guid, wsoData))
		{
			WsoData wsoData2 = wsoDataByUidCache[wsoData.UniqueId.Guid];
			Debug.LogError($"WsoData with [{wsoData.UniqueId}] already exists in cache, id: {wsoData.id}, scene: {wsoData.WorldId}" + ", duplicates: " + wsoData2.id + " from scene: " + wsoData2.WorldId);
			return;
		}
		if (!string.IsNullOrEmpty(wsoData.id))
		{
			if (wsoDataByIdCache.TryGetValue(wsoData.id, out var value))
			{
				value.Add(wsoData);
			}
			else
			{
				wsoDataByIdCache.Add(wsoData.id, new List<WsoData> { wsoData });
			}
		}
		if (!string.IsNullOrEmpty(wsoData.CustomTag))
		{
			if (wsoDataByCustomTagsCache.TryGetValue(wsoData.CustomTag, out var value2))
			{
				value2.Add(wsoData);
				return;
			}
			wsoDataByCustomTagsCache.Add(wsoData.CustomTag, new List<WsoData> { wsoData });
		}
	}

	public bool RemoveWsoDataFromCache(WsoData wsoData)
	{
		if (!wsoDataByUidCache.Remove(wsoData.UniqueId.Guid))
		{
			return false;
		}
		RemoveFromListCache(wsoDataByIdCache, wsoData.id, wsoData);
		RemoveFromListCache(wsoDataByCustomTagsCache, wsoData.CustomTag, wsoData);
		return true;
	}

	private static void RemoveFromListCache<T>(Dictionary<string, List<T>> cache, string key, T item)
	{
		if (!string.IsNullOrEmpty(key) && cache.TryGetValue(key, out var value))
		{
			value.Remove(item);
			if (value.Count == 0)
			{
				cache.Remove(key);
			}
		}
	}

	public WsoData GetWsoDataFromCache(SGuid uniqueId)
	{
		if (SGuid.IsNullOrEmpty(uniqueId))
		{
			return null;
		}
		wsoDataByUidCache.TryGetValue(uniqueId.Guid, out var value);
		return value;
	}
}
