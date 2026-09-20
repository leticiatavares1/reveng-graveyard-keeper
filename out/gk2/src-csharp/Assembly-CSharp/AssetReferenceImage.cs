using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class AssetReferenceImage : MonoBehaviour
{
	[SerializeField]
	private AssetReferenceSprite assetReference;

	private Image image;

	private AsyncOperationHandle<Sprite> loadHandle;

	private Image Image
	{
		get
		{
			if (image == null)
			{
				image = GetComponent<Image>();
			}
			return image;
		}
	}

	private void OnEnable()
	{
		TryLoad();
	}

	private void OnDisable()
	{
		TryUnload();
	}

	private void TryLoad()
	{
		if (assetReference != null && !string.IsNullOrEmpty(assetReference.AssetGUID) && !loadHandle.IsValid())
		{
			Sprite sprite;
			try
			{
				loadHandle = assetReference.LoadAssetAsync<Sprite>();
				sprite = loadHandle.WaitForCompletion();
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to load sprite from AssetReference '{assetReference.RuntimeKey}': {ex.Message}", this);
				TryUnload();
				return;
			}
			if (sprite == null)
			{
				Debug.LogError($"Failed to load sprite from AssetReference '{assetReference.RuntimeKey}'", this);
				TryUnload();
			}
			else
			{
				Image.sprite = sprite;
			}
		}
	}

	internal void TryUnload()
	{
		if (Image.sprite != null)
		{
			Image.sprite = null;
		}
		if (loadHandle.IsValid())
		{
			if (assetReference != null && assetReference.IsValid())
			{
				assetReference.ReleaseAsset();
			}
			else
			{
				Addressables.Release(loadHandle);
			}
			loadHandle = default(AsyncOperationHandle<Sprite>);
		}
	}
}
