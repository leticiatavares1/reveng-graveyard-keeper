using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BakedChunkableObjectPool : LazySingleton<BakedChunkableObjectPool>, IProgress<float>
{
	private const int YIELD_EVERY = 10;

	private Dictionary<string, Pool> poolsDict = new Dictionary<string, Pool>();

	private readonly Dictionary<string, AsyncOperationHandle<GameObject>> loadedHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

	private readonly Dictionary<string, int> borrowedCounts = new Dictionary<string, int>();

	public float LocalProgress { get; private set; }

	public async UniTask InitAsync()
	{
		GameShutdown.ThrowIfRequested();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		int total = LazySingletonSO<BakedChunkableObjectPoolInitialSizesConfig>.Instance.initialSizeConfigs.List.Count;
		int index = 0;
		foreach (GameResAtom item in LazySingletonSO<BakedChunkableObjectPoolInitialSizesConfig>.Instance.initialSizeConfigs.List)
		{
			GameShutdown.ThrowIfRequested();
			CreatePoolById(item.type);
			Report((float)index / (float)total);
			index++;
			await BackgroundLoading.YieldIfNeeded(index, 10);
		}
	}

	public static BakedChunkableObjectComponent Get(string pathToLoad)
	{
		return LazySingleton<BakedChunkableObjectPool>.Instance.GetInternal(pathToLoad);
	}

	public static void Release(string pathToLoad, BakedChunkableObjectComponent obj)
	{
		LazySingleton<BakedChunkableObjectPool>.Instance.ReleaseInternal(pathToLoad, obj);
	}

	public static int TrimPaths(IEnumerable<string> paths, ScenePoolTrimPolicy policy)
	{
		return LazySingleton<BakedChunkableObjectPool>.Instance.TrimPathsInternal(paths, policy);
	}

	public void BakeSizesToInitialConfig()
	{
		foreach (string key in poolsDict.Keys)
		{
			LazySingletonSO<BakedChunkableObjectPoolInitialSizesConfig>.Instance.SetSizeForPath(key, poolsDict[key].Objects.Count);
		}
	}

	private BakedChunkableObjectComponent GetInternal(string pathToLoad)
	{
		if (!poolsDict.TryGetValue(pathToLoad, out var value))
		{
			value = CreatePoolById(pathToLoad);
			if (value == null)
			{
				return null;
			}
		}
		BakedChunkableObjectComponent orCreateObject = value.GetOrCreateObject<BakedChunkableObjectComponent>();
		borrowedCounts.TryGetValue(pathToLoad, out var value2);
		borrowedCounts[pathToLoad] = value2 + 1;
		return orCreateObject;
	}

	private Pool CreatePoolById(string pathToLoad)
	{
		if (GameShutdown.IsRequested)
		{
			return null;
		}
		AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(pathToLoad);
		handle.WaitForCompletion();
		return FinishCreatePool(pathToLoad, handle);
	}

	private async UniTask<Pool> CreatePoolByIdAsync(string pathToLoad)
	{
		AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(pathToLoad);
		await handle.ToUniTask();
		return FinishCreatePool(pathToLoad, handle);
	}

	private Pool FinishCreatePool(string pathToLoad, AsyncOperationHandle<GameObject> handle)
	{
		if (poolsDict.TryGetValue(pathToLoad, out var value))
		{
			Addressables.Release(handle);
			return value;
		}
		GameObject result = handle.Result;
		if (!result.TryGetComponent<BakedChunkableObjectComponent>(out var component))
		{
			component = result.AddComponent<BakedChunkableObjectComponentWithHorizontalSpr>();
		}
		Pool pool = LazyPooler.CreatePoolById(pathToLoad, component, 0, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		poolsDict.Add(pathToLoad, pool);
		loadedHandles[pathToLoad] = handle;
		int sizeForPath = LazySingletonSO<BakedChunkableObjectPoolInitialSizesConfig>.Instance.GetSizeForPath(pathToLoad);
		for (int i = 0; i < sizeForPath; i++)
		{
			pool.AddObjectToPool();
		}
		return pool;
	}

	private void ReleaseInternal(string pathToLoad, BakedChunkableObjectComponent obj)
	{
		if (borrowedCounts.TryGetValue(pathToLoad, out var value) && value > 0)
		{
			value--;
			if (value == 0)
			{
				borrowedCounts.Remove(pathToLoad);
			}
			else
			{
				borrowedCounts[pathToLoad] = value;
			}
		}
		if (!poolsDict.TryGetValue(pathToLoad, out var value2))
		{
			if (obj != null)
			{
				UnityEngine.Object.Destroy(obj.gameObject);
			}
		}
		else
		{
			value2.ReleaseObject(obj);
		}
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
			if (!string.IsNullOrEmpty(path) && poolsDict.TryGetValue(path, out var value))
			{
				int maxIdleCount = ((policy != 0) ? LazySingletonSO<BakedChunkableObjectPoolInitialSizesConfig>.Instance.GetSizeForPath(path) : 0);
				num += value.TrimIdleToSize(maxIdleCount);
				borrowedCounts.TryGetValue(path, out var value2);
				if (policy == ScenePoolTrimPolicy.Aggressive && value.Objects.Count == 0 && value2 <= 0)
				{
					UnloadAddressablePrefab(path);
				}
			}
		}
		return num;
	}

	private void UnloadAddressablePrefab(string path)
	{
		poolsDict.Remove(path);
		borrowedCounts.Remove(path);
		LazyPooler.RemovePoolById(path);
		if (loadedHandles.TryGetValue(path, out var value))
		{
			loadedHandles.Remove(path);
			if (value.IsValid())
			{
				Addressables.Release(value);
			}
		}
	}

	public void Report(float value)
	{
		LocalProgress = Mathf.Clamp01(value);
	}

	private void OnDestroy()
	{
		Clear();
	}

	private void Clear()
	{
		foreach (KeyValuePair<string, Pool> item in poolsDict)
		{
			foreach (MonoBehaviour @object in item.Value.Objects)
			{
				if (@object != null)
				{
					UnityEngine.Object.Destroy(@object.gameObject);
				}
			}
		}
		poolsDict.Clear();
		borrowedCounts.Clear();
		foreach (KeyValuePair<string, AsyncOperationHandle<GameObject>> loadedHandle in loadedHandles)
		{
			if (loadedHandle.Value.IsValid())
			{
				Addressables.Release(loadedHandle.Value);
			}
		}
		loadedHandles.Clear();
	}
}
