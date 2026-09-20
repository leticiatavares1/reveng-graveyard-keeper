using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class AudioResourceOptimizer
{
	private static readonly Dictionary<string, List<AudioSource>> AudioResourceTargetsByName = new Dictionary<string, List<AudioSource>>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, AudioClip> AudioClipsByName = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, List<AudioClip>> PlaylistClipsByPlaylistName = new Dictionary<string, List<AudioClip>>(5, StringComparer.OrdinalIgnoreCase);

	private static readonly List<string> InternetFilesStartedLoading = new List<string>();

	private static string _supportedLanguageFolder = string.Empty;

	public static void ClearAudioClips()
	{
		AudioClipsByName.Clear();
		AudioResourceTargetsByName.Clear();
	}

	public static string GetLocalizedDynamicSoundGroupFileName(SystemLanguage localLanguage, bool useLocalization, string resourceFileName)
	{
		if (!useLocalization)
		{
			return resourceFileName;
		}
		if (MasterAudio.Instance != null)
		{
			return GetLocalizedFileName(useLocalization, resourceFileName);
		}
		return localLanguage.ToString() + "/" + resourceFileName;
	}

	public static string GetLocalizedFileName(bool useLocalization, string resourceFileName)
	{
		if (!useLocalization)
		{
			return resourceFileName;
		}
		return SupportedLanguageFolder() + "/" + resourceFileName;
	}

	public static void AddTargetForClip(string clipName, AudioSource source)
	{
		if (!AudioResourceTargetsByName.ContainsKey(clipName))
		{
			AudioResourceTargetsByName.Add(clipName, new List<AudioSource> { source });
			return;
		}
		List<AudioSource> list = AudioResourceTargetsByName[clipName];
		AudioClip audioClip = null;
		for (int i = 0; i < list.Count; i++)
		{
			AudioClip clip = list[i].clip;
			if (!(clip == null))
			{
				audioClip = clip;
				break;
			}
		}
		if (audioClip != null)
		{
			source.clip = audioClip;
			SoundGroupVariation component = source.GetComponent<SoundGroupVariation>();
			if (component != null)
			{
				component.internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
			}
		}
		list.Add(source);
	}

	private static string SupportedLanguageFolder()
	{
		if (!string.IsNullOrEmpty(_supportedLanguageFolder))
		{
			return _supportedLanguageFolder;
		}
		SystemLanguage item = Application.systemLanguage;
		if (MasterAudio.Instance != null)
		{
			switch (MasterAudio.Instance.langMode)
			{
			case MasterAudio.LanguageMode.SpecificLanguage:
				item = MasterAudio.Instance.testLanguage;
				break;
			case MasterAudio.LanguageMode.DynamicallySet:
				item = MasterAudio.DynamicLanguage;
				break;
			}
		}
		if (MasterAudio.Instance.supportedLanguages.Contains(item))
		{
			_supportedLanguageFolder = item.ToString();
		}
		else
		{
			_supportedLanguageFolder = MasterAudio.Instance.defaultLanguage.ToString();
		}
		return _supportedLanguageFolder;
	}

	public static void ClearSupportLanguageFolder()
	{
		_supportedLanguageFolder = string.Empty;
	}

	public static AudioClip PopulateResourceSongToPlaylistController(string controllerName, string songResourceName, string playlistName)
	{
		AudioClip audioClip = Resources.Load(songResourceName) as AudioClip;
		if (audioClip == null)
		{
			MasterAudio.LogWarning("Resource file '" + songResourceName + "' could not be located from Playlist '" + playlistName + "'.");
			return null;
		}
		if (!AudioUtil.AudioClipWillPreload(audioClip))
		{
			MasterAudio.LogWarning("Audio Clip for Resource file '" + songResourceName + "' from Playlist '" + playlistName + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
		}
		FinishRecordingPlaylistClip(controllerName, audioClip);
		return audioClip;
	}

	private static void FinishRecordingPlaylistClip(string controllerName, AudioClip resAudioClip)
	{
		List<AudioClip> list;
		if (!PlaylistClipsByPlaylistName.ContainsKey(controllerName))
		{
			list = new List<AudioClip>(5);
			PlaylistClipsByPlaylistName.Add(controllerName, list);
		}
		else
		{
			list = PlaylistClipsByPlaylistName[controllerName];
		}
		list.Add(resAudioClip);
	}

	public static IEnumerator PopulateResourceSongToPlaylistControllerAsync(string songResourceName, string playlistName, PlaylistController controller, PlaylistController.AudioPlayType playType)
	{
		ResourceRequest asyncRes = Resources.LoadAsync(songResourceName, typeof(AudioClip));
		while (!asyncRes.isDone)
		{
			yield return MasterAudio.EndOfFrameDelay;
		}
		AudioClip audioClip = asyncRes.asset as AudioClip;
		if (audioClip == null)
		{
			MasterAudio.LogWarning("Resource file '" + songResourceName + "' could not be located from Playlist '" + playlistName + "'.");
			yield break;
		}
		if (!AudioUtil.AudioClipWillPreload(audioClip))
		{
			MasterAudio.LogWarning("Audio Clip for Resource file '" + songResourceName + "' from Playlist '" + playlistName + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
		}
		FinishRecordingPlaylistClip(controller.ControllerName, audioClip);
		controller.FinishLoadingNewSong(audioClip, playType);
	}

	public static IEnumerator PopulateSourceWithInternetFile(string fileUrl, SoundGroupVariation variation, Action successAction, Action failureAction)
	{
		if (AudioClipsByName.ContainsKey(fileUrl))
		{
			successAction?.Invoke();
		}
		else
		{
			if (InternetFilesStartedLoading.Contains(fileUrl))
			{
				yield break;
			}
			InternetFilesStartedLoading.Add(fileUrl);
			AudioClip audioClip;
			using (WWW fileRequest = new WWW(fileUrl))
			{
				yield return fileRequest;
				if (fileRequest.error != null)
				{
					if (string.IsNullOrEmpty(fileUrl))
					{
						MasterAudio.LogWarning("Internet file is EMPTY for a Variation of Sound Group '" + variation.ParentGroup.name + "' could not be loaded.");
					}
					else
					{
						MasterAudio.LogWarning("Internet file '" + fileUrl + "' in a Variation of Sound Group '" + variation.ParentGroup.name + "' could not be loaded. This can happen if the URL is incorrect or you are not online.");
					}
					failureAction?.Invoke();
					yield break;
				}
				audioClip = fileRequest.GetAudioClip();
				string[] segments = new Uri(fileUrl).Segments;
				audioClip.name = Path.GetFileNameWithoutExtension(segments[segments.Length - 1]);
			}
			if (!AudioResourceTargetsByName.ContainsKey(fileUrl))
			{
				MasterAudio.LogError("No Audio Sources found to add Internet File '" + fileUrl + "' to.");
				failureAction?.Invoke();
				yield break;
			}
			List<AudioSource> list = AudioResourceTargetsByName[fileUrl];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].clip = audioClip;
				SoundGroupVariation component = list[i].GetComponent<SoundGroupVariation>();
				if (!(component == null))
				{
					component.internetFileLoadStatus = MasterAudio.InternetFileLoadStatus.Loaded;
				}
			}
			if (!AudioClipsByName.ContainsKey(fileUrl))
			{
				AudioClipsByName.Add(fileUrl, audioClip);
			}
			successAction?.Invoke();
		}
	}

	public static void RemoveLoadedInternetClip(string fileUrl)
	{
		if (!InternetFilesStartedLoading.Contains(fileUrl))
		{
			return;
		}
		InternetFilesStartedLoading.Remove(fileUrl);
		if (AudioResourceTargetsByName.ContainsKey(fileUrl))
		{
			List<AudioSource> list = AudioResourceTargetsByName[fileUrl];
			for (int i = 0; i < list.Count; i++)
			{
				UnityEngine.Object.Destroy(list[i].clip);
				list[i].clip = null;
			}
			AudioResourceTargetsByName.Remove(fileUrl);
		}
		if (AudioClipsByName.ContainsKey(fileUrl))
		{
			AudioClipsByName.Remove(fileUrl);
		}
	}

	public static IEnumerator PopulateSourcesWithResourceClipAsync(string clipName, SoundGroupVariation variation, Action successAction, Action failureAction)
	{
		if (AudioClipsByName.ContainsKey(clipName))
		{
			successAction?.Invoke();
			yield break;
		}
		ResourceRequest asyncRes = Resources.LoadAsync(clipName, typeof(AudioClip));
		while (!asyncRes.isDone)
		{
			yield return MasterAudio.EndOfFrameDelay;
		}
		AudioClip audioClip = asyncRes.asset as AudioClip;
		if (audioClip == null)
		{
			MasterAudio.LogError("Resource file '" + clipName + "' could not be located.");
			failureAction?.Invoke();
			yield break;
		}
		if (!AudioResourceTargetsByName.ContainsKey(clipName))
		{
			MasterAudio.LogError("No Audio Sources found to add Resource file '" + clipName + "'.");
			failureAction?.Invoke();
			yield break;
		}
		if (!AudioUtil.AudioClipWillPreload(audioClip))
		{
			MasterAudio.LogWarning("Audio Clip for Resource file '" + clipName + "' of Sound Group '" + variation.ParentGroup.name + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
		}
		List<AudioSource> list = AudioResourceTargetsByName[clipName];
		for (int i = 0; i < list.Count; i++)
		{
			list[i].clip = audioClip;
		}
		if (!AudioClipsByName.ContainsKey(clipName))
		{
			AudioClipsByName.Add(clipName, audioClip);
		}
		successAction?.Invoke();
	}

	public static void UnloadPlaylistSongIfUnused(string controllerName, AudioClip clipToRemove)
	{
		if (clipToRemove == null || !PlaylistClipsByPlaylistName.ContainsKey(controllerName))
		{
			return;
		}
		List<AudioClip> list = PlaylistClipsByPlaylistName[controllerName];
		if (list.Contains(clipToRemove))
		{
			list.Remove(clipToRemove);
			if (!list.Contains(clipToRemove))
			{
				Resources.UnloadAsset(clipToRemove);
			}
		}
	}

	public static bool PopulateSourcesWithResourceClip(string clipName, SoundGroupVariation variation)
	{
		if (AudioClipsByName.ContainsKey(clipName))
		{
			return true;
		}
		AudioClip audioClip = Resources.Load(clipName) as AudioClip;
		if (audioClip == null)
		{
			MasterAudio.LogError("Resource file '" + clipName + "' could not be located.");
			return false;
		}
		if (!AudioResourceTargetsByName.ContainsKey(clipName))
		{
			MasterAudio.LogError("No Audio Sources found to add Resource file '" + clipName + "'.");
			return false;
		}
		List<AudioSource> list = AudioResourceTargetsByName[clipName];
		for (int i = 0; i < list.Count; i++)
		{
			list[i].clip = audioClip;
		}
		if (!AudioUtil.AudioClipWillPreload(audioClip))
		{
			MasterAudio.LogWarning("Audio Clip for Resource file '" + clipName + "' of Sound Group '" + variation.ParentGroup.name + "' has 'Preload Audio Data' turned off, which can cause audio glitches. Resource files should always Preload Audio Data. Please turn it on.");
		}
		AudioClipsByName.Add(clipName, audioClip);
		return true;
	}

	public static void DeleteAudioSourceFromList(string clipName, AudioSource source)
	{
		if (!AudioResourceTargetsByName.ContainsKey(clipName))
		{
			MasterAudio.LogError("No Audio Sources found for Resource file '" + clipName + "'.");
			return;
		}
		List<AudioSource> list = AudioResourceTargetsByName[clipName];
		list.Remove(source);
		if (list.Count == 0)
		{
			AudioResourceTargetsByName.Remove(clipName);
		}
	}

	public static void UnloadClipIfUnused(string clipName)
	{
		if (!AudioClipsByName.ContainsKey(clipName))
		{
			return;
		}
		List<AudioSource> list = new List<AudioSource>();
		if (AudioResourceTargetsByName.ContainsKey(clipName))
		{
			list = AudioResourceTargetsByName[clipName];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].GetComponent<SoundGroupVariation>().IsPlaying)
				{
					return;
				}
			}
		}
		AudioClip assetToUnload = AudioClipsByName[clipName];
		for (int j = 0; j < list.Count; j++)
		{
			list[j].clip = null;
		}
		AudioClipsByName.Remove(clipName);
		Resources.UnloadAsset(assetToUnload);
	}
}
