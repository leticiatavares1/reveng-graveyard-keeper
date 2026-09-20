using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace LazyBearTechnology;

public static class LanguageModLoader
{
	private class PackedLanguage
	{
		public string id;

		public string displayName;

		public string font;

		public string fontDirectory;

		public LanguageModFontSettings fontSettings;

		public bool useOwnMaterial;

		public bool rtl;

		public bool requireResize;

		public Type[] preprocessorTypes;

		public LL language;

		public TMP_FontAsset customFontAsset;
	}

	private class PackSource
	{
		public string id;

		public string directory;

		public LanguageModManifest manifest;

		public Dictionary<string, string> overlay;
	}

	private const string FontDefault = "default";

	private const string FontJapanese = "japanese";

	private const string FontChinese = "chinese";

	private const string FontKorean = "korean";

	private const string FontHandjet = "handjet";

	private const string TtfExtension = ".ttf";

	private const string UiLangNameLocKey = "ui_lang_name_loc";

	private const int DefaultSamplingPointSize = 16;

	private const int DefaultAtlasPadding = 3;

	private const int DefaultAtlasSize = 512;

	private const float DefaultFaceScale = 1f;

	private const GlyphRenderMode DefaultRenderMode = GlyphRenderMode.RASTER_HINTED;

	private static readonly Dictionary<string, string> FontResourcePaths;

	private static readonly HashSet<string> FontsUsingOwnMaterial;

	private static readonly Type[] NoPreprocessorTypes;

	private static readonly Dictionary<string, PackedLanguage> packs;

	private static readonly Dictionary<string, TMP_FontAsset> fontCache;

	private static readonly Dictionary<string, Dictionary<string, string>> shippedMarkupCache;

	private static readonly Dictionary<string, Dictionary<string, string>> resolvedMarkupCache;

	private static readonly Dictionary<string, Type> preprocessorTypeCache;

	static LanguageModLoader()
	{
		FontResourcePaths = new Dictionary<string, string>
		{
			{ "japanese", "Fonts & Materials/japanese" },
			{ "chinese", "Fonts & Materials/chinese" },
			{ "korean", "Fonts & Materials/korean" },
			{ "handjet", "Fonts & Materials/handjet" }
		};
		FontsUsingOwnMaterial = new HashSet<string>();
		NoPreprocessorTypes = Array.Empty<Type>();
		packs = new Dictionary<string, PackedLanguage>();
		fontCache = new Dictionary<string, TMP_FontAsset>();
		shippedMarkupCache = new Dictionary<string, Dictionary<string, string>>();
		resolvedMarkupCache = new Dictionary<string, Dictionary<string, string>>();
		preprocessorTypeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		BindHooks();
	}

	private static void BindHooks()
	{
		LanguageModHooks.TryGetLanguage = TryGetLanguage;
		LanguageModHooks.TryGetFontAsset = TryGetFontAsset;
		LanguageModHooks.UsesOwnMaterial = UsesOwnMaterial;
		LanguageModHooks.RequiresResize = RequiresResize;
		LanguageModHooks.ApplyDirection = ApplyDirection;
		LanguageModHooks.AppendLanguages = AppendLanguages;
		VoiceOverModLoader.SearchRootsProvider = ModsPaths.GetModSearchRoots;
	}

	public static void Scan()
	{
		BindHooks();
		foreach (PackedLanguage value in packs.Values)
		{
			if (value.language != null)
			{
				UnityEngine.Object.Destroy(value.language);
			}
			DestroyRuntimeFontAsset(value.customFontAsset);
		}
		UnregisterDynamicStyleMaterials();
		TextStyle.ClearDynamicMaterialCaches();
		PurgeDestroyedTmpLookups();
		packs.Clear();
		shippedMarkupCache.Clear();
		resolvedMarkupCache.Clear();
		LL lL = LoadEnglish();
		if (lL == null)
		{
			Debug.LogWarning("[LanguageMod] English locale asset is missing. Cannot load language packs.");
			return;
		}
		Dictionary<string, string> dictionary = BuildMarkupMap(lL);
		shippedMarkupCache["en"] = dictionary;
		Dictionary<string, PackSource> dictionary2 = new Dictionary<string, PackSource>();
		CollectSources(dictionary2);
		foreach (PackSource value2 in dictionary2.Values)
		{
			BuildPack(value2, dictionary2, lL, dictionary);
		}
		Debug.Log(string.Format("[LanguageMod] Loaded {0} language pack(s). Search roots: {1}", packs.Count, string.Join(" | ", ModsPaths.GetModSearchRoots())));
		foreach (PackSource value3 in dictionary2.Values)
		{
			Debug.Log("[LanguageMod] Pack id='" + value3.id + "' dir='" + value3.directory + "'");
		}
	}

