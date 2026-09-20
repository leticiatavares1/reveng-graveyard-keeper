using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LinqTools;
using UnityEngine;

namespace LazyBearTechnology;

public abstract class LLBase : ScriptableObject
{
	public class LanguageInfo
	{
		public string id;

		public string iso;

		public string name;

		public LanguageInfo(string id, string iso, string name)
		{
			this.id = id;
			this.iso = iso;
			this.name = name;
		}
	}

	[Serializable]
	public class JsonEntry
	{
		public string key;

		public string value;
	}

	[Serializable]
	public class JsonData
	{
		public List<JsonEntry> data = new List<JsonEntry>();
	}

	public const string DEFAULT_LANGUAGE = "en";

	public static readonly Dictionary<string, LanguageInfo> languages = new Dictionary<string, LanguageInfo>
	{
		{
			"en",
			new LanguageInfo("en", "en-US", "English")
		},
		{
			"de",
			new LanguageInfo("de", "de-DE", "Deutsch")
		},
		{
			"fr",
			new LanguageInfo("fr", "fr-FR", "Français")
		},
		{
			"pt-br",
			new LanguageInfo("pt-br", "pt-BR", "Português do Brasil")
		},
		{
			"es",
			new LanguageInfo("es", "es-ES", "Español")
		},
		{
			"es-mx",
			new LanguageInfo("es", "es-MX", "Español")
		},
		{
			"ru",
			new LanguageInfo("ru", "ru-RU", "Русский")
		},
		{
			"uk-ua",
			new LanguageInfo("uk-ua", "uk-UA", "Україньска")
		},
		{
			"it",
			new LanguageInfo("it", "it-IT", "Italiano")
		},
		{
			"pl",
			new LanguageInfo("pl", "pl-PL", "Polski")
		},
		{
			"tr",
			new LanguageInfo("tr", "tr-TR", "Türkçe")
		},
		{
			"ja",
			new LanguageInfo("ja", "ja-JP", "Japanese")
		},
		{
			"zh_cn",
			new LanguageInfo("zh_cn", "zh-CN", "Chinese")
		},
		{
			"zh_cht",
			new LanguageInfo("zh_cht", "zh-CHT", "Chinese-Traditional")
		},
		{
			"ko",
			new LanguageInfo("ko", "ko-KR", "Korean")
		},
		{
			"th",
			new LanguageInfo("th", "th-TH", "Siamese")
		},
		{
			"vn",
			new LanguageInfo("vn", "vn-VN", "tiếng Việt")
		}
	};

	private static readonly HashSet<string> easternLangIds = new HashSet<string> { "ja", "zh_cn", "zh_cht", "ko", "th", "vn" };

	private static readonly HashSet<string> ellipsisFriendlyLangIds = new HashSet<string> { "ja", "zh_cn", "ko" };

	[NonSerialized]
	public Dictionary<string, string> dictionary = new Dictionary<string, string>();

	[NonSerialized]
	public Dictionary<string, NestedLocalesMetaInfo> idsToMetaInfo = new Dictionary<string, NestedLocalesMetaInfo>();

	[NonSerialized]
	public Dictionary<string, ReplacementKeysMetadata> replacementIdMetaInfo = new Dictionary<string, ReplacementKeysMetadata>();

	public List<string> aliases1 = new List<string>();

	public List<string> aliases2 = new List<string>();

	public string id = "en";

	[SerializeField]
	protected List<string> txtIds = new List<string>();

	[SerializeField]
	protected List<string> txts = new List<string>();

	[SerializeField]
	public List<NestedLocalesMetaInfo> nestedLocalesMetaInfos = new List<NestedLocalesMetaInfo>();

	[SerializeField]
	public List<ReplacementKeysMetadata> replacementKeysMetaInfoList = new List<ReplacementKeysMetadata>();

	protected static BaseReplacementRuleSet replacementRuleSet = new BaseReplacementRuleSet();

