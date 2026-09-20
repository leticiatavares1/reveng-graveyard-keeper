using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public static class ConveyorSaveSystem
{
	private const string DATA_FILE_EXTENSION = ".bytes";

	private static ConveyorPresetSerializer serializer;

	public static LazySaveSystem lazySaveSystem = new LazySaveSystem((typeof(ConveyorPreset), Serializer));

	private static ConveyorPresetSerializer Serializer
	{
		get
		{
			if (serializer != null)
			{
				return serializer;
			}
			serializer = new ConveyorPresetSerializer(".bytes");
			return serializer;
		}
	}

	public static List<ConveyorPreset> ReadConveyorPresets(string folder)
	{
		try
		{
			List<ConveyorPreset> presets = null;
			lazySaveSystem.LoadAll<ConveyorPreset>(folder, delegate
			{
				AfterLoadAll();
			});
			return presets;
			void AfterLoadAll()
			{
				presets = new List<ConveyorPreset>();
				foreach (var item in P_0.list)
				{
					string presetName = item.fileName.Replace(".bytes", "");
					item.data.presetName = presetName;
					presets.Add(item.data);
				}
			}
		}
		catch (Exception arg)
		{
			Debug.Log($"Error during load save slots:[{arg}]");
			return new List<ConveyorPreset>();
		}
	}

	public static void LoadConveyorPreset(string directory, string presetName, Action<ConveyorPreset> callback)
	{
		try
		{
			lazySaveSystem.Load(directory, presetName, callback);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error during load save:[{arg}]");
		}
	}

	public static void SaveConveyorPreset(ConveyorPreset conveyorPreset, string directory, string presetName, Action callbackSuccessful = null, Action callbackUnsuccessful = null, bool autoOnSaveStart = true)
	{
		try
		{
			lazySaveSystem.Save(conveyorPreset, directory, presetName, null);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error during save:[{arg}]");
			callbackUnsuccessful?.Invoke();
		}
	}
}