	private static void CollectSources(Dictionary<string, PackSource> sources)
	{
		IReadOnlyList<string> modSearchRoots = ModsPaths.GetModSearchRoots();
		for (int i = 0; i < modSearchRoots.Count; i++)
		{
			string text = modSearchRoots[i];
			TryAddPackDirectory(sources, text, warnIfMissing: false);
			string path = Path.Combine(text, "Languages");
			if (Directory.Exists(path))
			{
				string[] directories = Directory.GetDirectories(path);
				Array.Sort(directories, StringComparer.OrdinalIgnoreCase);
				for (int j = 0; j < directories.Length; j++)
				{
					TryAddPackDirectory(sources, directories[j], warnIfMissing: true);
				}
			}
		}
	}

	private static void TryAddPackDirectory(Dictionary<string, PackSource> sources, string directory, bool warnIfMissing)
	{
		string fileName = Path.GetFileName(directory);
		if (!ModsPaths.IsDisabledFolderName(fileName) && TryReadSource(directory, fileName, warnIfMissing, out var source) && !string.IsNullOrEmpty(source.id))
		{
			if (LLBase.languages.ContainsKey(source.id))
			{
				Debug.LogWarning("[LanguageMod] Skipping '" + source.id + "': id collides with a shipped language.");
			}
			else if (sources.ContainsKey(source.id))
			{
				Debug.LogWarning("[LanguageMod] Skipping '" + directory + "': id '" + source.id + "' already loaded from another folder.");
			}
			else
			{
				sources[source.id] = source;
			}
		}
	}

	public static void AppendLanguages(Dictionary<string, LLBase.LanguageInfo> languages)
	{
		foreach (KeyValuePair<string, PackedLanguage> pack in packs)
		{
			if (!languages.ContainsKey(pack.Key))
			{
				languages.Add(pack.Key, new LLBase.LanguageInfo(pack.Value.id, pack.Value.id, pack.Value.displayName));
			}
		}
	}

	public static bool TryGetLanguage(string langId, out LL language)
	{
		language = null;
		if (string.IsNullOrEmpty(langId))
		{
			return false;
		}
		if (!packs.TryGetValue(langId, out var value))
		{
			return false;
		}
		language = value.language;
		return language != null;
	}

	public static bool TryGetFontAsset(string lang, bool staticFont, out TMP_FontAsset fontAsset)
	{
		fontAsset = null;
		if (staticFont || string.IsNullOrEmpty(lang))
		{
			return false;
		}
		if (!packs.TryGetValue(lang, out var value))
		{
			return false;
		}
		if (IsExternalFontFile(value.font))
		{
			return TryGetOrCreateExternalFont(value, out fontAsset);
		}
		string text = NormalizeFont(value.font);
		if (text == "default")
		{
			return false;
		}
		if (!FontResourcePaths.TryGetValue(text, out var value2))
		{
			return false;
		}
		if (!fontCache.TryGetValue(value2, out fontAsset) || fontAsset == null)
		{
			fontAsset = Resources.Load<TMP_FontAsset>(value2);
			fontCache[value2] = fontAsset;
		}
		return fontAsset != null;
	}

	public static bool UsesOwnMaterial(string lang)
	{
		if (string.IsNullOrEmpty(lang) || !packs.TryGetValue(lang, out var value))
		{
			return false;
		}
		if (IsExternalFontFile(value.font))
		{
			return value.useOwnMaterial;
		}
		return FontsUsingOwnMaterial.Contains(NormalizeFont(value.font));
	}

