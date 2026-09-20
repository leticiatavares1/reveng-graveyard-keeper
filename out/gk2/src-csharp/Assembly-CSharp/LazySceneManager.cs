using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public static class LazySceneManager
{
	private static List<LazySceneInfo> lazySceneInfos = new List<LazySceneInfo>();

	public static string LastLoadedSceneId
	{
		get
		{
			if (lazySceneInfos.Count == 0)
			{
				return "MainScene";
			}
			List<LazySceneInfo> list = lazySceneInfos;
			return list[list.Count - 1].sceneId;
		}
	}

	public static void LoadScene(string sceneId, Action<SceneInstance> loadCompleteCallback = null)
	{
		LazySceneInfo sceneInfo = new LazySceneInfo();
		sceneInfo.sceneId = sceneId;
		sceneInfo.status = SceneStatus.Loading;
		sceneInfo.onLoadedCallback = loadCompleteCallback;
		lazySceneInfos.Add(sceneInfo);
		AsyncOperationHandle<SceneInstance> asyncOperationHandle = Addressables.LoadSceneAsync(sceneId ?? "", LoadSceneMode.Additive);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<SceneInstance> handle)
		{
			OnSceneLoaded(handle, sceneInfo);
		};
	}

	public static async UniTask<SceneInstance> LoadSceneAsync(string sceneId, IProgress<float> progress = null)
	{
		LazySceneInfo sceneInfo = new LazySceneInfo
		{
			sceneId = sceneId,
			status = SceneStatus.Loading
		};
		lazySceneInfos.Add(sceneInfo);
		AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneId ?? "", LoadSceneMode.Additive);
		sceneInfo.sceneHandle = handle;
		SceneInstance sceneInstance = await handle.ToUniTask(progress);
		if (handle.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError("LazySceneManager: error during scene [" + sceneId + "] async loading, unsuccessful.");
			sceneInfo.status = SceneStatus.LoadingFailed;
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			lazySceneInfos.Remove(sceneInfo);
			throw new Exception("Failed to load scene [" + sceneId + "]");
		}
		sceneInfo.sceneInstance = sceneInstance;
		sceneInfo.status = SceneStatus.Loaded;
		return sceneInstance;
	}

	public static AsyncOperationHandle UnloadScene(string sceneId, Action<SceneInstance> unloadStartCallback = null, Action unloadCompleteCallback = null)
	{
		AsyncOperationHandle result = default(AsyncOperationHandle);
		for (int i = 0; i < lazySceneInfos.Count; i++)
		{
			LazySceneInfo sceneInfo = lazySceneInfos[i];
			if (!(sceneInfo.sceneId == sceneId))
			{
				continue;
			}
			if (!sceneInfo.sceneHandle.IsValid())
			{
				Debug.LogError("LazySceneManager: scene [" + sceneId + "] has no valid load handle, cannot unload.");
				break;
			}
			result = Addressables.UnloadSceneAsync(sceneInfo.sceneHandle, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
			unloadStartCallback?.Invoke(sceneInfo.sceneInstance);
			result.Completed += delegate(AsyncOperationHandle handle)
			{
				OnSceneUnloaded(handle, sceneInfo, unloadCompleteCallback);
			};
			sceneInfo.status = SceneStatus.Unloading;
			break;
		}
		return result;
	}

	public static bool IsSceneUnloading(string sceneId)
	{
		foreach (LazySceneInfo lazySceneInfo in lazySceneInfos)
		{
			if (lazySceneInfo.sceneId == sceneId)
			{
				return lazySceneInfo.status == SceneStatus.Unloading;
			}
		}
		return false;
	}

	public static bool IsSceneLoadingOrLoaded(string sceneId)
	{
		foreach (LazySceneInfo lazySceneInfo in lazySceneInfos)
		{
			if (lazySceneInfo.sceneId == sceneId)
			{
				return true;
			}
		}
		return false;
	}

	public static async UniTask<SceneInstance> AwaitSceneLoadedAsync(string sceneId)
	{
		while (true)
		{
			foreach (LazySceneInfo lazySceneInfo in lazySceneInfos)
			{
				if (!(lazySceneInfo.sceneId != sceneId))
				{
					switch (lazySceneInfo.status)
					{
					case SceneStatus.LoadingFailed:
						throw new Exception("Scene [" + sceneId + "] failed to load");
					case SceneStatus.Loaded:
						return lazySceneInfo.sceneInstance;
					}
				}
			}
			await UniTask.Yield();
		}
	}

	private static void OnSceneLoaded(AsyncOperationHandle<SceneInstance> asyncOperationHandle, LazySceneInfo sceneInfo)
	{
		sceneInfo.sceneHandle = asyncOperationHandle;
		if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogError("LazySceneManager: error during scene [" + sceneInfo.sceneId + "] loading callback, unsuccessful.");
			sceneInfo.status = SceneStatus.LoadingFailed;
			sceneInfo.onLoadedCallback = null;
			if (asyncOperationHandle.IsValid())
			{
				Addressables.Release(asyncOperationHandle);
			}
			lazySceneInfos.Remove(sceneInfo);
		}
		else
		{
			sceneInfo.sceneInstance = asyncOperationHandle.Result;
			sceneInfo.status = SceneStatus.Loaded;
			sceneInfo.onLoadedCallback?.Invoke(sceneInfo.sceneInstance);
			sceneInfo.onLoadedCallback = null;
		}
	}

	private static void OnSceneUnloaded(AsyncOperationHandle operationHandle, LazySceneInfo sceneInfo, Action OnUnloaded)
	{
		Debug.Log(string.Format("{0} Status: {1}", "OnSceneUnloaded", operationHandle.Status));
		if (operationHandle.Status == AsyncOperationStatus.Succeeded)
		{
			lazySceneInfos.Remove(sceneInfo);
		}
		else
		{
			sceneInfo.status = SceneStatus.Loaded;
		}
		OnUnloaded?.Invoke();
	}
}
