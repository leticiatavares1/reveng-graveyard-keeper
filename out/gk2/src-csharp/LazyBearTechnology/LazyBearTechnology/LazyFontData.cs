using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
[CreateAssetMenu(menuName = "LazyFont/Lazy Font Data", fileName = "LazyFontData")]
public class LazyFontData : ScriptableObject
{
	[Serializable]
	private class FontOverrideData
	{
		public string langId;

		public string fontAssetPath;

		public bool useOwnMaterial;

		private TMP_FontAsset fontAssetCache;

		public TMP_FontAsset GetFontAsset()
		{
			return LoadFontAsset(fontAssetPath, ref fontAssetCache);
		}
	}

	[SerializeField]
	private string fontAssetPath;

	[SerializeField]
	private List<FontOverrideData> assetOverrideByLang = new List<FontOverrideData>();

	private TMP_FontAsset fontAssetCache;

	public TMP_FontAsset GetFontAssetFor(string lang, bool staticFont, bool applyLanguagePackFont = true)
	{
		if (!staticFont)
		{
			if (applyLanguagePackFont && LanguageModHooks.TryGetFontAsset != null && LanguageModHooks.TryGetFontAsset(lang, staticFont: false, out var fontAsset))
			{
				return fontAsset;
			}
			for (int i = 0; i < assetOverrideByLang.Count; i++)
			{
				if (assetOverrideByLang[i].langId == lang)
				{
					return assetOverrideByLang[i].GetFontAsset();
				}
			}
		}
		return LoadFontAsset(fontAssetPath, ref fontAssetCache);
	}

	public bool IsFontUsesOwnMaterial(string lang)
	{
		if (LanguageModHooks.UsesOwnMaterial != null && LanguageModHooks.UsesOwnMaterial(lang))
		{
			return true;
		}
		for (int i = 0; i < assetOverrideByLang.Count; i++)
		{
			if (assetOverrideByLang[i].langId == lang)
			{
				return assetOverrideByLang[i].useOwnMaterial;
			}
		}
		return false;
	}

	private static TMP_FontAsset LoadFontAsset(string path, ref TMP_FontAsset cache)
	{
		if (cache != null)
		{
			return cache;
		}
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		cache = Resources.Load<TMP_FontAsset>(path);
		return cache;
	}
}