	public static bool IsRightToLeft(string lang)
	{
		if (string.IsNullOrEmpty(lang) || !packs.TryGetValue(lang, out var value))
		{
			return false;
		}
		return value.rtl;
	}

	public static bool RequiresResize(string lang)
	{
		if (string.IsNullOrEmpty(lang) || !packs.TryGetValue(lang, out var value))
		{
			return false;
		}
		return value.requireResize;
	}

	public static void ApplyDirection(TMP_Text label, string lang, bool staticFont)
	{
		if (!(label == null))
		{
			LanguageRtlLabelState languageRtlLabelState = label.GetComponent<LanguageRtlLabelState>();
			if (languageRtlLabelState == null)
			{
				languageRtlLabelState = label.gameObject.AddComponent<LanguageRtlLabelState>();
				languageRtlLabelState.hideFlags = HideFlags.HideAndDontSave;
			}
			if (!languageRtlLabelState.captured)
			{
				DestroyRuntimePreprocessors(label, NoPreprocessorTypes);
				languageRtlLabelState.originalRtl = label.isRightToLeftText;
				languageRtlLabelState.originalAlignment = label.alignment;
				languageRtlLabelState.originalPreprocessor = label.textPreprocessor;
				languageRtlLabelState.captured = true;
			}
			bool flag = !staticFont && IsRightToLeft(lang);
			label.isRightToLeftText = flag || languageRtlLabelState.originalRtl;
			bool flag2 = false;
			LocalizedRtlAlign component = label.GetComponent<LocalizedRtlAlign>();
			if (component != null && component.DontChangeAlignOnRtl)
			{
				flag2 = true;
			}
			label.alignment = ((flag && !flag2) ? FlipHorizontalAlignment(languageRtlLabelState.originalAlignment) : languageRtlLabelState.originalAlignment);
			Type[] desired = (staticFont ? NoPreprocessorTypes : GetPreprocessorTypes(lang));
			SyncPreprocessors(label, desired, languageRtlLabelState);
		}
	}

	public static void EnsureExamplePack()
	{
		string exampleLanguageFolder = ModsPaths.ExampleLanguageFolder;
		if (Directory.Exists(exampleLanguageFolder))
		{
			return;
		}
		Directory.CreateDirectory(exampleLanguageFolder);
		File.WriteAllText(Path.Combine(exampleLanguageFolder, "language.json"), ModsDocumentation.Load("Mods/Languages/language"));
		LL lL = LoadEnglish();
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		if (lL != null)
		{
			int textEntryCount = lL.TextEntryCount;
			for (int i = 0; i < textEntryCount; i++)
			{
				list.Add(new KeyValuePair<string, string>(lL.GetTextIdAt(i), lL.GetTextWithMarkup(i)));
			}
		}
		ModsCsv.WriteKeyValueFile(Path.Combine(exampleLanguageFolder, "strings.csv"), list);
		Debug.Log("[LanguageMod] Created example pack at '" + exampleLanguageFolder + "'.");
	}

