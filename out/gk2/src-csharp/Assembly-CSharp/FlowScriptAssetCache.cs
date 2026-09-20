using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FlowCanvas;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FlowScriptAssetCache : LazySingleton<FlowScriptAssetCache>
{
	private const int YIELD_EVERY = 5;

	private static readonly string[] PreloadPathPrefixes = new string[2] { "Assets/AddressableAssets/VisualScripts/GlobalScripts/", "Assets/AddressableAssets/VisualScripts/WGODataScripts/" };

	private readonly Dictionary<string, FlowGraph> graphsByAssetPath = new Dictionary<string, FlowGraph>(StringComparer.Ordinal);

	private readonly HashSet<FlowGraph> cachedGraphs = new HashSet<FlowGraph>();

	private readonly List<AsyncOperationHandle<FlowGraph>> handles = new List<AsyncOperationHandle<FlowGraph>>();

	private bool isLoaded;

	private float localProgress;

	public static bool IsLoaded => LazySingleton<FlowScriptAssetCache>.Instance.isLoaded;

	public static float LocalProgress => LazySingleton<FlowScriptAssetCache>.Instance.localProgress;

	public static int LoadedCount => LazySingleton<FlowScriptAssetCache>.Instance.graphsByAssetPath.Count;

	public static async UniTask LoadAllAsync()
	{
		await LazySingleton<FlowScriptAssetCache>.Instance.LoadAllAsyncInternal();
	}

	public static FlowGraph Get(string graphPathWithoutExtension)
	{
		return LazySingleton<FlowScriptAssetCache>.Instance.GetInternal(graphPathWithoutExtension);
	}

	public static void EnsureLoaded(string graphPathWithoutExtension)
	{
		LazySingleton<FlowScriptAssetCache>.Instance.EnsureLoadedInternal(graphPathWithoutExtension);
	}

	public static bool IsCached(FlowGraph graph)
	{
		return LazySingleton<FlowScriptAssetCache>.Instance.IsCachedInternal(graph);
	}

	public static void ReleaseAll()
	{
		LazySingleton<FlowScriptAssetCache>.Instance.ReleaseAllInternal();
	}

	private async UniTask LoadAllAsyncInternal()
	{
		if (isLoaded)
		{
			return;
		}
		GameShutdown.ThrowIfRequested();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		List<string> addresses = CollectAddresses();
		if (addresses.Count == 0)
		{
			isLoaded = true;
			localProgress = 1f;
			return;
		}
		for (int i = 0; i < addresses.Count; i++)
		{
			GameShutdown.ThrowIfRequested();
			string text = addresses[i];
			if (!graphsByAssetPath.ContainsKey(text))
			{
				await LoadAndCacheAsync(text);
			}
			UpdateProgress(i + 1, addresses.Count);
			await BackgroundLoading.YieldIfNeeded(i + 1, 5);
		}
		isLoaded = true;
		Debug.Log($"FlowScriptAssetCache loaded {graphsByAssetPath.Count}/{addresses.Count} flow script assets.");
	}

	private FlowGraph GetInternal(string graphPathWithoutExtension)
	{
		string text = graphPathWithoutExtension + ".asset";
		if (!graphsByAssetPath.TryGetValue(text, out var value))
		{
			Debug.LogError("Flow script asset is not preloaded in cache: " + text);
			return null;
		}
		return UnityEngine.Object.Instantiate(value);
	}

	private void EnsureLoadedInternal(string graphPathWithoutExtension)
	{
		string text = graphPathWithoutExtension + ".asset";
		if (!graphsByAssetPath.ContainsKey(text))
		{
			LoadAndCacheAsync(text).GetAwaiter().GetResult();
		}
	}

	private bool IsCachedInternal(FlowGraph graph)
	{
		if (graph != null)
		{
			return cachedGraphs.Contains(graph);
		}
		return false;
	}

	private void ReleaseAllInternal()
	{
		foreach (AsyncOperationHandle<FlowGraph> handle in handles)
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
		handles.Clear();
		graphsByAssetPath.Clear();
		cachedGraphs.Clear();
		isLoaded = false;
		localProgress = 0f;
	}

	private async UniTask LoadAndCacheAsync(string address)
	{
		AsyncOperationHandle<FlowGraph> handle = Addressables.LoadAssetAsync<FlowGraph>(address);
		await handle.ToUniTask(null, PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true, autoReleaseWhenCanceled: true);
		if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
		{
			RegisterLoadedAsset(address, handle);
		}
		else if (handle.IsValid())
		{
			Addressables.Release(handle);
		}
	}

	private void RegisterLoadedAsset(string address, AsyncOperationHandle<FlowGraph> handle)
	{
		handles.Add(handle);
		graphsByAssetPath[address] = handle.Result;
		cachedGraphs.Add(handle.Result);
	}

	private static List<string> CollectAddresses()
	{
		List<string> list = new List<string>();
		foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
		{
			foreach (object key in resourceLocator.Keys)
			{
				if (key is string text && text.EndsWith(".asset", StringComparison.Ordinal) && IsPreloadPath(text) && !list.Contains(text))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	private static bool IsPreloadPath(string address)
	{
		string[] preloadPathPrefixes = PreloadPathPrefixes;
		foreach (string value in preloadPathPrefixes)
		{
			if (address.StartsWith(value, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateProgress(int loaded, int total)
	{
		localProgress = ((total > 0) ? ((float)loaded / (float)total) : 1f);
	}
}
