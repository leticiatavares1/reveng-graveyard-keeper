using System;
using DarkTonic.MasterAudio;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
	public GameObject[] game_objects;

	public void PlayMusic(string sound)
	{
		MasterAudio.TriggerPlaylistClip("music", sound);
	}

	public void PlaySound(string sound)
	{
		string[] array = sound.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			MasterAudio.PlaySound(array[i]);
		}
	}

	public void EnableGameObject(int i)
	{
		if (game_objects != null && game_objects.Length > i)
		{
			game_objects[i].SetActive(value: true);
		}
	}

	public void DisableGameObject(int i)
	{
		if (game_objects != null && game_objects.Length > i)
		{
			game_objects[i].SetActive(value: false);
		}
	}
}
