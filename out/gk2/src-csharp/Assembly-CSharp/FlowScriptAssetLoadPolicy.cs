using System;
using Cysharp.Threading.Tasks;
using FlowCanvas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class FlowScriptAssetLoadPolicy
{
	public static FlowScriptAssetLoadPolicyType Current { get; private set; }

	public static bool UsesBackgroundPreload => Current == FlowScriptAssetLoadPolicyType.Cached;

	public static float PreloadProgress => FlowScriptAssetCache.LocalProgress;

	public static void SetPolicy(FlowScriptAssetLoadPolicyType policy)
	{
		if (Current != policy)
		{
			if (Current == FlowScriptAssetLoadPolicyType.Cached)
			{
				FlowScriptAssetCache.ReleaseAll();
			}
			Current = policy;
			D.LogColor($"Flow script asset load policy set to [{policy}]", "yellow");
		}
	}

	public static async UniTask PreloadAsync()
	{
		if (UsesBackgroundPreload)
		{
			await FlowScriptAssetCache.LoadAllAsync();
		}
	}

	public static FlowGraph Load(string graphPathWithoutExtension)
	{
		if (Current == FlowScriptAssetLoadPolicyType.Cached)
		{
			return FlowScriptAssetCache.Get(graphPathWithoutExtension);
		}
		return LoadOnDemand(graphPathWithoutExtension);
	}

	public static void ReleaseGraph(FlowGraph graph)
	{
		if (graph == null)
		{
			return;
		}
		if (Current == FlowScriptAssetLoadPolicyType.Cached)
		{
			if (!FlowScriptAssetCache.IsCached(graph))
			{
				UnityEngine.Object.Destroy(graph);
			}
		}
		else
		{
			Addressables.Release(graph);
		}
	}

	private static FlowGraph LoadOnDemand(string graphPathWithoutExtension)
	{
		string text = graphPathWithoutExtension + ".asset";
		try
		{
			AsyncOperationHandle<FlowGraph> handle = Addressables.LoadAssetAsync<FlowGraph>(text);
			handle.WaitForCompletion();
			if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
			{
				if (handle.IsValid())
				{
					Addressables.Release(handle);
				}
				Debug.LogError("Not found flow graph: " + text);
				return null;
			}
			return handle.Result;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}
}
