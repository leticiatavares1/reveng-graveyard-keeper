using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ChunkableObjectComponentStatic3DObjectAssetReference : ChunkableObjectComponent
{
	[SerializeField]
	private AssetReferenceGameObject assetReference;

	private AsyncOperationHandle<GameObject> loadedInstanceHandle;

	private int loadVersion;

	private Object3D object3D;

	private bool? lastAppliedShouldBeActive;

	protected override void ApplyVisibility()
	{
		bool shouldBeActive = base.ShouldBeActive;
		if (lastAppliedShouldBeActive != shouldBeActive)
		{
			lastAppliedShouldBeActive = shouldBeActive;
			Debug.Log(string.Format("[{0}] ApplyVisibility: {1} {2}", "ChunkableObjectComponentStatic3DObjectAssetReference", base.name, shouldBeActive));
			if (shouldBeActive)
			{
				LoadAssetAsync().Forget();
			}
			else
			{
				UnloadAsset();
			}
		}
	}

	private void Awake()
	{
		if (!GetComponentInParent<ChunkableObjectComponentStatic3DObjectAssetReference>())
		{
			StripEmbeddedObject3DHierarchy();
		}
	}

	public void StripEmbeddedObject3DHierarchy()
	{
		if (TryGetComponent<Object3D>(out var component))
		{
			DestroyObjectInternal(component);
			object3D = null;
			for (int num = base.transform.childCount - 1; num >= 0; num--)
			{
				DestroyObjectInternal(base.transform.GetChild(num).gameObject);
			}
		}
	}

	public override void CalculateChunkBounds()
	{
	}

	private static void DestroyObjectInternal(UnityEngine.Object obj)
	{
		if ((bool)obj)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	private async UniTaskVoid LoadAssetAsync()
	{
		Debug.Log(string.Format("[{0}] LoadAssetAsync: {1} {2}", "ChunkableObjectComponentStatic3DObjectAssetReference", base.name, loadVersion));
		if (loadedInstanceHandle.IsValid())
		{
			return;
		}
		int version = ++loadVersion;
		AsyncOperationHandle<GameObject> handle = (loadedInstanceHandle = assetReference.InstantiateAsync(base.transform));
		GameObject gameObject;
		try
		{
			gameObject = await handle.ToUniTask();
		}
		catch (Exception ex)
		{
			Debug.LogError("[ChunkableObjectComponentStatic3DObjectAssetReference] Failed to instantiate [" + base.name + "]: " + ex.Message, this);
			ReleaseHandleIfCurrent(handle);
			return;
		}
		if (version != loadVersion || !base.ShouldBeActive || !this)
		{
			ReleaseHandleIfCurrent(handle);
			return;
		}
		if (handle.Status != AsyncOperationStatus.Succeeded || !gameObject)
		{
			ReleaseHandleIfCurrent(handle);
			return;
		}
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.TryGetComponent<Object3D>(out object3D);
		if (gameObject.TryGetComponent<ChunkableObjectComponentStatic3DObjectAssetReference>(out var component))
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	private void UnloadAsset()
	{
		loadVersion++;
		if (loadedInstanceHandle.IsValid())
		{
			Addressables.ReleaseInstance(loadedInstanceHandle);
		}
		loadedInstanceHandle = default(AsyncOperationHandle<GameObject>);
		object3D = null;
	}

	private void ReleaseHandleIfCurrent(AsyncOperationHandle<GameObject> handle)
	{
		if (loadedInstanceHandle.Equals(handle))
		{
			if (handle.IsValid())
			{
				Addressables.ReleaseInstance(handle);
			}
			loadedInstanceHandle = default(AsyncOperationHandle<GameObject>);
			object3D = null;
		}
	}

	private void OnDestroy()
	{
		UnloadAsset();
	}
}
