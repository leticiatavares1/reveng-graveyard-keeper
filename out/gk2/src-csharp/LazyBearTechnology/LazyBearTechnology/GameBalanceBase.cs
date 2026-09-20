using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LazyBearTechnology;

public class GameBalanceBase : ScriptableObject, IBalance
{
	[NonSerialized]
	private List<IList> datas = new List<IList>();

	[NonSerialized]
	private List<Type> types = new List<Type>();

	[NonSerialized]
	private List<Dictionary<string, int>> cache = new List<Dictionary<string, int>>();

	private bool cacheCreated;

	public static string currentTabName = string.Empty;

	private Dictionary<string, int> tabStartRow = new Dictionary<string, int>();

	public Dictionary<Type, List<AutoValidatorObjectData>> validationData = new Dictionary<Type, List<AutoValidatorObjectData>>();

	private static GameBalanceBase instance = null;

	public static GameBalanceBase Instance
	{
		get
		{
			if (instance == null)
			{
				throw new Exception("GameBalanceBase.Instance is null. You should set it after the balance is loaded to use this functionality.");
			}
			return instance;
		}
		set
		{
			instance = value;
		}
	}

	public GameBalanceBase()
	{
		InitBalance();
	}

	public void InitBalance()
	{
		datas.Clear();
		types.Clear();
		cache.Clear();
		foreach (KeyValuePair<string, IList> allTab in GetAllTabs())
		{
			datas.Add(allTab.Value);
			Type item = allTab.Value.GetType().GetGenericArguments()[0];
			types.Add(item);
			cache.Add(new Dictionary<string, int>());
		}
	}

	public virtual void InitCache()
	{
		CreateIDsCache();
	}

	private void CreateIDsCache()
	{
		for (int i = 0; i < types.Count; i++)
		{
			cache[i].Clear();
			for (int j = 0; j < datas[i].Count; j++)
			{
				BalanceBaseObject balanceBaseObject = datas[i][j] as BalanceBaseObject;
				cache[i].Add(balanceBaseObject.id, j);
			}
		}
		cacheCreated = true;
	}

	public void ClearBalance()
	{
		InitBalance();
		foreach (IList data in datas)
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

	private string AddToDataCollections<T>(List<T> list, T dataToAdd) where T : BalanceBaseObject
	{
		if (HaveCollectionSameID(list, dataToAdd))
		{
			return "Can't add: same id already exist: " + dataToAdd.id;
		}
		list.Add(dataToAdd);
		return "";
	}

	public string AddData<T>(T dataToAdd) where T : BalanceBaseObject
	{
		int num = types.IndexOf(typeof(T));
		if (num == -1)
		{
			Debug.LogError("Unknown type at AddData: " + typeof(T));
			return null;
		}
		return AddToDataCollections(datas[num] as List<T>, dataToAdd);
	}

	public string AddDataUniversal(object dataToAdd)
	{
		BalanceBaseObject balanceBaseObject = dataToAdd as BalanceBaseObject;
		Type type = dataToAdd.GetType();
		if (balanceBaseObject == null)
		{
			Debug.LogError("Type " + type.Name + " couldn't be converted to BalanceBaseObject");
			return null;
		}
		int num = types.IndexOf(type);
		if (num == -1)
		{
			Debug.LogError("Unknown type at AddData: " + type.Name);
			return null;
		}
		foreach (object item in datas[num])
		{
			if ((item as BalanceBaseObject).id == balanceBaseObject.id)
			{
				return "Can't add. Same id already exist: " + balanceBaseObject.id;
			}
		}
		datas[num].Add(dataToAdd);
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
		int num = types.IndexOf(typeof(T));
		if (num == -1)
		{
			Debug.LogError($"No data for object [{typeof(T)}] with id = [{id}]");
			return null;
		}
		if (!cacheCreated)
		{
			return GetElementByID(datas[num] as List<T>, id);
		}
		return GetElementByID(datas[num] as List<T>, cache[num], id);
	}

	public List<T> GetDataCollection<T>() where T : BalanceBaseObject
	{
		int num = types.IndexOf(typeof(T));
		if (num == -1)
		{
			Debug.LogError("Unknown type at GetDataCollection: " + typeof(T));
			return null;
		}
		return datas[num] as List<T>;
	}

	public IList GetDataCollection(Type type)
	{
		int num = types.IndexOf(type);
		if (num == -1)
		{
			Debug.LogError("Unknown type at GetDataCollection: " + type);
			return null;
		}
		return datas[num];
	}

	[Obsolete("There's no need overriding this method anymore. Use a [BalanceTab] attribute instead.")]
	public virtual Dictionary<string, IList> GetAllDataListsAndGoogleTabs()
	{
		return new Dictionary<string, IList>();
	}

	public Dictionary<string, IList> GetAllTabs()
	{
		Dictionary<string, IList> allDataListsAndGoogleTabs = GetAllDataListsAndGoogleTabs();
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			BalanceTabAttribute customAttribute = fieldInfo.GetCustomAttribute<BalanceTabAttribute>();
			if (customAttribute != null)
			{
				allDataListsAndGoogleTabs.Add(customAttribute.TabName, (IList)fieldInfo.GetValue(this));
				if (customAttribute.StartRow != -1)
				{
					tabStartRow[customAttribute.TabName] = customAttribute.StartRow;
				}
			}
		}
		return allDataListsAndGoogleTabs;
	}
}
