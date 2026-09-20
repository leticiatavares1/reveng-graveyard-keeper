using System;
using System.Collections.Generic;
using UnityEngine;

public static class AudioLoaderOptimizer
{
	private static readonly Dictionary<string, List<GameObject>> PlayingGameObjectsByClipName = new Dictionary<string, List<GameObject>>(StringComparer.OrdinalIgnoreCase);

	public static void AddNonPreloadedPlayingClip(AudioClip clip, GameObject maHolderGameObject)
	{
		if (clip == null)
		{
			return;
		}
		string name = clip.name;
		if (!PlayingGameObjectsByClipName.ContainsKey(name))
		{
			PlayingGameObjectsByClipName.Add(name, new List<GameObject> { maHolderGameObject });
			return;
		}
		List<GameObject> list = PlayingGameObjectsByClipName[name];
		if (!list.Contains(maHolderGameObject))
		{
			list.Add(maHolderGameObject);
		}
	}

	public static void RemoveNonPreloadedPlayingClip(AudioClip clip, GameObject maHolderGameObject)
	{
		if (clip == null)
		{
			return;
		}
		string name = clip.name;
		if (PlayingGameObjectsByClipName.ContainsKey(name))
		{
			List<GameObject> list = PlayingGameObjectsByClipName[name];
			list.Remove(maHolderGameObject);
			if (list.Count == 0)
			{
				PlayingGameObjectsByClipName.Remove(name);
			}
		}
	}

	public static bool IsAnyOfNonPreloadedClipPlaying(AudioClip clip)
	{
		if (clip == null)
		{
			return false;
		}
		return PlayingGameObjectsByClipName.ContainsKey(clip.name);
	}
}