	protected static LL currentLang = null;

	public static string CurrentLang => currentLang?.id ?? "en";

	public static bool IsCurrentLangLoaded => currentLang != null;

	public static Dictionary<string, LanguageInfo> AvailableLanguages
	{
		get
		{
			if (currentLang == null)
			{
				currentLang = new LL();
			}
			return currentLang.GetAvailableLanguages();
		}
	}

	public int TextEntryCount => txtIds.Count;

	public static void InitReplacementRuleSet(BaseReplacementRuleSet replacementRuleSet)
	{
		LLBase.replacementRuleSet = replacementRuleSet;
	}

	public static void LoadLanguageResource(string langId)
	{
		if (LanguageModHooks.TryGetLanguage != null && LanguageModHooks.TryGetLanguage(langId, out var language) && language != null)
		{
			currentLang = language;
			currentLang.InitHashDictionary();
			Debug.Log("LoadLanguageResource(\"" + langId + "\") [mod], " + currentLang.txts.Count + " lines loaded");
			return;
		}
		currentLang = Resources.Load<LL>("Locales/lng_" + langId);
		if (currentLang == null && langId != "en")
		{
			LoadLanguageResource("en");
		}
		Debug.Log("LoadLanguageResource(\"" + langId + "\"), " + currentLang.txts.Count + " lines loaded");
		currentLang.InitHashDictionary();
	}

	public string GetTextIdAt(int index)
	{
		return txtIds[index];
	}

	public string GetTextWithMarkup(int index)
	{
		string text = txts[index];
		if (index < nestedLocalesMetaInfos.Count && nestedLocalesMetaInfos[index].hasMetaInfo)
		{
			int num = 0;
			for (int i = 0; i < nestedLocalesMetaInfos[index].idsToInsert.Count; i++)
			{
				string text2 = "#(" + nestedLocalesMetaInfos[index].localeIDsToInsert[i] + ")";
				text = text.Insert(nestedLocalesMetaInfos[index].idsToInsert[i] + num, text2);
				num += text2.Length;
			}
		}
		ReplacementKeysMetadata replacementKeysMetadata = replacementKeysMetaInfoList.Find((ReplacementKeysMetadata x) => x.id == txtIds[index]);
		if (replacementKeysMetadata != null)
		{
			int num2 = 0;
			for (int j = 0; j < replacementKeysMetadata.keys.Count; j++)
			{
				string text3 = "@(" + replacementKeysMetadata.keys[j] + ")";
				text = text.Insert(replacementKeysMetadata.keysIndexes[j] + num2, text3);
				num2 += text3.Length;
			}
		}
		return text;
	}

	public void InitHashDictionary()
	{
		dictionary.Clear();
		idsToMetaInfo.Clear();
		for (int i = 0; i < txtIds.Count; i++)
		{
			dictionary.Add(txtIds[i], txts[i]);
			idsToMetaInfo.Add(txtIds[i], nestedLocalesMetaInfos[i]);
		}
		replacementIdMetaInfo.Clear();
		foreach (ReplacementKeysMetadata replacementKeysMetaInfo in replacementKeysMetaInfoList)
		{
			replacementIdMetaInfo.Add(replacementKeysMetaInfo.id, replacementKeysMetaInfo);
		}
	}

	public static string GetCurrentLocaleCode()
	{
		bool flag = false;
		if (currentLang == null)
		{
			flag = true;
			currentLang = new LL();
		}
		string text = currentLang.GetLocaleCode().ToLower();
		if (flag)
		{
			currentLang = null;
		}
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
		if (!languages.ContainsKey(text))
		{
			Debug.LogWarning("Language '" + text + "' not found. Loading EN...");
			text = "en";
		}
		return text;
	}

