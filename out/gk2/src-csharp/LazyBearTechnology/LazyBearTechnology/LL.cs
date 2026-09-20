using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LazyBearTechnology;

public class LL : LLBase
{
	public enum LocModificator
	{
		None,
		VoiceOverMuted
	}

	private const string LATIN_AMERICA_CODE = "es-419";

	private static LL englishLang;

	private static readonly Regex nobrTagRegex = new Regex("</?nobr>", RegexOptions.IgnoreCase);

	private static readonly Regex spaceTagRegex = new Regex("<space=\\d+>", RegexOptions.IgnoreCase);

	private static readonly Regex whitespaceRegex = new Regex("\\s+");

	public static bool IsCurrentLangAsian
	{
		get
		{
			if (!(LLBase.CurrentLang == "zh_cn") && !(LLBase.CurrentLang == "ja") && !(LLBase.CurrentLang == "ko"))
			{
				return LLBase.CurrentLang == "zh_cht";
			}
			return true;
		}
	}

	protected override Dictionary<string, LanguageInfo> GetAvailableLanguages()
	{
		Dictionary<string, LanguageInfo> result = new Dictionary<string, LanguageInfo>
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
				"pl",
				new LanguageInfo("pl", "pl-PL", "Polski")
			},
			{
				"ja",
				new LanguageInfo("ja", "ja-JP", "Japanese")
			},
			{
				"zh_cn",
				new LanguageInfo("zh_cn", "zh-CN", "Chinese (Simplified)")
			},
			{
				"ko",
				new LanguageInfo("ko", "ko-KR", "Korean")
			},
			{
				"tr",
				new LanguageInfo("tr", "tr-TR", "Türkçe")
			}
		};
		LanguageModHooks.AppendLanguages?.Invoke(result);
		return result;
	}

	public static string GetLocaleCodeId()
	{
		Debug.Log(string.Format("[{0}]: Detected System Language = {1}", "LL", Application.systemLanguage));
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
		case SystemLanguage.ChineseSimplified:
			return "zh_cn";
		case SystemLanguage.ChineseTraditional:
			return "zh_cht";
		case SystemLanguage.Korean:
			return "ko";
		case SystemLanguage.Polish:
			return "pl";
		case SystemLanguage.Italian:
			return "it";
		case SystemLanguage.Turkish:
			return "tr";
		default:
			return "en";
		}
	}

	public new static string GetCurrentLocaleCode()
	{
		bool flag = false;
		if (LLBase.currentLang == null)
		{
			flag = true;
			LLBase.currentLang = new LL();
		}
		string text = LLBase.currentLang.GetLocaleCode().ToLower();
		Dictionary<string, LanguageInfo> availableLanguages = LLBase.currentLang.GetAvailableLanguages();
		if (flag)
		{
			LLBase.currentLang = null;
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
		if (!availableLanguages.ContainsKey(text))
		{
			Debug.LogWarning("Language '" + text + "' not found. Loading EN...");
			text = "en";
		}
		return text;
	}

	protected override string GetLocaleCode()
	{
		return GetLocaleCodeId();
	}

	public static string GetSpace()
	{
		if (LLBase.currentLang.id == "zh_cht")
		{
			return "<space=6>";
		}
		return " ";
	}

	public static string GetHintsSeparator()
	{
		return GetSpace() + GetSpace() + GetSpace() + GetSpace();
	}

	public static bool HasLocalizedValueForCurrentLang(string lngId)
	{
		if (string.IsNullOrEmpty(lngId) || LLBase.currentLang == null)
		{
			return false;
		}
		string key = LLBase.ResolveAlias(lngId);
		if (!LLBase.currentLang.dictionary.TryGetValue(key, out var value))
		{
			return false;
		}
		if (LLBase.currentLang.id == "en")
		{
			return true;
		}
		string text = NormalizeForTranslationCompare(value);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		LL lL = GetEnglishLang();
		if (lL != null && lL.dictionary.TryGetValue(key, out var value2) && text == NormalizeForTranslationCompare(value2))
		{
			return false;
		}
		return true;
	}

	private static string NormalizeForTranslationCompare(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		s = nobrTagRegex.Replace(s, string.Empty);
		s = spaceTagRegex.Replace(s, string.Empty);
		s = s.Replace("\u200b", string.Empty);
		s = whitespaceRegex.Replace(s, " ").Trim();
		return s;
	}

	private static LL GetEnglishLang()
	{
		if (englishLang == null)
		{
			englishLang = Resources.Load<LL>("Locales/lng_en");
			if (englishLang != null)
			{
				englishLang.InitHashDictionary();
			}
		}
		return englishLang;
	}
}
