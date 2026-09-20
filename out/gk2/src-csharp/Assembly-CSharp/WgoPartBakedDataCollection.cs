using System.Collections.Generic;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "WgoPartBakedDataCollection")]
public class WgoPartBakedDataCollection : LazySingletonSerializedSO<WgoPartBakedDataCollection>
{
	private const string DATA_DIRECTORY = "Assets/AddressableAssets/Configurations/WgoPartBakedData";

	private const string ADDRESSABLE_GROUP = "WgoPartBakedData";

	private const string ADDRESSABLE_LABEL = "WgoPartBakedData";

	private const string ADDRESSABLE_ADDRESS_PREFIX = "WgoPartBakedData";

	[OdinSerialize]
	private List<WgoPartBakedData> dataList = new List<WgoPartBakedData>();

	private Dictionary<string, WgoPartBakedData> cachedData = new Dictionary<string, WgoPartBakedData>();

	private bool isCacheLoaded;

	public void AddOrUpdate(WgoPartBakedData data)
	{
		if (data == null || string.IsNullOrEmpty(data.id))
		{
			Debug.LogWarning("Can not add null or empty id WgoPartBakedData.");
			return;
		}
		EnsureCacheLoaded();
		cachedData[data.id] = data;
	}

	public WgoPartBakedData Get(string id)
	{
		EnsureCacheLoaded();
		cachedData.TryGetValue(id, out var value);
		if (value == null)
		{
			Debug.LogWarning("No WgoPartBakedData with id:[" + id + "]");
			return WgoPartBakedData.Empty;
		}
		return value;
	}

	public void ClearCache()
	{
		if (cachedData == null)
		{
			cachedData = new Dictionary<string, WgoPartBakedData>();
		}
		cachedData.Clear();
		isCacheLoaded = false;
	}

	public void Clear()
	{
		ClearCache();
	}

	public void LoadCache()
	{
		EnsureCacheLoaded();
	}

	private void EnsureCacheLoaded()
	{
		if (isCacheLoaded)
		{
			return;
		}
		if (cachedData == null)
		{
			cachedData = new Dictionary<string, WgoPartBakedData>();
		}
		cachedData.Clear();
		foreach (WgoPartBakedData data in dataList)
		{
			if (data != null && !string.IsNullOrEmpty(data.id))
			{
				cachedData[data.id] = data;
			}
		}
		Debug.Log($"WgoPartBakedDataCollection loaded [{cachedData.Count}] items");
		isCacheLoaded = true;
	}

	private void OnEnable()
	{
		isCacheLoaded = false;
	}
}