	protected virtual Dictionary<string, LanguageInfo> GetAvailableLanguages()
	{
		return new Dictionary<string, LanguageInfo>
		{
			{
				"en",
				new LanguageInfo("en", "en-US", "English")
			},
			{
				"de",
				new LanguageInfo("de", "de-DE", "Deutsch")
			},
			{
				"fr",
				new LanguageInfo("fr", "fr-FR", "Français")
			},
			{
				"pt-br",
				new LanguageInfo("pt-br", "pt-BR", "Português do Brasil")
			},
			{
				"es",
				new LanguageInfo("es", "es-ES", "Español")
			},
			{
				"ru",
				new LanguageInfo("ru", "ru-RU", "Русский")
			},
			{
				"it",
				new LanguageInfo("it", "it-IT", "Italiano")
			},
			{
				"pl",
				new LanguageInfo("pl", "pl-PL", "Polski")
			},
			{
				"ja",
				new LanguageInfo("ja", "ja-JP", "Japanese")
			},
			{
				"zh_cn",
				new LanguageInfo("zh_cn", "zh-CN", "Chinese")
			},
			{
				"ko",
				new LanguageInfo("ko", "ko-KR", "Korean")
			}
		};
	}

	protected virtual string GetLocaleCode()
	{
		switch (Application.systemLanguage)
		{
		case SystemLanguage.Belarusian:
		case SystemLanguage.Russian:
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
		case SystemLanguage.ChineseTraditional:
			return "zh_cht";
		case SystemLanguage.Korean:
			return "ko";
		case SystemLanguage.Polish:
			return "pl";
		case SystemLanguage.Italian:
			return "it";
		case SystemLanguage.Ukrainian:
			return "uk-ua";
		default:
			return "en";
		}
	}

	public void AddLangString(string id, ref string txt, bool isEllipsisFriendly = false)
	{
		txt = txt.Replace("(*", "<sprite name=\"").Replace("*)", "\">");
		txt = txt.Replace('“', '"');
		txt = txt.Replace('„', '"');
		txt = txt.Replace('・', '⊙');
		if (isEllipsisFriendly)
		{
			txt = txt.Replace("...", "…");
		}
		NestedLocalesMetaInfo nestedLocalesMetaInfo = GetNestedLocalesMetaInfo(ref txt);
		ReplacementKeysMetadata replacementKeysMetadata = FormReplacementMetadataFor(id, ref txt);
		int num = txtIds.IndexOf(id);
		if (num == -1)
		{
			txtIds.Add(id);
			txts.Add(txt);
			nestedLocalesMetaInfos.Add(nestedLocalesMetaInfo);
			if (replacementKeysMetadata != null)
			{
				replacementKeysMetaInfoList.Add(replacementKeysMetadata);
			}
		}
		else
		{
			txtIds[num] = id;
			txts[num] = txt;
			nestedLocalesMetaInfos[num] = nestedLocalesMetaInfo;
			if (replacementKeysMetadata != null)
			{
				replacementKeysMetaInfoList[num] = replacementKeysMetadata;
			}
		}
	}

	public static LanguageInfo GetAvailableLanguageInfoByIndex(int index)
	{
		index = Mathf.Clamp(index, 0, AvailableLanguages.Count - 1);
		return AvailableLanguages.ElementAt(index).Value;
	}

	public static LanguageInfo GetLanguageInfoById(string languageID)
	{
		AvailableLanguages.TryGetValue(languageID, out var value);
		return value;
	}

	public static int GetIndexByLanguage(string languageCode)
	{
		int num = 0;
		foreach (KeyValuePair<string, LanguageInfo> availableLanguage in AvailableLanguages)
		{
			if (availableLanguage.Value.id == languageCode)
			{
				return num;
			}
			num++;
		}
		return num;
	}

	public static string[] GetLanguagesRange()
	{
		return GetLanguagesRange(languages);
	}

	public static string[] GetAvailableLanguagesRange()
	{
		return GetLanguagesRange(AvailableLanguages);
	}

