using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ConstructorPartPool : LazySingleton<ConstructorPartPool>, IProgress<float>
{
	private const int YIELD_EVERY = 10;

	private Dictionary<string, Pool> poolsDict = new Dictionary<string, Pool>();

	private readonly Dictionary<string, AsyncOperationHandle<GameObject>> loadedHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();

	public float LocalProgress { get; private set; }

	public async UniTask InitAsync()
	{
		GameShutdown.ThrowIfRequested();
		Debug.Log("ConstructorPartPool InitAsync");
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		List<GameResAtom> list = LazySingletonSO<ConstructorPartPoolInitialSizesConfig>.Instance.initialSizeConfigs.List;
		int total = list.Count;
		int index = 0;
		foreach (GameResAtom item in list)
		{
			GameShutdown.ThrowIfRequested();
			CreatePoolById(item.type);
			index++;
			if (total > 0)
			{
				Report((float)index / (float)total);
			}
			await BackgroundLoading.YieldIfNeeded(index, 10);
		}
	}

	private void OnDestroy()
	{
		Clear();
	}

	public static ConstructorPartChildObject Get(string pathToLoad)
	{
		return LazySingleton<ConstructorPartPool>.Instance.GetInternal(pathToLoad);
	}

	public static void Release(string pathToLoad, ConstructorPartChildObject obj)
	{
		LazySingleton<ConstructorPartPool>.Instance.ReleaseInternal(pathToLoad, obj);
	}

	public static int TrimPaths(IEnumerable<string> paths, ScenePoolTrimPolicy policy)
	{
		return LazySingleton<ConstructorPartPool>.Instance.TrimPathsInternal(paths, policy);
	}

	public void BakeSizesToInitialConfig()
	{
		foreach (string key in poolsDict.Keys)
		{
			LazySingletonSO<ConstructorPartPoolInitialSizesConfig>.Instance.SetSizeForPath(key, poolsDict[key].Objects.Count);
		}
	}

	private ConstructorPartChildObject GetInternal(string pathToLoad)
	{
		if (!poolsDict.TryGetValue(pathToLoad, out var value))
		{
			value = CreatePoolById(pathToLoad);
			if (value == null)
			{
				return null;
			}
		}
		return value.GetOrCreateObject<ConstructorPartChildObject>();
	}

	private GameObject LoadPrefab(string pathToLoad)
	{
		if (GameShutdown.IsRequested)
		{
			return null;
		}
		AsyncOperationHandle<GameObject> value = Addressables.LoadAssetAsync<GameObject>(pathToLoad);
		value.WaitForCompletion();
		loadedHandles[pathToLoad] = value;
		return value.Result;
	}

	private async UniTask<GameObject> LoadPrefabAsync(string pathToLoad)
	{
		AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(pathToLoad);
		await handle.ToUniTask();
		loadedHandles[pathToLoad] = handle;
		return handle.Result;
	}

	private Pool CreatePoolById(string pathToLoad)
	{
		if (poolsDict.TryGetValue(pathToLoad, out var value))
		{
			return value;
		}
		GameObject gameObject = LoadPrefab(pathToLoad);
		if (gameObject == null)
		{
			return null;
		}
		return FinishCreatePool(pathToLoad, gameObject);
	}

	private async UniTask<Pool> CreatePoolByIdAsync(string pathToLoad)
	{
		return FinishCreatePool(pathToLoad, await LoadPrefabAsync(pathToLoad));
	}

	private Pool FinishCreatePool(string pathToLoad, GameObject obj)
	{
		if (poolsDict.TryGetValue(pathToLoad, out var value))
		{
			return value;
		}
		if (!obj.TryGetComponent<ConstructorPartChildObject>(out var component))
		{
			component = obj.AddComponent<ConstructorPartChildObject>();
		}
		Pool pool = LazyPooler.CreatePoolById(pathToLoad, component, LazySingletonSO<ConstructorPartPoolInitialSizesConfig>.Instance.GetSizeForPath(pathToLoad), Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		poolsDict.Add(pathToLoad, pool);
		return pool;
	}

	private void ReleaseInternal(string pathToLoad, ConstructorPartChildObject obj)
	{
		if (!(obj == null))
		{
			if (!poolsDict.TryGetValue(pathToLoad, out var value))
			{
				Debug.LogError($"Pool not found for path: {pathToLoad}, total pool count:[{poolsDict.Count}]");
			}
			else
			{
				value.ReleaseObject(obj);
			}
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
				int maxIdleCount = ((policy != 0) ? LazySingletonSO<ConstructorPartPoolInitialSizesConfig>.Instance.GetSizeForPath(path) : 0);
				num += value.TrimIdleToSize(maxIdleCount);
				if (policy == ScenePoolTrimPolicy.Aggressive && value.Objects.Count == 0)
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
