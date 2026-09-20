using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBalanceBase : ScriptableObject
{
	[NonSerialized]
	private List<IList> _datas = new List<IList>();

	[NonSerialized]
	private List<Type> _types = new List<Type>();

	[NonSerialized]
	private List<Dictionary<string, int>> _cache = new List<Dictionary<string, int>>();

	private bool _cache_created;

	public static string current_tab_name = "";

	public GameBalanceBase()
	{
		InitBalance();
	}

	private void InitBalance()
	{
		_datas.Clear();
		_types.Clear();
		_cache.Clear();
		foreach (KeyValuePair<string, IList> allDataListsAndGoogleTab in GetAllDataListsAndGoogleTabs())
		{
			_datas.Add(allDataListsAndGoogleTab.Value);
			Type item = allDataListsAndGoogleTab.Value.GetType().GetGenericArguments()[0];
			_types.Add(item);
			_cache.Add(new Dictionary<string, int>());
		}
	}

	public void CreateIDsCache()
	{
		for (int i = 0; i < _types.Count; i++)
		{
			_cache[i].Clear();
			for (int j = 0; j < _datas[i].Count; j++)
			{
				BalanceBaseObject balanceBaseObject = _datas[i][j] as BalanceBaseObject;
				_cache[i].Add(balanceBaseObject.id, j);
			}
		}
		_cache_created = true;
	}

	public void ClearBalance()
	{
		InitBalance();
		foreach (IList data in _datas)
		{
			data.Clear();
		}
	}

	protected static T DataNotFound<T>(string identifier) where T : BalanceBaseObject
	{
		Debug.LogWarning("No data for object [" + typeof(T)?.ToString() + "] with id = \"" + identifier + "\"");
		return null;
	}

	private static bool HaveCollectionSameID<T>(List<T> list, T data) where T : BalanceBaseObject
	{
		return list.FindIndex((T p) => p.id == data.id) != -1;
	}

	private static T GetElementByID<T>(List<T> list, string id) where T : BalanceBaseObject
	{
		return list.Find((T p) => p.id == id);
	}

	private static T GetElementByID<T>(List<T> list, Dictionary<string, int> cache, string id) where T : BalanceBaseObject
	{
		if (id == null)
		{
			return null;
		}
		try
		{
			if (cache == null)
			{
				Debug.LogError("ERROR: Trying to get a " + typeof(T)?.ToString() + " item with a null cache");
				return null;
			}
			return list[cache[id]];
		}
		catch (Exception)
		{
			return null;
		}
	}

	private string AddToDataCollections<T>(List<T> list, T data_to_add) where T : BalanceBaseObject
	{
		if (HaveCollectionSameID(list, data_to_add))
		{
			return "Can't add: same id already exist: " + data_to_add.id;
		}
		list.Add(data_to_add);
		return "";
	}

	public string AddData<T>(T data_to_add) where T : BalanceBaseObject
	{
		int num = _types.IndexOf(typeof(T));
		if (num == -1)
		{
			Debug.LogError("Unknown type at AddData: " + typeof(T));
			return null;
		}
		return AddToDataCollections(_datas[num] as List<T>, data_to_add);
	}

	public string AddDataUniversal(object data_to_add)
	{
		BalanceBaseObject balanceBaseObject = data_to_add as BalanceBaseObject;
		Type type = data_to_add.GetType();
		if (balanceBaseObject == null)
		{
			Debug.LogError("Type " + type.Name + " couldn't be converted to BalanceBaseObject");
			return null;
		}
		int num = _types.IndexOf(type);
		if (num == -1)
		{
			Debug.LogError("Unknown type at AddData: " + type.Name);
			return null;
		}
		foreach (object item in _datas[num])
		{
			if ((item as BalanceBaseObject).id == balanceBaseObject.id)
			{
				return "Can't add: same id already exist: " + balanceBaseObject.id;
			}
		}
		_datas[num].Add(data_to_add);
		return "";
	}

	public T GetData<T>(string id) where T : BalanceBaseObject
	{
		return GetDataOrNull<T>(id) ?? DataNotFound<T>(id);
	}

	public T GetDataOrNull<T>(string id) where T : BalanceBaseObject
	{
		if (id == null)
		{
			return null;
		}
		int num = _types.IndexOf(typeof(T));
		if (num == -1)
		{
			Debug.LogError("Unknown type at GetData: " + typeof(T));
			return null;
		}
		if (!_cache_created)
		{
			return GetElementByID(_datas[num] as List<T>, id);
		}
		return GetElementByID(_datas[num] as List<T>, _cache[num], id);
	}

	public virtual Dictionary<string, IList> GetAllDataListsAndGoogleTabs()
	{
		throw new Exception("GameBalanceBase.GetAllDataListsAndGoogleTabs() should be overriden!");
	}
}