	public static string[] GetAvailableLanguageNamesRange()
	{
		string[] array = new string[AvailableLanguages.Count];
		int num = 0;
		foreach (KeyValuePair<string, LanguageInfo> availableLanguage in AvailableLanguages)
		{
			array[num] = availableLanguage.Value.name;
			num++;
		}
		return array;
	}

	private static string[] GetLanguagesRange(Dictionary<string, LanguageInfo> languageCollection)
	{
		string[] array = new string[languageCollection.Count];
		int num = 0;
		foreach (KeyValuePair<string, LanguageInfo> item in languageCollection)
		{
			array[num] = item.Value.id;
			num++;
		}
		return array;
	}

	public static string L(string lngId)
	{
		if (lngId == null)
		{
			Debug.LogError("lng_id is null");
			return "";
		}
		if (currentLang != null)
		{
			int num = currentLang.aliases1.IndexOf(lngId);
			if (num != -1)
			{
				return L(currentLang.aliases2[num]);
			}
		}
		string text;
		if (currentLang == null || !currentLang.dictionary.ContainsKey(lngId))
		{
			text = lngId;
		}
		else
		{
			text = currentLang.dictionary[lngId];
			NestedLocalesMetaInfo nestedLocalesMetaInfo = currentLang.idsToMetaInfo[lngId];
			if (nestedLocalesMetaInfo.hasMetaInfo)
			{
				int num2 = 0;
				for (int i = 0; i < nestedLocalesMetaInfo.idsToInsert.Count; i++)
				{
					string text2 = L(nestedLocalesMetaInfo.localeIDsToInsert[i]);
					text = text.Insert(nestedLocalesMetaInfo.idsToInsert[i] + num2, text2);
					num2 += text2.Length;
				}
			}
			if (currentLang.replacementIdMetaInfo.ContainsKey(lngId))
			{
				ReplacementKeysMetadata replacementKeysMetadata = currentLang.replacementIdMetaInfo[lngId];
				int num3 = 0;
				for (int j = 0; j < replacementKeysMetadata.keys.Count; j++)
				{
					string text3 = replacementRuleSet.Replace(replacementKeysMetadata.keys[j]);
					text = text.Insert(replacementKeysMetadata.keysIndexes[j] + num3, text3);
					num3 += text3.Length;
				}
			}
		}
		text = text.Replace("&#xA;", "\n");
		text = text.Replace("’", "'");
		text = text.Replace('”', '"');
		text = text.Replace('“', '"');
		return text.Replace("\\u00A0", "\u00a0");
	}

	public static bool HasL(string lngId)
	{
		if (lngId == null)
		{
			return false;
		}
		if (currentLang != null && currentLang.aliases1.IndexOf(lngId) != -1)
		{
			return true;
		}
		if (currentLang == null || !currentLang.dictionary.ContainsKey(lngId))
		{
			return false;
		}
		return true;
	}

	public static string ResolveAlias(string lngId)
	{
		if (string.IsNullOrEmpty(lngId) || currentLang == null)
		{
			return lngId;
		}
		string text = lngId;
		for (int i = 0; i < 16; i++)
		{
			int num = currentLang.aliases1.IndexOf(text);
			if (num < 0)
			{
				return text;
			}
			string text2 = currentLang.aliases2[num];
			if (string.IsNullOrEmpty(text2))
			{
				return text;
			}
			text = text2;
		}
		return text;
	}

	public static string L(string id, string value)
	{
		return L(id).Replace("%1", value);
	}

	public static string L(string id, string value1, string value2)
	{
		return L(id).Replace("%1", value1).Replace("%2", value2);
	}

	public static string L(string id, string value1, string value2, string value3)
	{
		return L(id).Replace("%1", value1).Replace("%2", value2).Replace("%3", value3);
	}

	public static string L(string id, int value)
	{
		return L(id, value.ToString());
	}

