using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LazyBearTechnology.Preloader;

public class LazyAssetPreloader
{
	private struct LoadingAsset
	{
		public AsyncOperationHandle handle;

		public bool isLoading;
	}

	private readonly List<LoadingAsset> assetHandles = new List<LoadingAsset>();

	private int loadingAssetsCount;

	public bool IsLoading => loadingAssetsCount > 0;

	public void AddressablesLoadAssetAsync<TObject>(string name)
	{
		Debug.Log("Loading asset: " + name);
		loadingAssetsCount++;
		LoadingAsset assetHandle = new LoadingAsset
		{
			handle = Addressables.LoadAssetAsync<TObject>(name),
			isLoading = true
		};
		assetHandles.Add(assetHandle);
		assetHandle.handle.Completed += delegate(AsyncOperationHandle operationHandle)
		{
			loadingAssetsCount--;
			assetHandle.isLoading = false;
			if (operationHandle.OperationException != null)
			{
				Debug.LogError("Error loading asset: " + name + ", result: " + operationHandle.Result?.ToString() + ", exception: " + operationHandle.OperationException);
			}
			else
			{
				Debug.Log("Loading asset complete: " + name);
			}
		};
	}

	public int GetPercentLoaded()
	{
		if (assetHandles.Count == 0)
		{
			return 0;
		}
		return Mathf.FloorToInt(assetHandles.Sum((LoadingAsset handle) => (!handle.isLoading) ? 1f : handle.handle.PercentComplete) / (float)assetHandles.Count * 100f);
	}

	public IEnumerator WaitUntilLoaded()
	{
		while (loadingAssetsCount > 0)
		{
			yield return null;
		}
	}
}
