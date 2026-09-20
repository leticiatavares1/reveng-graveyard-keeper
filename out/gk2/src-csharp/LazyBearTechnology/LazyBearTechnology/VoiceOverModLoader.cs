using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LazyBearTechnology;

public static class VoiceOverModLoader
{
	[Serializable]
	private class VoiceOverModManifest
	{
		public int schemaVersion = 1;

		public string language = "";

		public string name = "";

		public bool enabled = true;
	}

	private const string ModsFolder = "Mods";

	private const string VoiceOversFolder = "VoiceOvers";

	private const string ManifestFileName = "mod.json";

	private const int SupportedSchemaVersion = 1;

	private static readonly string[] ClipExtensions = new string[3] { ".wav", ".ogg", ".mp3" };

	private static string activeLanguage;

	private static bool isActive;

	private static string activePackDirectory;

	public static Func<IReadOnlyList<string>> SearchRootsProvider;

	public static string RootPath => Path.Combine(Application.persistentDataPath, "Mods", "VoiceOvers");

	public static bool IsActive => isActive;

	public static string ActiveLanguage => activeLanguage;

	public static void Refresh()
	{
		Refresh(LLBase.CurrentLang);
	}

	public static void Refresh(string language)
	{
		isActive = false;
		activeLanguage = language;
		activePackDirectory = null;
		VoiceOverSettings.LanguageId = "en";
		if (!string.IsNullOrEmpty(language) && TryFindPackDirectory(language, out var packDir, out var manifest))
		{
			isActive = true;
			activePackDirectory = packDir;
			VoiceOverSettings.LanguageId = language;
			Debug.Log("[VoiceOverMod] Active pack for '" + language + "' at '" + packDir + "'" + (string.IsNullOrEmpty(manifest.name) ? "." : (": " + manifest.name)));
		}
	}

	private static IReadOnlyList<string> GetSearchRoots()
	{
		if (SearchRootsProvider != null)
		{
			IReadOnlyList<string> readOnlyList = SearchRootsProvider();
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				return readOnlyList;
			}
		}
		return new string[1] { Path.Combine(Application.persistentDataPath, "Mods") };
	}

	private static bool TryFindPackDirectory(string language, out string packDir, out VoiceOverModManifest manifest)
	{
		packDir = null;
		manifest = null;
		IReadOnlyList<string> searchRoots = GetSearchRoots();
		for (int i = 0; i < searchRoots.Count; i++)
		{
			if (TryValidatePack(searchRoots[i], language, out manifest))
			{
				packDir = searchRoots[i];
				return true;
			}
			string text = Path.Combine(searchRoots[i], "VoiceOvers", language);
			if (TryValidatePack(text, language, out manifest))
			{
				packDir = text;
				return true;
			}
			string path = Path.Combine(searchRoots[i], "VoiceOvers");
			if (!Directory.Exists(path))
			{
				continue;
			}
			string[] directories = Directory.GetDirectories(path);
			for (int j = 0; j < directories.Length; j++)
			{
				string fileName = Path.GetFileName(directories[j]);
				if ((string.IsNullOrEmpty(fileName) || fileName[0] != '~') && TryValidatePack(directories[j], language, out manifest))
				{
					packDir = directories[j];
					return true;
				}
			}
		}
		return false;
	}

	private static bool TryValidatePack(string packDir, string language, out VoiceOverModManifest manifest)
	{
		manifest = null;
		if (string.IsNullOrEmpty(packDir) || !Directory.Exists(packDir))
		{
			return false;
		}
		string text = Path.Combine(packDir, "mod.json");
		if (!File.Exists(text))
		{
			return false;
		}
		try
		{
			manifest = JsonUtility.FromJson<VoiceOverModManifest>(File.ReadAllText(text));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[VoiceOverMod] Failed to parse '" + text + "': " + ex.Message);
			return false;
		}
		if (manifest == null)
		{
			return false;
		}
		if (manifest.schemaVersion > 1)
		{
			Debug.LogWarning($"[VoiceOverMod] Unsupported schemaVersion {manifest.schemaVersion} in '{text}'.");
			return false;
		}
		if (!manifest.enabled)
		{
			return false;
		}
		if (!string.Equals(manifest.language, language, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}

	public static bool TryLoadClip(string id, out AudioClip clip)
	{
		clip = null;
		if (!isActive || string.IsNullOrEmpty(activePackDirectory) || string.IsNullOrEmpty(id))
		{
			return false;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(id);
		if (string.IsNullOrEmpty(fileNameWithoutExtension))
		{
			return false;
		}
		if (!TryResolveClipPath(fileNameWithoutExtension, out var clipPath, out var audioType))
		{
			return false;
		}
		try
		{
			using UnityWebRequest unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(new Uri(clipPath).AbsoluteUri, audioType);
			((DownloadHandlerAudioClip)unityWebRequest.downloadHandler).streamAudio = false;
			unityWebRequest.SendWebRequest();
			while (!unityWebRequest.isDone)
			{
			}
			if (unityWebRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.LogWarning("[VoiceOverMod] Failed to load '" + clipPath + "': " + unityWebRequest.error);
				return false;
			}
			clip = DownloadHandlerAudioClip.GetContent(unityWebRequest);
			if (clip == null)
			{
				return false;
			}
			clip.name = fileNameWithoutExtension;
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[VoiceOverMod] Exception loading '" + fileNameWithoutExtension + "': " + ex.Message);
			clip = null;
			return false;
		}
	}

	public static void ReleaseClip(AudioClip clip)
	{
		if (!(clip == null))
		{
			UnityEngine.Object.Destroy(clip);
		}
	}

	private static bool TryResolveClipPath(string fileId, out string clipPath, out AudioType audioType)
	{
		for (int i = 0; i < ClipExtensions.Length; i++)
		{
			string text = ClipExtensions[i];
			string text2 = Path.Combine(activePackDirectory, fileId + text);
			if (File.Exists(text2))
			{
				clipPath = text2;
				audioType = GetAudioType(text);
				return true;
			}
		}
		clipPath = null;
		audioType = AudioType.UNKNOWN;
		return false;
	}

	private static AudioType GetAudioType(string extension)
	{
		return extension.ToLowerInvariant() switch
		{
			".wav" => AudioType.WAV, 
			".ogg" => AudioType.OGGVORBIS, 
			".mp3" => AudioType.MPEG, 
			_ => AudioType.UNKNOWN, 
		};
	}
}
