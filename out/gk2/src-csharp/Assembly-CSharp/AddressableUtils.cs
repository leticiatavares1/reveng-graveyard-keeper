using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableUtils
{
	public static T LoadAssetReferenceSync<T>(AssetReferenceT<T> assetReference, ref AsyncOperationHandle<T> handle) where T : UnityEngine.Object
	{
		T result = null;
		if (handle.IsValid())
		{
			Addressables.Release(handle);
		}
		if (assetReference != null && assetReference.RuntimeKeyIsValid())
		{
			handle = assetReference.LoadAssetAsync<T>();
			try
			{
				if (GameShutdown.IsRequested)
				{
					if (handle.IsValid())
					{
						Addressables.Release(handle);
					}
					return null;
				}
				result = handle.WaitForCompletion();
				return result;
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to synchronously load asset with GUID '" + assetReference.AssetGUID + "': " + ex.Message);
				if (handle.IsValid())
				{
					Addressables.Release(handle);
				}
			}
		}
		return result;
	}

	public static void ReleaseAssetReference<T>(ref AsyncOperationHandle<T> handle) where T : class
	{
		if (handle.IsValid())
		{
			Addressables.Release(handle);
			handle = default(AsyncOperationHandle<T>);
		}
	}
}
