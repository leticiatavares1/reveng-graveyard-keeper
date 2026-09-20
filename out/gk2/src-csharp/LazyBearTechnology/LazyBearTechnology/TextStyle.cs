using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

[CreateAssetMenu(menuName = "LazyFont/Text Style", fileName = "TextStyle")]
public class TextStyle : ScriptableObject
{
	public LazyFontData lazyFont;

	public TextStyleData data;

	public List<OverrideTextStyleData> overrideDataList = new List<OverrideTextStyleData>();

	[NonSerialized]
	private Dictionary<string, Material> cachedMaterialsByDynamicLangId = new Dictionary<string, Material>();

	[NonSerialized]
	private Dictionary<string, Material> cachedMaterialsByStaticLangId = new Dictionary<string, Material>();

	public LazyFontData Font => lazyFont;

	public static void ClearDynamicMaterialCaches()
	{
		TextStyle[] array = Resources.FindObjectsOfTypeAll<TextStyle>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ClearDynamicMaterialCache();
		}
	}

	private void ClearDynamicMaterialCache()
	{
		DestroyCachedMaterials(cachedMaterialsByDynamicLangId);
	}

	private static void DestroyCachedMaterials(Dictionary<string, Material> dictionary)
	{
		foreach (Material value in dictionary.Values)
		{
			if (value != null)
			{
				UnityEngine.Object.DestroyImmediate(value, allowDestroyingAssets: true);
			}
		}
		dictionary.Clear();
	}

	public void ApplyStyle(TMP_Text label, bool staticFont = false, Color? overrideColor = null, Color? overrideOutlineColor = null, Color? overrideSecondOutlineColor = null)
	{
		string currentLang = LLBase.CurrentLang;
		ApplyStyleAndLanguage(label, currentLang, staticFont, overrideColor, overrideOutlineColor, overrideSecondOutlineColor);
	}

	public void ApplyStyleAndLanguage(TMP_Text label, string lang, bool staticFont = false, Color? overrideColor = null, Color? overrideOutlineColor = null, Color? overrideSecondOutlineColor = null)
	{
		if (label == null)
		{
			Debug.LogError("Error in ApplyStyle(): Label is null");
			return;
		}
		staticFont = staticFont || (data != null && data.isAlwaysStaticFont) || label.GetComponent<StaticFontLabel>() != null;
		Material material = null;
		label.font = Font.GetFontAssetFor(lang, staticFont);
		LanguageModHooks.ApplyDirection?.Invoke(label, lang, staticFont);
		material = ((Font.IsFontUsesOwnMaterial(lang) && !staticFont) ? GetMaterialFor(label.font.material, lang, overrideOutlineColor, overrideSecondOutlineColor) : GetMaterialFor(lang, staticFont, overrideOutlineColor, overrideSecondOutlineColor));
		SetColor(label, lang, overrideColor);
		if (data != null && HasTextStyleData(lang, (TextStyleData style) => style.containsLineSpacing, out var result))
		{
			label.lineSpacing = result.lineSpacing;
		}
		label.fontSharedMaterial = material;
		label.ForceMeshUpdate();
		LayoutRebuilder.ForceRebuildLayoutImmediate(label.rectTransform);
	}

	public string ApplyStyleToString(string str, bool staticFont = false, bool includeFontTag = true)
	{
		string currentLang = LLBase.CurrentLang;
		string text = string.Empty;
		if (staticFont || !Font.IsFontUsesOwnMaterial(currentLang))
		{
			int num = 0;
			text = (staticFont ? (currentLang + ":" + base.name + "_static") : (currentLang + ":" + base.name + "_dynamic"));
			for (int i = 0; i < text.Length; i++)
			{
				num = ((num << 5) + num) ^ TMP_TextParsingUtilities.ToUpperASCIIFast(text[i]);
			}
			if (!MaterialReferenceManager.TryGetMaterial(num, out var material))
			{
				material = GetMaterialFor(currentLang, staticFont, null, null, applyLanguagePackFont: false);
				MaterialReferenceManager.AddFontMaterial(num, material);
			}
		}
		string text2 = "color=#" + ColorUtility.ToHtmlStringRGB(GetColorForLabel(currentLang));
		string text3 = string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			text3 = " material=" + text;
		}
		string text4 = Font.GetFontAssetFor(currentLang, staticFont, applyLanguagePackFont: false).name;
		string text5 = (includeFontTag ? ("<font=\"" + text4 + "\"" + text3 + ">") : "");
		text5 = text5 + "<" + text2 + ">" + str + "</color>";
		if (includeFontTag)
		{
			text5 += "</font>";
		}
		return text5;
	}

	public string TranslateAndColorizeTags(string localeKey)
	{
		return ColorizeTags(LLBase.L(localeKey));
	}

	public string ColorizeTags(string str)
	{
		int num = str.IndexOf('[');
		int num2 = str.IndexOf(']');
		while (num != -1 && num2 != -1 && num < num2)
		{
			string str2 = str.Substring(num + 1, num2 - num - 1);
			str2 = ApplyStyleToString(str2);
			str = str.Replace(str.Substring(num, num2 - num + 1), str2);
			num = str.IndexOf('[');
			num2 = str.IndexOf(']');
		}
		return str;
	}

	private Material GetMaterialFor(string lang, bool staticFont, Color? overrideOutlineColor = null, Color? overrideSecondOutlineColor = null, bool applyLanguagePackFont = true)
	{
		Dictionary<string, Material> dictionary = (staticFont ? cachedMaterialsByStaticLangId : cachedMaterialsByDynamicLangId);
		string text = lang;
		if (!applyLanguagePackFont)
		{
			text += ":style";
		}
		if (overrideOutlineColor.HasValue)
		{
			text += overrideOutlineColor.Value;
		}
		if (overrideSecondOutlineColor.HasValue)
		{
			text += overrideSecondOutlineColor.Value;
		}
		if (!dictionary.TryGetValue(text, out var value))
		{
			value = CreateMaterial(lang, staticFont, overrideOutlineColor, overrideSecondOutlineColor, applyLanguagePackFont);
			dictionary.Add(text, value);
		}
		return value;
	}

	private Material GetMaterialFor(Material fontMaterial, string lang, Color? overrideOutlineColor = null, Color? overrideSecondOutlineColor = null)
	{
		Dictionary<string, Material> dictionary = cachedMaterialsByDynamicLangId;
		string text = lang;
		if (overrideOutlineColor.HasValue)
		{
			text += overrideOutlineColor.Value;
		}
		if (overrideSecondOutlineColor.HasValue)
		{
			text += overrideSecondOutlineColor.Value;
		}
		if (!dictionary.TryGetValue(text, out var value))
		{
			value = CreateMaterial(fontMaterial, LLBase.CurrentLang, overrideOutlineColor, overrideSecondOutlineColor);
			dictionary.Add(text, value);
		}
		return value;
	}

	private void SetColor(TMP_Text label, string lang, Color? overrideColor = null)
	{
		Color colorForLabel = GetColorForLabel(lang, overrideColor);
		if (colorForLabel != default(Color))
		{
			label.color = colorForLabel;
		}
	}

	private Color GetColorForLabel(string lang, Color? overrideColor = null)
	{
		OverrideTextStyleData overrideTextStyleData = overrideDataList.Find((OverrideTextStyleData x) => x.languages.Contains(lang) && x.data.containsFontColor);
		if (overrideTextStyleData != null)
		{
			return overrideTextStyleData.data.fontColor;
		}
		if (data.containsFontColor)
		{
			return data.fontColor;
		}
		if (overrideColor.HasValue)
		{
			return overrideColor.Value;
		}
		return default(Color);
	}

	private Material CreateMaterial(string lang, bool staticFont, Color? overrideOutlineColor = null, Color? overrideSecondOutlineColor = null, bool applyLanguagePackFont = true)
	{
		TextStyleData result;
		bool num = HasTextStyleData(lang, (TextStyleData style) => style.containsOutline, out result);
		TextStyleData result2;
		bool flag = HasTextStyleData(lang, (TextStyleData style) => style.containsSecondOutline, out result2);
		TextStyleData result3;
		bool flag2 = HasTextStyleData(lang, (TextStyleData style) => style.containsShadow, out result3);
		TextStyleData result4;
		bool flag3 = HasTextStyleData(lang, (TextStyleData style) => style.containsOverlayTexture, out result4);
		bool eightSide = num && result.eightSide;
		Material material = new Material(TMPShaderSetup.FindShader(num, flag, flag2, flag3, eightSide))
		{
			name = lang + ":[" + base.name + "]"
		};
		TMP_FontAsset fontAssetFor = Font.GetFontAssetFor(lang, staticFont, applyLanguagePackFont);
		material.SetTexture("_MainTex", fontAssetFor.atlasTexture);
		material.SetFloat("_MainTexSizeX", fontAssetFor.atlasWidth);
		material.SetFloat("_MainTexSizeY", fontAssetFor.atlasHeight);
		if (num)
		{
			material.SetFloat("_Outline", 1f);
			material.SetColor("_OutlineColor", overrideOutlineColor ?? result.outlineColor);
		}
		if (flag)
		{
			material.SetFloat("_Outline", 1f);
			material.SetColor("_OutlineColor2", overrideSecondOutlineColor ?? result2.secondOutlineColor);
		}
		if (flag2)
		{
			material.SetInt("_UseShadow", 1);
			material.SetColor("_ShadowColor", result3.shadowOutlineColor);
		}
		if (flag3)
		{
			material.SetTexture("_FaceTex", result4.overlayTexture);
		}
		return material;
	}

	private Material CreateMaterial(Material fontMaterial, string lang, Color? overrideOutlineColor = null, Color? overrideSecondOutlineColor = null)
	{
		TextStyleData result;
		bool num = HasTextStyleData(lang, (TextStyleData style) => style.containsOutline, out result);
		HasTextStyleData(lang, (TextStyleData style) => style.containsSecondOutline, out var _);
		HasTextStyleData(lang, (TextStyleData style) => style.containsShadow, out var _);
		HasTextStyleData(lang, (TextStyleData style) => style.containsOverlayTexture, out var _);
		Material material = new Material(fontMaterial);
		if (num)
		{
			material.EnableKeyword("UNDERLAY_ON");
			material.SetColor("_UnderlayColor", LinearizeColor(overrideOutlineColor ?? result.outlineColor));
			material.SetFloat("_UnderlayDilate", 0.7f);
		}
		else
		{
			material.DisableKeyword("UNDERLAY_ON");
		}
		return material;
	}

	private bool HasTextStyleData(string lang, Func<TextStyleData, bool> condition, out TextStyleData result)
	{
		OverrideTextStyleData overrideTextStyleData = overrideDataList.Find((OverrideTextStyleData x) => x.languages.Contains(lang) && condition(x.data));
		if (overrideTextStyleData != null)
		{
			result = overrideTextStyleData.data;
			return true;
		}
		if (condition(data))
		{
			result = data;
			return true;
		}
		result = null;
		return false;
	}

	private float LinearizeColorComponent(float sRGBValue)
	{
		if (!((double)sRGBValue <= 0.04045))
		{
			return Mathf.Pow((sRGBValue + 0.055f) / 1.055f, 2.4f);
		}
		return sRGBValue / 12.92f;
	}

	private Color LinearizeColor(Color sRGBColor)
	{
		return new Color(LinearizeColorComponent(sRGBColor.r), LinearizeColorComponent(sRGBColor.g), LinearizeColorComponent(sRGBColor.b), sRGBColor.a);
	}
}