	public static string L(string id, float value)
	{
		return L(id, value.ToString());
	}

	public static string L(string id, int value1, int value2)
	{
		return L(id, value1.ToString(), value2.ToString());
	}

	public static string L(string id, string value1, int value2)
	{
		return L(id, value1.ToString(), value2.ToString());
	}

	public static string L(string id, int value1, string value2)
	{
		return L(id, value1.ToString(), value2.ToString());
	}

	public static string L(string id, float value1, float value2)
	{
		return L(id, value1.ToString(), value2.ToString());
	}

	public static string L(string id, string value1, float value2)
	{
		return L(id, value1.ToString(), value2.ToString());
	}

	public static string L(string id, float value1, string value2)
	{
		return L(id, value1.ToString(), value2.ToString());
	}

	public static string L(string id, string value1, int value2, int value3)
	{
		return L(id, value1.ToString(), value2.ToString(), value3.ToString());
	}

	public static bool IsEastern()
	{
		if (currentLang == null)
		{
			return false;
		}
		return easternLangIds.Contains(currentLang.id);
	}

	public static bool IsEllipsisFriendly(string langId)
	{
		return ellipsisFriendlyLangIds.Contains(langId);
	}

	public static string GetLocaleNameByCode(string localeCode)
	{
		foreach (KeyValuePair<string, LanguageInfo> availableLanguage in AvailableLanguages)
		{
			if (availableLanguage.Value.id == localeCode)
			{
				return availableLanguage.Value.name;
			}
		}
		return "?";
	}

	private NestedLocalesMetaInfo GetNestedLocalesMetaInfo(ref string s)
	{
		NestedLocalesMetaInfo nestedLocalesMetaInfo = new NestedLocalesMetaInfo();
		MatchCollection matchCollection = new Regex("\\#\\(.*?\\)").Matches(s);
		int num = 0;
		foreach (Match item in matchCollection)
		{
			s = s.Replace(item.Value, "");
			string localeIDToInsert = item.Value.Substring(2, item.Value.Length - 3);
			nestedLocalesMetaInfo.AddEntry(item.Index - num, localeIDToInsert);
			num += item.Value.Length;
		}
		return nestedLocalesMetaInfo;
	}

	private ReplacementKeysMetadata FormReplacementMetadataFor(string id, ref string inputStr)
	{
		ReplacementKeysMetadata replacementKeysMetadata = new ReplacementKeysMetadata();
		MatchCollection matchCollection = new Regex("\\@\\(.*?\\)").Matches(inputStr);
		if (matchCollection.Count == 0)
		{
			return null;
		}
		replacementKeysMetadata.id = id;
		int num = 0;
		foreach (Match item2 in matchCollection)
		{
			inputStr = inputStr.Replace(item2.Value, "");
			string item = item2.Value.Substring(2, item2.Value.Length - 3);
			replacementKeysMetadata.keysIndexes.Add(item2.Index - num);
			replacementKeysMetadata.keys.Add(item);
			num += item2.Value.Length;
		}
		return replacementKeysMetadata;
	}

	public static void AddAliases(List<string> alias1, List<string> alias2)
	{
		foreach (KeyValuePair<string, LanguageInfo> language in languages)
		{
			LL lL = Resources.Load<LL>("Locales/lng_" + language.Value.id);
			if (lL == null)
			{
				continue;
			}
			for (int i = 0; i < alias1.Count; i++)
			{
				if (!lL.aliases1.Contains(alias1[i]))
				{
					lL.aliases1.Add(alias1[i]);
					lL.aliases2.Add(alias2[i]);
				}
			}
		}
	}

	public string ToJSON()
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < txtIds.Count; i++)
		{
			jsonData.data.Add(new JsonEntry
			{
				key = txtIds[i],
				value = txts[i]
			});
		}
		return JsonUtility.ToJson(jsonData, prettyPrint: true);
	}
}
