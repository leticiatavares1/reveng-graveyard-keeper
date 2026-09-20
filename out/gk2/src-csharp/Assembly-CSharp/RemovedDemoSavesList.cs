using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class RemovedDemoSavesList
{
	private const string PLAYER_PREFS_KEY = "removedDemoSaves";

	private static RemovedDemoSavesList instance;

	public List<string> removedSaves = new List<string>();

	public static RemovedDemoSavesList Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Load();
			}
			return instance;
		}
	}

	public static void TryAddSave(string save)
	{
		if (!IsDeleted(save))
		{
			Instance.removedSaves.Add(save);
			Save();
		}
	}

	public static void TryRemoveSave(string save)
	{
		if (IsDeleted(save))
		{
			Instance.removedSaves.Remove(save);
			Save();
		}
	}

	public static bool IsDeleted(string save)
	{
		return Instance.removedSaves.Contains(save);
	}

	private static RemovedDemoSavesList Load()
	{
		string text = string.Empty;
		if (LazyAPI.PlayerPrefs.HasKey("removedDemoSaves"))
		{
			text = LazyAPI.PlayerPrefs.GetString("removedDemoSaves");
		}
		if (!string.IsNullOrEmpty(text))
		{
			return JsonUtility.FromJson<RemovedDemoSavesList>(text);
		}
		return new RemovedDemoSavesList();
	}

	public static void Save()
	{
		string value = JsonUtility.ToJson(Instance);
		LazyAPI.PlayerPrefs.SetString("removedDemoSaves", value);
		LazyAPI.PlayerPrefs.Save();
		try
		{
			LazyAPI.PlayerPrefs.Save();
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error during save RemovedDemoSavesList :[{arg}]");
		}
	}
}
