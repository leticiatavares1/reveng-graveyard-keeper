using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WorldFXPool : LazySingleton<WorldFXPool>, IProgress<float>
{
	private const int YIELD_EVERY = 10;

	private Dictionary<string, Pool> poolsDict = new Dictionary<string, Pool>();

	private static readonly Dictionary<string, AsyncOperationHandle<GameObject>> loadedPrefabHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

	public float LocalProgress { get; private set; }

	public static bool TryLoadPrefab(string fxName, out GameObject prefab)
	{
		prefab = null;
		if (string.IsNullOrEmpty(fxName))
		{
			return false;
		}
		if (loadedPrefabHandles.TryGetValue(fxName, out var value))
		{
			if (value.IsValid() && value.Status == AsyncOperationStatus.Succeeded && value.Result != null)
			{
				prefab = value.Result;
				return true;
			}
			loadedPrefabHandles.Remove(fxName);
			if (value.IsValid())
			{
				Addressables.Release(value);
			}
		}
		if (GameShutdown.IsRequested)
		{
			return false;
		}
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(fxName + ".prefab");
		asyncOperationHandle.WaitForCompletion();
		if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded || asyncOperationHandle.Result == null)
		{
			if (asyncOperationHandle.IsValid())
			{
				Addressables.Release(asyncOperationHandle);
			}
			return false;
		}
		loadedPrefabHandles[fxName] = asyncOperationHandle;
		prefab = asyncOperationHandle.Result;
		return true;
	}

	public async UniTask InitAsync()
	{
		GameShutdown.ThrowIfRequested();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		int total = LazySingletonSO<WorldFXPoolInitialSizeConfig>.Instance.initialSizeConfigs.List.Count;
		int index = 0;
		foreach (GameResAtom item in LazySingletonSO<WorldFXPoolInitialSizeConfig>.Instance.initialSizeConfigs.List)
		{
			GameShutdown.ThrowIfRequested();
			CreatePoolById(item.type);
			Report((float)index / (float)total);
			index++;
			await BackgroundLoading.YieldIfNeeded(index, 10);
		}
	}

	public static WorldFX Get(string fxName)
	{
		return LazySingleton<WorldFXPool>.Instance.GetInternal(fxName);
	}

	public static void Release(string fxName, WorldFX obj)
	{
		LazySingleton<WorldFXPool>.Instance.ReleaseInternal(fxName, obj);
	}

	public void BakeSizesToInitialConfig()
	{
		foreach (string key in poolsDict.Keys)
		{
			LazySingletonSO<WorldFXPoolInitialSizeConfig>.Instance.SetSizeForName(key, poolsDict[key].Objects.Count);
		}
	}

	private WorldFX GetInternal(string fxName)
	{
		if (!poolsDict.TryGetValue(fxName, out var value))
		{
			value = CreatePoolById(fxName);
			if (value == null)
			{
				return null;
			}
		}
		return value.GetOrCreateInactiveObject<WorldFX>();
	}

	private Pool CreatePoolById(string fxName)
	{
		if (!TryLoadPrefab(fxName, out var prefab))
		{
			Debug.LogError("WorldFX prefab not found: [" + fxName + "]");
			return null;
		}
		return FinishCreatePool(fxName, prefab);
	}

	private Pool FinishCreatePool(string fxName, GameObject obj)
	{
		if (poolsDict.TryGetValue(fxName, out var value))
		{
			return value;
		}
		if (!obj.TryGetComponent<WorldFX>(out var component))
		{
			component = obj.AddComponent<WorldFX>();
		}
		Pool pool = LazyPooler.CreatePoolById(fxName, component, 0, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		int sizeForName = LazySingletonSO<WorldFXPoolInitialSizeConfig>.Instance.GetSizeForName(fxName);
		for (int i = 0; i < sizeForName; i++)
		{
			pool.AddInactiveObjectToPool();
		}
		poolsDict.Add(fxName, pool);
		return pool;
	}

	private void ReleaseInternal(string fxName, WorldFX obj)
	{
		poolsDict[fxName].ReleaseObject(obj);
	}

	public void Report(float value)
	{
		LocalProgress = Mathf.Clamp01(value);
	}
}
