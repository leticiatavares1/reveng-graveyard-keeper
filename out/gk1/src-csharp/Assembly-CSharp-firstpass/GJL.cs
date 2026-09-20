using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class GJL : ScriptableObject
{
	private class CustomFont
	{
		public string filename;

		public int size;

		public int spacing_y;
	}

	private class LabelCacheData
	{
		public bool font_can_be_changed;

		public UIFont orig_bitmap_font;

		public int orig_spacing_y;

		public UILabel.Overflow orig_overflow_method;

		public int orig_font_size;

		public CustomFont current_font_settings;
	}

	public static readonly string[] LANGUAGES = new string[11]
	{
		"en", "de", "fr", "pt-br", "es", "ru", "it", "pl", "ja", "zh_cn",
		"ko"
	};

	[NonSerialized]
	public Dictionary<string, string> dict = new Dictionary<string, string>();

	public string id = "en";

	[SerializeField]
	protected List<string> txt_ids = new List<string>();

	[SerializeField]
	protected List<string> txts = new List<string>();

	protected static GJL cur_lng = null;

	public static readonly string[] AVAILABLE_LOCALES = new string[11]
	{
		"en", "de", "fr", "ru", "es", "pt-br", "it", "ja", "zh_cn", "ko",
		"pl"
	};

	public static readonly string[] AVAILABLE_LOCALE_NAMES = new string[11]
	{
		"English", "Deutsch", "Français", "Русский", "Español", "Português do Brasil", "Italiano", "Japanese", "Chinese", "Korean",
		"Polski"
	};

	public List<string> aliases_1 = new List<string>();

	public List<string> aliases_2 = new List<string>();

	private static readonly Dictionary<string, CustomFont> CUSTOM_FONTS = new Dictionary<string, CustomFont>
	{
		{
			"ja",
			new CustomFont
			{
				filename = "ngui_fonts/ch_jp_12",
				size = 12,
				spacing_y = 2
			}
		},
		{
			"zh_cn",
			new CustomFont
			{
				filename = "ngui_fonts/ch_jp_12",
				size = 12,
				spacing_y = 2
			}
		},
		{
			"ko",
			new CustomFont
			{
				filename = "ngui_fonts/korean",
				size = 13,
				spacing_y = 1
			}
		}
	};

	private static readonly Dictionary<UILabel, LabelCacheData> _labels_cache = new Dictionary<UILabel, LabelCacheData>();

	private static readonly Dictionary<string, UIFont> _loaded_fonts = new Dictionary<string, UIFont>();

	public static void LoadLanguageResource(string lng_id)
	{
		cur_lng = Resources.Load<GJL>("Locales/lng_" + lng_id);
		if (cur_lng == null && lng_id != "en")
		{
			LoadLanguageResource("en");
		}
		Debug.Log("LoadLanguageResource(\"" + lng_id + "\"), " + cur_lng.txts.Count + " lines loaded");
		cur_lng.InitHashDictionary();
	}

	public void InitHashDictionary()
	{
		dict.Clear();
		for (int i = 0; i < txt_ids.Count; i++)
		{
			dict.Add(txt_ids[i], txts[i]);
		}
	}

	public static string GetCurrentLocaleCode()
	{
		string text = GetLocaleCode().ToLower();
		switch (text)
		{
		case "ptbr":
			text = "pt-br";
			break;
		case "es-es":
		case "es-mx":
			text = "es";
			break;
		}
		if (!LANGUAGES.Contains(text))
		{
			Debug.LogWarning("Language '" + text + "' not found. Loading EN...");
			text = "en";
		}
		return text;
	}

	private static string GetLocaleCode()
	{
		switch (Application.systemLanguage)
		{
		case SystemLanguage.Belarusian:
		case SystemLanguage.Russian:
		case SystemLanguage.Ukrainian:
			return "ru";
		case SystemLanguage.German:
			return "de";
		case SystemLanguage.Portuguese:
			return "pt-br";
		case SystemLanguage.French:
			return "fr";
		case SystemLanguage.Spanish:
			return "es";
		case SystemLanguage.Japanese:
			return "ja";
		case SystemLanguage.Chinese:
			return "zh_cn";
		case SystemLanguage.Korean:
			return "ko";
		default:
			return "en";
		}
	}

	public static void LoadLangugage()
	{
		string text = GetLocaleCode();
		if (text == "ptbr")
		{
			text = "pt-br";
		}
		LoadLanguageResource(text);
	}

	public void AddLngString(string id, string txt)
	{
		int num = txt_ids.IndexOf(id);
		if (num == -1)
		{
			txt_ids.Add(id);
			txts.Add(txt);
		}
		else
		{
			txt_ids[num] = id;
			txts[num] = txt;
		}
	}

	public static string L(string lng_id)
	{
		if (lng_id == null)
		{
			Debug.LogError("lng_id is null");
			return "";
		}
		if (cur_lng != null)
		{
			int num = cur_lng.aliases_1.IndexOf(lng_id);
			if (num != -1)
			{
				return L(cur_lng.aliases_2[num]);
			}
		}
		string text = ((!(cur_lng == null) && cur_lng.dict.ContainsKey(lng_id)) ? cur_lng.dict[lng_id] : lng_id);
		text = text.Replace("&#xA;", "\n");
		return text.Replace("’", "'");
	}

	public static string L_colored(string lng_id, string v1, string color_1)
	{
		return L(lng_id).Replace("%1", "[" + color_1 + "]" + v1 + "[-]");
	}

	public static string L(string lng_id, string v1)
	{
		return L(lng_id).Replace("%1", v1);
	}

	public static string L(string lng_id, string v1, string v2)
	{
		return L(lng_id).Replace("%1", v1).Replace("%2", v2);
	}

	public static string L(string lng_id, string v1, string v2, string v3)
	{
		return L(lng_id).Replace("%1", v1).Replace("%2", v2).Replace("%3", v3);
	}

	public static string L(string lng_id, int v1)
	{
		return L(lng_id, v1.ToString());
	}

	public static string L(string lng_id, float v1)
	{
		return L(lng_id, v1.ToString());
	}

	public static string L(string lng_id, int v1, int v2)
	{
		return L(lng_id, v1.ToString(), v2.ToString());
	}

	public static string L(string lng_id, string v1, int v2)
	{
		return L(lng_id, v1.ToString(), v2.ToString());
	}

	public static string L(string lng_id, int v1, string v2)
	{
		return L(lng_id, v1.ToString(), v2.ToString());
	}

	public static string L(string lng_id, float v1, float v2)
	{
		return L(lng_id, v1.ToString(), v2.ToString());
	}

	public static string L(string lng_id, string v1, float v2)
	{
		return L(lng_id, v1.ToString(), v2.ToString());
	}

	public static string L(string lng_id, float v1, string v2)
	{
		return L(lng_id, v1.ToString(), v2.ToString());
	}

	public static string L(string lng_id, string v1, int v2, int v3)
	{
		return L(lng_id, v1.ToString(), v2.ToString(), v3.ToString());
	}

	public static string GetLocaleNameByCode(string locale_code)
	{
		for (int i = 0; i < AVAILABLE_LOCALES.Length; i++)
		{
			if (AVAILABLE_LOCALES[i] == locale_code)
			{
				return AVAILABLE_LOCALE_NAMES[i];
			}
		}
		return "?";
	}

	public static string GetCurLng()
	{
		return cur_lng.id;
	}

	public static bool IsEastern()
	{
		if (cur_lng == null)
		{
			return false;
		}
		if (!(cur_lng.id == "ja") && !(cur_lng.id == "zh_cn"))
		{
			return cur_lng.id == "ko";
		}
		return true;
	}

	public static void EnsureChildLabelsHasCorrectFont(GameObject go, bool do_cache = true)
	{
		UILabel[] componentsInChildren = go.GetComponentsInChildren<UILabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			EnsureLabelHasCorrectFont(componentsInChildren[i], do_cache);
		}
	}

	public static void ApplyCustomFontSettings(GameObject go)
	{
		CustomFont value = null;
		CUSTOM_FONTS.TryGetValue(cur_lng.id, out value);
		CustomFontSettings[] componentsInChildren = go.GetComponentsInChildren<CustomFontSettings>(includeInactive: true);
		foreach (CustomFontSettings customFontSettings in componentsInChildren)
		{
			if (!(customFontSettings.GetComponent<UILabel>() != null))
			{
				if (value == null)
				{
					customFontSettings.Restore();
				}
				else
				{
					customFontSettings.Apply();
				}
			}
		}
	}

	public static void EnsureLabelHasCorrectFont(UILabel label, bool do_cache = true)
	{
		if (!_labels_cache.TryGetValue(label, out var value))
		{
			value = new LabelCacheData
			{
				font_can_be_changed = (label.GetComponent<StaticFontLabel>() == null),
				orig_bitmap_font = label.bitmapFont,
				orig_spacing_y = label.spacingY,
				orig_overflow_method = label.overflowMethod,
				orig_font_size = label.fontSize,
				current_font_settings = null
			};
			if (do_cache)
			{
				_labels_cache.Add(label, value);
			}
		}
		if (!value.font_can_be_changed)
		{
			return;
		}
		CustomFont value2 = null;
		CUSTOM_FONTS.TryGetValue(cur_lng.id, out value2);
		if (value.current_font_settings == value2)
		{
			return;
		}
		CustomFontSettings component = label.GetComponent<CustomFontSettings>();
		value.current_font_settings = value2;
		if (value2 == null)
		{
			label.bitmapFont = value.orig_bitmap_font;
			label.spacingY = value.orig_spacing_y;
			label.overflowMethod = value.orig_overflow_method;
			label.fontSize = value.orig_font_size;
			if (component != null)
			{
				component.Restore();
			}
			return;
		}
		label.fontSize = value2.size;
		UIFont value3 = null;
		if (!_loaded_fonts.TryGetValue(value2.filename, out value3))
		{
			value3 = Resources.Load<UIFont>(value2.filename);
			if (value3 == null)
			{
				Debug.LogError("Couldn't load a custom font: " + value2.filename);
				return;
			}
			_loaded_fonts.Add(value2.filename, value3);
		}
		label.bitmapFont = value3;
		label.spacingY = value2.spacing_y;
		if (component != null)
		{
			component.Apply();
		}
	}
}
