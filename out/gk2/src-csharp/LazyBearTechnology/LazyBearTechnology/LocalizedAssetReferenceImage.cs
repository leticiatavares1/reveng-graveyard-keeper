using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace LazyBearTechnology;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class LocalizedAssetReferenceImage : MonoBehaviour
{
	[Serializable]
	private class LocalizedSpriteReference
	{
		[SerializeField]
		private string languageId;

		[SerializeField]
		private AssetReferenceSprite assetReference;

		public string LanguageId => languageId;

		public AssetReferenceSprite AssetReference => assetReference;
	}

	[SerializeField]
	private AssetReferenceSprite defaultAssetReference;

	[SerializeField]
	private List<LocalizedSpriteReference> localizedAssetReferences = new List<LocalizedSpriteReference>();

	private Image image;

	private AsyncOperationHandle<Sprite> loadHandle;

	private AssetReferenceSprite loadedAssetReference;

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
		Localize();
	}

	private void OnDisable()
	{
		TryUnload();
	}

	public void Localize()
	{
		if (base.isActiveAndEnabled)
		{
			TryLoad();
		}
	}

	private void TryLoad()
	{
		AssetReferenceSprite assetReferenceForCurrentLanguage = GetAssetReferenceForCurrentLanguage();
		if (assetReferenceForCurrentLanguage == loadedAssetReference && (loadHandle.IsValid() || !Application.isPlaying))
		{
			return;
		}
		TryUnload();
		if (assetReferenceForCurrentLanguage != null && !string.IsNullOrEmpty(assetReferenceForCurrentLanguage.AssetGUID))
		{
			Sprite sprite;
			try
			{
				loadHandle = assetReferenceForCurrentLanguage.LoadAssetAsync<Sprite>();
				loadedAssetReference = assetReferenceForCurrentLanguage;
				sprite = loadHandle.WaitForCompletion();
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to load localized sprite from AssetReference '{assetReferenceForCurrentLanguage.RuntimeKey}': {ex.Message}", this);
				TryUnload();
				return;
			}
			if (sprite == null)
			{
				Debug.LogError($"Failed to load localized sprite from AssetReference '{assetReferenceForCurrentLanguage.RuntimeKey}'", this);
				TryUnload();
			}
			else
			{
				Image.sprite = sprite;
			}
		}
	}

	private AssetReferenceSprite GetAssetReferenceForCurrentLanguage()
	{
		string currentLang = LLBase.CurrentLang;
		foreach (LocalizedSpriteReference localizedAssetReference in localizedAssetReferences)
		{
			if (localizedAssetReference != null && localizedAssetReference.AssetReference != null && !string.IsNullOrEmpty(localizedAssetReference.AssetReference.AssetGUID) && !(localizedAssetReference.LanguageId != currentLang))
			{
				return localizedAssetReference.AssetReference;
			}
		}
		return defaultAssetReference;
	}

	internal void TryUnload()
	{
		if (Image.sprite != null)
		{
			Image.sprite = null;
		}
		if (loadHandle.IsValid())
		{
			if (loadedAssetReference != null && loadedAssetReference.IsValid())
			{
				loadedAssetReference.ReleaseAsset();
			}
			else
			{
				Addressables.Release(loadHandle);
			}
			loadHandle = default(AsyncOperationHandle<Sprite>);
		}
		loadedAssetReference = null;
	}
}
