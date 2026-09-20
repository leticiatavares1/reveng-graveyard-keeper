using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ConstructorPartReplacement", menuName = "GK2/Constructor/Replacement Config")]
public class ConstructorPartReplacementConfig : ScriptableObject
{
	[Tooltip("ID of the preset this config belongs to (e.g. 'broken', 'broken_2')")]
	public string presetId;

	[Tooltip("List of replacement mappings from broken to repaired models")]
	public List<ReplacementMapping> mappings = new List<ReplacementMapping>();

	[Tooltip("Addressable references for prefabs that should be deleted (not replaced) during repair")]
	public List<AssetReferenceGameObject> deletionPrefabRefs = new List<AssetReferenceGameObject>();

	[Tooltip("Model IDs for prefabs that should be deleted (not replaced) during repair")]
	[SerializeField]
	private List<string> deletionModelIds = new List<string>();

	[Tooltip("Directory path for broken LUTs")]
	public string brokenLutsDirectoryPath;

	[Tooltip("Directory path for repaired LUTs")]
	public string repairedLutsDirectoryPath;

	public bool TryGetLutAssetPath(bool useRepairedLut, string lutName, out string lutAssetPath)
	{
		lutAssetPath = null;
		if (string.IsNullOrEmpty(lutName) || lutName == LazyConsts.NO_LUT)
		{
			return false;
		}
		string text = (useRepairedLut ? repairedLutsDirectoryPath : brokenLutsDirectoryPath);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		text = text.Replace('\\', '/').TrimEnd('/');
		string text2 = (lutName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? lutName : (lutName + ".png"));
		lutAssetPath = text + "/" + text2;
		return true;
	}

	public bool TryGetRepairedModel(string brokenModelId, out string repairedModelId)
	{
		repairedModelId = null;
		foreach (ReplacementMapping mapping in mappings)
		{
			if (mapping.ContainsBrokenModel(brokenModelId))
			{
				repairedModelId = mapping.RepairedModelId;
				return !string.IsNullOrEmpty(repairedModelId);
			}
		}
		return false;
	}

	public bool TryGetBrokenAssetPath(string brokenModelId, out string brokenAssetPath)
	{
		brokenAssetPath = null;
		foreach (ReplacementMapping mapping in mappings)
		{
			if (mapping.TryGetBrokenAssetPath(brokenModelId, out brokenAssetPath))
			{
				return !string.IsNullOrEmpty(brokenAssetPath);
			}
		}
		return false;
	}

	public bool TryGetRepairedModelWithPath(string brokenModelId, out string repairedModelId, out string repairedAssetPath)
	{
		repairedModelId = null;
		repairedAssetPath = null;
		foreach (ReplacementMapping mapping in mappings)
		{
			if (mapping.ContainsBrokenModel(brokenModelId))
			{
				repairedModelId = mapping.RepairedModelId;
				repairedAssetPath = mapping.RepairedAssetPath;
				return !string.IsNullOrEmpty(repairedModelId);
			}
		}
		return false;
	}

	public bool ShouldDeleteModel(string modelId)
	{
		if (string.IsNullOrEmpty(modelId))
		{
			return false;
		}
		foreach (string deletionModelId in deletionModelIds)
		{
			if (deletionModelId == modelId)
			{
				return true;
			}
		}
		return false;
	}
}
