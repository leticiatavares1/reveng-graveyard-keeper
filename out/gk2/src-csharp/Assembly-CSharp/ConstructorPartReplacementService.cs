using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class ConstructorPartReplacementService
{
	private const string ADDRESSABLES_LABEL = "ConstructorPartReplacements";

	private static Dictionary<string, ConstructorPartReplacementConfig> configsByPresetId;

	private static Dictionary<string, ConstructorPartReplacementConfig> configsById;

	private static bool isInitialized;

	public static void Initialize()
	{
		if (!isInitialized)
		{
			configsByPresetId = new Dictionary<string, ConstructorPartReplacementConfig>();
			configsById = new Dictionary<string, ConstructorPartReplacementConfig>();
			LoadAllConfigs();
			isInitialized = true;
		}
	}

	public static void Reload()
	{
		isInitialized = false;
		Initialize();
	}

	public static ConstructorPartReplacementConfig GetReplacementConfigForPreset(string presetId)
	{
		if (string.IsNullOrEmpty(presetId))
		{
			return null;
		}
		if (!isInitialized)
		{
			Initialize();
		}
		configsByPresetId.TryGetValue(presetId, out var value);
		return value;
	}

	public static ConstructorPartReplacementConfig GetReplacementConfig(string configId)
	{
		if (string.IsNullOrEmpty(configId))
		{
			return null;
		}
		if (!isInitialized)
		{
			Initialize();
		}
		configsById.TryGetValue(configId, out var value);
		return value;
	}

	public static bool TryGetRepairedModel(string brokenModelId, out string repairedModelId)
	{
		repairedModelId = null;
		if (string.IsNullOrEmpty(brokenModelId))
		{
			return false;
		}
		if (!isInitialized)
		{
			Initialize();
		}
		string text = ExtractPresetId(brokenModelId);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (configsByPresetId.TryGetValue(text, out var value))
		{
			return value.TryGetRepairedModel(brokenModelId, out repairedModelId);
		}
		foreach (KeyValuePair<string, ConstructorPartReplacementConfig> item in configsByPresetId)
		{
			if (item.Value.TryGetRepairedModel(brokenModelId, out repairedModelId))
			{
				return true;
			}
		}
		return false;
	}

	public static int ApplyRepairToWso(Wso wso)
	{
		if (wso == null || wso.Data == null)
		{
			return 0;
		}
		WsoRepairablePartData componentData = wso.Data.GetComponentData<WsoRepairablePartData>();
		if (componentData == null)
		{
			return 0;
		}
		ConstructorPartReplacementConfig constructorPartReplacementConfig = wso.Data.Definition.ReplacementConfig;
		if (constructorPartReplacementConfig == null && wso.RuntimeConstructorParts.Count > 0)
		{
			ConstructorPart constructorPart = wso.RuntimeConstructorParts[0];
			if (constructorPart != null)
			{
				constructorPartReplacementConfig = GetReplacementConfigForPreset(ExtractPresetId(constructorPart.constructorPartChildData?.pathToObject ?? ""));
			}
		}
		if (constructorPartReplacementConfig == null)
		{
			Debug.LogWarning("[ConstructorPartReplacementService] No replacement config found for Wso: " + wso.name);
			return 0;
		}
		return componentData.RepairAllStages(constructorPartReplacementConfig);
	}

	private static string ExtractPresetId(string modelName)
	{
		if (string.IsNullOrEmpty(modelName))
		{
			return null;
		}
		int num = modelName.IndexOf('-');
		if (num <= 0)
		{
			return null;
		}
		return modelName.Substring(0, num);
	}

	private static void LoadAllConfigs()
	{
		foreach (ConstructorPartReplacementConfig item in Addressables.LoadAssetsAsync<ConstructorPartReplacementConfig>("ConstructorPartReplacements").WaitForCompletion())
		{
			RegisterConfig(item);
		}
	}

	private static void RegisterConfig(ConstructorPartReplacementConfig config)
	{
		if (config == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(config.presetId))
		{
			if (!configsByPresetId.ContainsKey(config.presetId))
			{
				configsByPresetId[config.presetId] = config;
			}
			else
			{
				Debug.LogWarning("[ConstructorPartReplacementService] Duplicate preset ID '" + config.presetId + "' found. Using first config.");
			}
		}
		string name = config.name;
		if (!string.IsNullOrEmpty(name))
		{
			configsById[name] = config;
		}
	}
}
