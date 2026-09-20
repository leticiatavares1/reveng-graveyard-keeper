using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WgoPartPool : LazySingleton<WgoPartPool>, IProgress<float>
{
	private const int YIELD_EVERY = 10;

	private const string ADDRESSABLE_KEY_PREFIX = "Assets/AddressableAssets/WGOs/";

	private readonly Dictionary<string, Stack<WgoPart>> pools = new Dictionary<string, Stack<WgoPart>>();

	private readonly Dictionary<string, AsyncOperationHandle<GameObject>> loadedHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

	public float LocalProgress { get; private set; }

	public async UniTask InitAsync()
	{
		GameShutdown.ThrowIfRequested();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		float totalConfigsCount = LazySingletonSO<WgoPartPoolInitialSizesConfig>.Instance.initialSizeConfigs.List.Count;
		int currentConfigIndex = 0;
		foreach (GameResAtom atom in LazySingletonSO<WgoPartPoolInitialSizesConfig>.Instance.initialSizeConfigs.List)
		{
			GameShutdown.ThrowIfRequested();
			string addressableKey = atom.type;
			GameObject prefab = LoadPrefabSync(addressableKey);
			Report((float)currentConfigIndex / totalConfigsCount);
			currentConfigIndex++;
			if (prefab == null)
			{
				Debug.LogError("[WgoPartPool] Failed to load prefab at [" + addressableKey + "]");
				continue;
			}
			Stack<WgoPart> value;
			int num = (pools.TryGetValue(addressableKey, out value) ? value.Count : 0);
			for (int i = num; (float)i < atom.value; i++)
			{
				GameShutdown.ThrowIfRequested();
				WgoPart wgoPart = UnityEngine.Object.Instantiate(prefab.GetComponent<WgoPart>());
				wgoPart.CleanupChunkableComponents();
				wgoPart.PooledAddressableKey = addressableKey;
				Release(addressableKey, wgoPart);
				await BackgroundLoading.YieldIfNeeded(i + 1, 10);
			}
		}
	}

	public WgoPart GetSync(string addressableKey, Wgo wgo)
	{
		GameObject gameObject;
		if (pools.TryGetValue(addressableKey, out var value) && value.Count > 0)
		{
			WgoPart wgoPart = value.Pop();
			wgoPart.transform.SetParent(wgo.transform);
			gameObject = LoadPrefabSync(addressableKey);
			if (gameObject == null)
			{
				Debug.LogError("[WgoPartPool] Failed to load prefab at [" + addressableKey + "]");
			}
			else
			{
				wgoPart.transform.localScale = gameObject.transform.localScale;
			}
			wgoPart.transform.localPosition = Vector3.zero;
			wgoPart.ReInitFromPool(wgo);
			return wgoPart;
		}
		gameObject = LoadPrefabSync(addressableKey);
		if (gameObject == null)
		{
			Debug.LogError("[WgoPartPool] Failed to load prefab at [" + addressableKey + "]");
			return null;
		}
		WgoPart wgoPart2 = UnityEngine.Object.Instantiate(gameObject.GetComponent<WgoPart>(), wgo.transform);
		wgoPart2.transform.localScale = gameObject.transform.localScale;
		wgoPart2.CleanupChunkableComponents();
		wgoPart2.ReInitFromPool(wgo);
		wgoPart2.PooledAddressableKey = addressableKey;
		return wgoPart2;
	}

	public async Awaitable<WgoPart> GetAsync(string addressableKey, Wgo wgo)
	{
		if (pools.TryGetValue(addressableKey, out var value) && value.Count > 0)
		{
			WgoPart wgoPart = value.Pop();
			wgoPart.transform.SetParent(wgo.transform);
			wgoPart.transform.localPosition = Vector3.zero;
			wgoPart.ReInitFromPool(wgo);
			return wgoPart;
		}
		GameObject gameObject = await LoadPrefab(addressableKey);
		if (gameObject == null)
		{
			Debug.LogError("[WgoPartPool] Failed to load prefab at [" + addressableKey + "]");
			return null;
		}
		WgoPart wgoPart2 = UnityEngine.Object.Instantiate(gameObject.GetComponent<WgoPart>(), wgo.transform);
		wgoPart2.CleanupChunkableComponents();
		wgoPart2.ReInitFromPool(wgo);
		wgoPart2.PooledAddressableKey = addressableKey;
		return wgoPart2;
	}

	public void Release(string addressableKey, WgoPart wgoPart)
	{
		if (!(wgoPart == null))
		{
			wgoPart.DeInitForPool();
			wgoPart.gameObject.SetActive(value: false);
			wgoPart.transform.SetParent(base.transform);
			if (!pools.TryGetValue(addressableKey, out var value))
			{
				value = new Stack<WgoPart>();
				pools[addressableKey] = value;
			}
			value.Push(wgoPart);
		}
	}

	public static int TrimPaths(IEnumerable<string> paths, ScenePoolTrimPolicy policy)
	{
		return LazySingleton<WgoPartPool>.Instance.TrimPathsInternal(paths, policy);
	}

	public static string GetAddressableKey(string partAssetId)
	{
		if (string.IsNullOrEmpty(partAssetId))
		{
			return null;
		}
		return "Assets/AddressableAssets/WGOs/" + partAssetId + ".prefab";
	}

	public static void CollectAddressableKeysForWgoData(WgoData wgoData, HashSet<string> paths)
	{
		if (wgoData == null || paths == null)
		{
			return;
		}
		string addressableKey = GetAddressableKey((wgoData.Definition != null) ? wgoData.Definition.ResolveAssetId(wgoData.id, wgoData) : wgoData.id);
		if (!string.IsNullOrEmpty(addressableKey))
		{
			paths.Add(addressableKey);
		}
		List<WgoPartData> additionalWgoPartsData = wgoData.AdditionalWgoPartsData;
		for (int i = 0; i < additionalWgoPartsData.Count; i++)
		{
			string addressableKey2 = GetAddressableKey(additionalWgoPartsData[i].id);
			if (!string.IsNullOrEmpty(addressableKey2))
			{
				paths.Add(addressableKey2);
			}
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<string, Stack<WgoPart>> pool in pools)
		{
			foreach (WgoPart item in pool.Value)
			{
				if (item != null)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
		}
		pools.Clear();
		foreach (KeyValuePair<string, AsyncOperationHandle<GameObject>> loadedHandle in loadedHandles)
		{
			if (loadedHandle.Value.IsValid())
			{
				Addressables.Release(loadedHandle.Value);
			}
		}
		loadedHandles.Clear();
	}

	private GameObject LoadPrefabSync(string addressableKey)
	{
		if (GameShutdown.IsRequested)
		{
			return null;
		}
		if (loadedHandles.TryGetValue(addressableKey, out var value) && value.IsValid())
		{
			if (!value.IsDone)
			{
				value.WaitForCompletion();
			}
			if (value.Status == AsyncOperationStatus.Succeeded && value.Result != null)
			{
				return value.Result;
			}
			loadedHandles.Remove(addressableKey);
			if (value.IsValid())
			{
				Addressables.Release(value);
			}
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = LoadPrefabAsync(addressableKey);
		GameObject gameObject = asyncOperationHandle.WaitForCompletion();
		if (asyncOperationHandle.Status == AsyncOperationStatus.Failed || gameObject == null)
		{
			loadedHandles.Remove(addressableKey);
			if (asyncOperationHandle.IsValid())
			{
				Addressables.Release(asyncOperationHandle);
			}
			Debug.LogError("[WgoPartPool] Addressable load failed for [" + addressableKey + "]");
			return null;
		}
		loadedHandles[addressableKey] = asyncOperationHandle;
		return gameObject;
	}

	private async UniTask<GameObject> LoadPrefab(string addressableKey)
	{
		AsyncOperationHandle<GameObject> handle = LoadPrefabAsync(addressableKey);
		GameObject gameObject = await handle.ToUniTask();
		if (handle.Status == AsyncOperationStatus.Failed || gameObject == null)
		{
			loadedHandles.Remove(addressableKey);
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			Debug.LogError("[WgoPartPool] Addressable load failed for [" + addressableKey + "]");
			return null;
		}
		return gameObject;
	}

	private AsyncOperationHandle<GameObject> LoadPrefabAsync(string addressableKey)
	{
		if (loadedHandles.TryGetValue(addressableKey, out var value) && value.IsValid())
		{
			return value;
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(addressableKey);
		loadedHandles[addressableKey] = asyncOperationHandle;
		return asyncOperationHandle;
	}

	private void OnDestroy()
	{
		Clear();
	}

	private int TrimPathsInternal(IEnumerable<string> paths, ScenePoolTrimPolicy policy)
	{
		if (paths == null)
		{
			return 0;
		}
		int num = 0;
		foreach (string path in paths)
		{
			if (string.IsNullOrEmpty(path) || !pools.TryGetValue(path, out var value))
			{
				continue;
			}
			int num2 = ((policy != 0) ? LazySingletonSO<WgoPartPoolInitialSizesConfig>.Instance.GetSizeForPath(path) : 0);
			while (value.Count > num2)
			{
				WgoPart wgoPart = value.Pop();
				if (wgoPart != null)
				{
					UnityEngine.Object.Destroy(wgoPart.gameObject);
				}
				num++;
			}
			if (policy == ScenePoolTrimPolicy.Aggressive && value.Count == 0)
			{
				UnloadAddressablePrefab(path);
			}
		}
		return num;
	}

	private void UnloadAddressablePrefab(string path)
	{
		pools.Remove(path);
		if (loadedHandles.TryGetValue(path, out var value))
		{
			loadedHandles.Remove(path);
			if (value.IsValid())
			{
				Addressables.Release(value);
			}
		}
	}

	public void BakeSizesToInitialConfig()
	{
		foreach (string key in pools.Keys)
		{
			LazySingletonSO<WgoPartPoolInitialSizesConfig>.Instance.SetSizeForPath(key, pools[key].Count);
		}
	}

	public void Report(float value)
	{
		LocalProgress = Mathf.Clamp01(value);
	}
}