	private static bool TryReadSource(string directory, string folderName, bool warnIfMissing, out PackSource source)
	{
		source = null;
		string text = Path.Combine(directory, "language.json");
		if (!File.Exists(text))
		{
			if (warnIfMissing)
			{
				Debug.LogWarning("[LanguageMod] '" + folderName + "' has no language.json. Pack ignored.");
			}
			return false;
		}
		LanguageModManifest languageModManifest;
		try
		{
			languageModManifest = JsonUtility.FromJson<LanguageModManifest>(ModsJsonComments.Strip(File.ReadAllText(text)));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LanguageMod] Failed to parse '" + text + "': " + ex.Message);
			return false;
		}
		if (languageModManifest == null)
		{
			Debug.LogWarning("[LanguageMod] Failed to parse '" + text + "'.");
			return false;
		}
		if (string.IsNullOrEmpty(folderName))
		{
			return false;
		}
		source = new PackSource
		{
			id = folderName,
			directory = directory,
			manifest = languageModManifest,
			overlay = ModsCsv.ReadKeyValueFile(Path.Combine(directory, "strings.csv"))
		};
		return true;
	}

	private static void BuildPack(PackSource source, Dictionary<string, PackSource> sources, LL english, Dictionary<string, string> englishMarkup)
	{
		string id = source.id;
		Dictionary<string, string> overlay = source.overlay;
		HashSet<string> visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { id };
		Dictionary<string, string> dictionary = ResolveFallbackMarkup(source.manifest.fallback, id, sources, visiting, englishMarkup);
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		LL lL = ScriptableObject.CreateInstance<LL>();
		lL.hideFlags = HideFlags.HideAndDontSave;
		lL.id = id;
		lL.aliases1.AddRange(english.aliases1);
		lL.aliases2.AddRange(english.aliases2);
		bool rtl = source.manifest.rtl;
		HashSet<string> hashSet = new HashSet<string>();
		int textEntryCount = english.TextEntryCount;
		for (int i = 0; i < textEntryCount; i++)
		{
			string textIdAt = english.GetTextIdAt(i);
			string value;
			string text = (englishMarkup.TryGetValue(textIdAt, out value) ? value : english.GetTextWithMarkup(i));
			string text2 = text;
			if (overlay.TryGetValue(textIdAt, out var value2) && !string.IsNullOrWhiteSpace(value2))
			{
				text2 = value2;
			}
			else if (textIdAt == "ui_lang_name_loc")
			{
				text2 = (string.IsNullOrWhiteSpace(source.manifest.name) ? id : source.manifest.name.Trim());
			}
			else
			{
				if (dictionary.TryGetValue(textIdAt, out var value3) && !string.IsNullOrWhiteSpace(value3))
				{
					text2 = value3;
				}
				list.Add(new KeyValuePair<string, string>(textIdAt, text));
			}
			string txt = (rtl ? ArabicShaper.Shape(text2) : text2);
			lL.AddLangString(textIdAt, ref txt);
			hashSet.Add(textIdAt);
		}
		foreach (KeyValuePair<string, string> item in overlay)
		{
			if (!hashSet.Contains(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
			{
				string txt2 = (rtl ? ArabicShaper.Shape(item.Value) : item.Value);
				lL.AddLangString(item.Key, ref txt2);
			}
		}
		lL.InitHashDictionary();
		if (ModsPaths.IsEditableModPath(source.directory))
		{
			ModsCsv.WriteKeyValueFile(Path.Combine(source.directory, "_missing_lines.csv"), list);
		}
		string text3 = (string.IsNullOrWhiteSpace(source.manifest.name) ? id : source.manifest.name.Trim());
		if (rtl)
		{
			text3 = ArabicShaper.Shape(text3);
		}
		string font = NormalizeFont(source.manifest.font);
		packs[id] = new PackedLanguage
		{
			id = id,
			displayName = text3,
			font = font,
			fontDirectory = source.directory,
			fontSettings = (IsExternalFontFile(font) ? source.manifest.fontSettings : null),
			useOwnMaterial = false,
			rtl = source.manifest.rtl,
			requireResize = source.manifest.requireResize,
			preprocessorTypes = ResolvePreprocessorTypes(source.manifest.preprocessors, id),
			language = lL
		};
	}

	private static Dictionary<string, string> ResolveFallbackMarkup(string fallback, string packId, Dictionary<string, PackSource> sources, HashSet<string> visiting, Dictionary<string, string> englishMarkup)
	{
		string text = NormalizeFallbackId(fallback, packId);
		if (string.Equals(text, "en", StringComparison.OrdinalIgnoreCase))
		{
			return englishMarkup;
		}
		if (TryFindSource(sources, text, out var source))
		{
			if (!visiting.Add(source.id))
			{
				Debug.LogWarning("[LanguageMod] Fallback cycle involving '" + packId + "' / '" + source.id + "'. Using English.");
				return englishMarkup;
			}
			if (resolvedMarkupCache.TryGetValue(source.id, out var value))
			{
				visiting.Remove(source.id);
				return value;
			}
			Dictionary<string, string> dictionary = OverlayMarkup(ResolveFallbackMarkup(source.manifest.fallback, source.id, sources, visiting, englishMarkup), source.overlay);
			resolvedMarkupCache[source.id] = dictionary;
			visiting.Remove(source.id);
			return dictionary;
		}
		if (TryGetShippedMarkup(text, out var markup))
		{
			return markup;
		}
		Debug.LogWarning("[LanguageMod] '" + packId + "' fallback '" + text + "' was not found. Using English.");
		return englishMarkup;
	}

	private static Dictionary<string, string> OverlayMarkup(Dictionary<string, string> baseline, Dictionary<string, string> overlay)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(baseline);
		foreach (KeyValuePair<string, string> item in overlay)
		{
			if (!string.IsNullOrWhiteSpace(item.Value))
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	private static bool TryFindSource(Dictionary<string, PackSource> sources, string id, out PackSource source)
	{
		if (sources.TryGetValue(id, out source))
		{
			return true;
		}
		foreach (KeyValuePair<string, PackSource> source2 in sources)
		{
			if (string.Equals(source2.Key, id, StringComparison.OrdinalIgnoreCase))
			{
				source = source2.Value;
				return true;
			}
		}
		source = null;
		return false;
	}

	private static bool TryGetShippedMarkup(string langId, out Dictionary<string, string> markup)
	{
		if (shippedMarkupCache.TryGetValue(langId, out markup))
		{
			return true;
		}
		foreach (KeyValuePair<string, Dictionary<string, string>> item in shippedMarkupCache)
		{
			if (string.Equals(item.Key, langId, StringComparison.OrdinalIgnoreCase))
			{
				markup = item.Value;
				return true;
			}
		}
		LL lL = Resources.Load<LL>("Locales/lng_" + langId);
		if (lL == null && !string.Equals(langId, langId.ToLowerInvariant(), StringComparison.Ordinal))
		{
			lL = Resources.Load<LL>("Locales/lng_" + langId.ToLowerInvariant());
		}
		if (lL == null)
		{
			markup = null;
			return false;
		}
		if (lL.dictionary == null || lL.dictionary.Count == 0)
		{
			lL.InitHashDictionary();
		}
		markup = BuildMarkupMap(lL);
		shippedMarkupCache[lL.id ?? langId] = markup;
		return true;
	}

	private static Dictionary<string, string> BuildMarkupMap(LL language)
	{
		int textEntryCount = language.TextEntryCount;
		Dictionary<string, string> dictionary = new Dictionary<string, string>(textEntryCount);
		for (int i = 0; i < textEntryCount; i++)
		{
			string textIdAt = language.GetTextIdAt(i);
			if (!string.IsNullOrEmpty(textIdAt) && !dictionary.ContainsKey(textIdAt))
			{
				dictionary.Add(textIdAt, language.GetTextWithMarkup(i));
			}
		}
		return dictionary;
	}

	private static string NormalizeFallbackId(string fallback, string packId)
	{
		if (string.IsNullOrWhiteSpace(fallback))
		{
			return "en";
		}
		string text = fallback.Trim();
		if (string.Equals(text, packId, StringComparison.OrdinalIgnoreCase))
		{
			Debug.LogWarning("[LanguageMod] '" + packId + "' fallback points at itself. Using English.");
			return "en";
		}
		return text;
	}

	private static Type[] GetPreprocessorTypes(string lang)
	{
		if (string.IsNullOrEmpty(lang) || !packs.TryGetValue(lang, out var value) || value.preprocessorTypes == null)
		{
			return NoPreprocessorTypes;
		}
		return value.preprocessorTypes;
	}

	private static void SyncPreprocessors(TMP_Text label, Type[] desired, LanguageRtlLabelState state)
	{
		DestroyRuntimePreprocessors(label, desired);
		foreach (Type type in desired)
		{
			if (type == null)
			{
				continue;
			}
			Component component = label.GetComponent(type);
			if (component != null)
			{
				if (component is Behaviour behaviour)
				{
					behaviour.enabled = true;
				}
			}
			else
			{
				label.gameObject.AddComponent(type).hideFlags = HideFlags.HideAndDontSave;
			}
		}
		if (desired.Length == 0)
		{
			label.textPreprocessor = state.originalPreprocessor;
		}
	}

	private static void DestroyRuntimePreprocessors(TMP_Text label, Type[] keep)
	{
		MonoBehaviour[] components = label.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			if (!(monoBehaviour == null) && monoBehaviour is ITextPreprocessor && (monoBehaviour.hideFlags & HideFlags.HideAndDontSave) != 0 && !ContainsType(keep, monoBehaviour.GetType()))
			{
				UnityEngine.Object.Destroy(monoBehaviour);
			}
		}
	}

	private static bool ContainsType(Type[] types, Type type)
	{
		if (types == null)
		{
			return false;
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] == type)
			{
				return true;
			}
		}
		return false;
	}

	private static bool ListContainsType(List<Type> types, Type type)
	{
		for (int i = 0; i < types.Count; i++)
		{
			if (types[i] == type)
			{
				return true;
			}
		}
		return false;
	}

	private static Type[] ResolvePreprocessorTypes(string[] names, string packId)
	{
		if (names == null || names.Length == 0)
		{
			return NoPreprocessorTypes;
		}
		List<Type> list = new List<Type>(names.Length);
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			if (!string.IsNullOrWhiteSpace(text))
			{
				text = text.Trim();
				Type type = ResolvePreprocessorType(text);
				if (type == null)
				{
					Debug.LogWarning("[LanguageMod] '" + packId + "' preprocessor '" + text + "' was not found.");
				}
				else if (!typeof(MonoBehaviour).IsAssignableFrom(type) || type.IsAbstract)
				{
					Debug.LogWarning("[LanguageMod] '" + packId + "' preprocessor '" + text + "' must be a MonoBehaviour.");
				}
				else if (!typeof(ITextPreprocessor).IsAssignableFrom(type))
				{
					Debug.LogWarning("[LanguageMod] '" + packId + "' preprocessor '" + text + "' must implement ITextPreprocessor.");
				}
				else if (!ListContainsType(list, type))
				{
					list.Add(type);
				}
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return NoPreprocessorTypes;
	}

	private static Type ResolvePreprocessorType(string name)
	{
		if (preprocessorTypeCache.TryGetValue(name, out var value))
		{
			return value;
		}
		Type type = FindPreprocessorType(name);
		preprocessorTypeCache[name] = type;
		return type;
	}

	private static Type FindPreprocessorType(string name)
	{
		Type type = Type.GetType(name);
		if (type != null)
		{
			return type;
		}
		Assembly assembly = typeof(LanguageModLoader).Assembly;
		type = assembly.GetType(name) ?? assembly.GetType("LazyBearTechnology." + name);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly2 in assemblies)
		{
			try
			{
				type = assembly2.GetType(name);
				if (type != null)
				{
					return type;
				}
				if (name.IndexOf('.') < 0)
				{
					type = assembly2.GetType("LazyBearTechnology." + name);
					if (type != null)
					{
						return type;
					}
				}
			}
			catch (ReflectionTypeLoadException)
			{
			}
		}
		return null;
	}

	private static LL LoadEnglish()
	{
		LL lL = Resources.Load<LL>("Locales/lng_en");
		if (lL == null)
		{
			return null;
		}
		if (lL.dictionary == null || lL.dictionary.Count == 0)
		{
			lL.InitHashDictionary();
		}
		return lL;
	}

	private static string NormalizeFont(string font)
	{
		if (string.IsNullOrWhiteSpace(font))
		{
			return "default";
		}
		string text = font.Trim();
		if (IsExternalFontFile(text))
		{
			return Path.GetFileName(text);
		}
		string text2 = text.ToLowerInvariant();
		switch (text2)
		{
		case "japanese":
		case "chinese":
		case "korean":
		case "handjet":
			return text2;
		default:
			return "default";
		}
	}

	private static bool IsExternalFontFile(string font)
	{
		if (!string.IsNullOrEmpty(font))
		{
			return font.IndexOf(".ttf", StringComparison.OrdinalIgnoreCase) >= 0;
		}
		return false;
	}

	private static bool TryGetOrCreateExternalFont(PackedLanguage pack, out TMP_FontAsset fontAsset)
	{
		fontAsset = pack.customFontAsset;
		if (fontAsset != null)
		{
			return true;
		}
		string fileName = Path.GetFileName(pack.font);
		string text = Path.Combine(pack.fontDirectory, fileName);
		if (!File.Exists(text))
		{
			Debug.LogWarning("[LanguageMod] '" + pack.id + "' font file '" + fileName + "' was not found in the pack folder.");
			return false;
		}
		LanguageModFontSettings fontSettings = pack.fontSettings;
		int samplingPointSize = ((fontSettings != null && fontSettings.pointSize > 0) ? fontSettings.pointSize : 16);
		int atlasWidth = ((fontSettings != null && fontSettings.atlasWidth > 0) ? fontSettings.atlasWidth : 512);
		int atlasHeight = ((fontSettings != null && fontSettings.atlasHeight > 0) ? fontSettings.atlasHeight : 512);
		int faceIndex = ((fontSettings != null) ? Math.Max(0, fontSettings.faceIndex) : 0);
		fontAsset = TMP_FontAsset.CreateFontAsset(text, faceIndex, samplingPointSize, 3, GlyphRenderMode.RASTER_HINTED, atlasWidth, atlasHeight);
		if (fontAsset == null)
		{
			Debug.LogWarning("[LanguageMod] '" + pack.id + "' failed to load font '" + fileName + "'.");
			return false;
		}
		fontAsset.hideFlags = HideFlags.HideAndDontSave;
		fontAsset.name = Path.GetFileNameWithoutExtension(fileName);
		if (fontAsset.material != null)
		{
			fontAsset.material.hideFlags = HideFlags.HideAndDontSave;
			fontAsset.material.name = fontAsset.name + " Material";
		}
		ApplyFaceInfoOverrides(fontAsset, fontSettings);
		fontAsset.ReadFontAssetDefinition();
		WarmupFontCharacters(fontAsset, pack);
		ApplyAtlasFilter(fontAsset);
		MaterialReferenceManager.AddFontAsset(fontAsset);
		pack.customFontAsset = fontAsset;
		return true;
	}

	private static void ApplyFaceInfoOverrides(TMP_FontAsset fontAsset, LanguageModFontSettings settings)
	{
		if (fontAsset == null)
		{
			return;
		}
		FaceInfo faceInfo = fontAsset.faceInfo;
		faceInfo.scale = 1f;
		if (settings != null && settings.overrideFaceInfo)
		{
			faceInfo.lineHeight = settings.lineHeight;
			faceInfo.ascentLine = settings.ascentLine;
			faceInfo.capLine = settings.capLine;
			faceInfo.meanLine = settings.meanLine;
			faceInfo.baseline = settings.baseline;
			faceInfo.descentLine = settings.descentLine;
			if (settings.pointSize > 0)
			{
				faceInfo.pointSize = settings.pointSize;
			}
		}
		fontAsset.faceInfo = faceInfo;
	}

	private static void ApplyAtlasFilter(TMP_FontAsset fontAsset)
	{
		Texture2D[] atlasTextures = fontAsset.atlasTextures;
		if (atlasTextures == null)
		{
			return;
		}
		for (int i = 0; i < atlasTextures.Length; i++)
		{
			if (!(atlasTextures[i] == null))
			{
				atlasTextures[i].filterMode = FilterMode.Point;
				atlasTextures[i].wrapMode = TextureWrapMode.Clamp;
				atlasTextures[i].hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}

	private static void WarmupFontCharacters(TMP_FontAsset fontAsset, PackedLanguage pack)
	{
		if (pack.language == null || pack.language.dictionary == null)
		{
			return;
		}
		HashSet<char> hashSet = new HashSet<char>();
		foreach (KeyValuePair<string, string> item in pack.language.dictionary)
		{
			string value = item.Value;
			if (!string.IsNullOrEmpty(value))
			{
				for (int i = 0; i < value.Length; i++)
				{
					hashSet.Add(value[i]);
				}
			}
		}
		for (char c = ' '; c <= '~'; c = (char)(c + 1))
		{
			hashSet.Add(c);
		}
		if (hashSet.Count != 0)
		{
			char[] array = new char[hashSet.Count];
			hashSet.CopyTo(array);
			fontAsset.TryAddCharacters(new string(array), includeFontFeatures: true);
		}
	}

	private static void DestroyRuntimeFontAsset(TMP_FontAsset fontAsset)
	{
		if (!(fontAsset == null))
		{
			UnregisterTmpFont(fontAsset);
			UnityEngine.Object.DestroyImmediate(fontAsset, allowDestroyingAssets: true);
		}
	}

	private static void UnregisterTmpFont(TMP_FontAsset fontAsset)
	{
		GetTmpFontLookup()?.Remove(fontAsset.hashCode);
		Dictionary<int, Material> tmpMaterialLookup = GetTmpMaterialLookup();
		if (tmpMaterialLookup != null)
		{
			tmpMaterialLookup.Remove(fontAsset.hashCode);
			tmpMaterialLookup.Remove(fontAsset.materialHashCode);
		}
	}

	private static void UnregisterDynamicStyleMaterials()
	{
		Dictionary<int, Material> tmpMaterialLookup = GetTmpMaterialLookup();
		if (tmpMaterialLookup == null)
		{
			return;
		}
		TextStyle[] array = Resources.FindObjectsOfTypeAll<TextStyle>();
		foreach (string key in packs.Keys)
		{
			for (int i = 0; i < array.Length; i++)
			{
				tmpMaterialLookup.Remove(HashTmpMaterialName(key + ":" + array[i].name + "_dynamic"));
			}
		}
	}

	private static void PurgeDestroyedTmpLookups()
	{
		Dictionary<int, TMP_FontAsset> tmpFontLookup = GetTmpFontLookup();
		if (tmpFontLookup != null)
		{
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, TMP_FontAsset> item in tmpFontLookup)
			{
				if (item.Value == null)
				{
					list.Add(item.Key);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				tmpFontLookup.Remove(list[i]);
			}
		}
		Dictionary<int, Material> tmpMaterialLookup = GetTmpMaterialLookup();
		if (tmpMaterialLookup == null)
		{
			return;
		}
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, Material> item2 in tmpMaterialLookup)
		{
			if (item2.Value == null)
			{
				list2.Add(item2.Key);
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			tmpMaterialLookup.Remove(list2[j]);
		}
	}

	private static Dictionary<int, TMP_FontAsset> GetTmpFontLookup()
	{
		return typeof(MaterialReferenceManager).GetField("m_FontAssetReferenceLookup", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(MaterialReferenceManager.instance) as Dictionary<int, TMP_FontAsset>;
	}

	private static Dictionary<int, Material> GetTmpMaterialLookup()
	{
		return typeof(MaterialReferenceManager).GetField("m_FontMaterialReferenceLookup", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(MaterialReferenceManager.instance) as Dictionary<int, Material>;
	}

	private static int HashTmpMaterialName(string matName)
	{
		int num = 0;
		for (int i = 0; i < matName.Length; i++)
		{
			num = ((num << 5) + num) ^ TMP_TextParsingUtilities.ToUpperASCIIFast(matName[i]);
		}
		return num;
	}

	private static TextAlignmentOptions FlipHorizontalAlignment(TextAlignmentOptions alignment)
	{
		return alignment switch
		{
			TextAlignmentOptions.TopLeft => TextAlignmentOptions.TopRight, 
			TextAlignmentOptions.TopRight => TextAlignmentOptions.TopLeft, 
			TextAlignmentOptions.Left => TextAlignmentOptions.Right, 
			TextAlignmentOptions.Right => TextAlignmentOptions.Left, 
			TextAlignmentOptions.BottomLeft => TextAlignmentOptions.BottomRight, 
			TextAlignmentOptions.BottomRight => TextAlignmentOptions.BottomLeft, 
			TextAlignmentOptions.BaselineLeft => TextAlignmentOptions.BaselineRight, 
			TextAlignmentOptions.BaselineRight => TextAlignmentOptions.BaselineLeft, 
			TextAlignmentOptions.MidlineLeft => TextAlignmentOptions.MidlineRight, 
			TextAlignmentOptions.MidlineRight => TextAlignmentOptions.MidlineLeft, 
			TextAlignmentOptions.CaplineLeft => TextAlignmentOptions.CaplineRight, 
			TextAlignmentOptions.CaplineRight => TextAlignmentOptions.CaplineLeft, 
			_ => alignment, 
		};
	}
}
