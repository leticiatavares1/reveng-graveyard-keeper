using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class ReplacementMapping
{
	[Tooltip("Addressable references for broken prefabs that can be replaced (many-to-one support)")]
	public List<AssetReferenceGameObject> brokenPrefabRefs = new List<AssetReferenceGameObject>();

	[Tooltip("Addressable reference for the repaired prefab")]
	public AssetReferenceGameObject repairedPrefabRef;

	[Tooltip("Broken model IDs cached for runtime lookup")]
	[SerializeField]
	private List<string> brokenModelIds = new List<string>();

	[Tooltip("Addressables paths for broken prefabs (parallel to brokenModelIds, editor-synced)")]
	[SerializeField]
	private List<string> brokenAssetPaths = new List<string>();

	[Tooltip("Repaired model ID cached for runtime lookup")]
	[SerializeField]
	private string repairedModelId;

	[Tooltip("Addressables path for the repaired prefab (auto-populated in editor)")]
	[SerializeField]
	private string repairedAssetPath;

	public string RepairedModelId => repairedModelId;

	public string RepairedAssetPath => repairedAssetPath;

	public bool ContainsBrokenModel(string modelId)
	{
		if (string.IsNullOrEmpty(modelId))
		{
			return false;
		}
		foreach (string brokenModelId in brokenModelIds)
		{
			if (brokenModelId == modelId)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetBrokenAssetPath(string brokenModelId, out string brokenAssetPath)
	{
		brokenAssetPath = null;
		if (string.IsNullOrEmpty(brokenModelId))
		{
			return false;
		}
		for (int i = 0; i < brokenModelIds.Count; i++)
		{
			if (!(brokenModelIds[i] != brokenModelId))
			{
				if (i < brokenAssetPaths.Count)
				{
					brokenAssetPath = brokenAssetPaths[i];
				}
				return !string.IsNullOrEmpty(brokenAssetPath);
			}
		}
		return false;
	}

	public List<string> GetBrokenModelIds()
	{
		return new List<string>(brokenModelIds);
	}
}
