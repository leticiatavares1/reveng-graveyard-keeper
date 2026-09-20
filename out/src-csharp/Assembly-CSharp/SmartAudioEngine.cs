using System.Collections.Generic;
using DarkTonic.MasterAudio;
using LinqTools;
using UnityEngine;
using UnityEngine.Audio;

public class SmartAudioEngine : MonoBehaviour
{
	private enum SoundState
	{
		FadeIn,
		FadeOut,
		Playing
	}

	public List<SmartAudioSound> sounds = new List<SmartAudioSound>();

	private static Dictionary<string, SoundState> _playing_sounds = new Dictionary<string, SoundState>();

	public AudioMixer mixer;

	public PlaylistController pl_music;

	public PlaylistController pl_music_ovr;

	private string _last_ovr_music = "";

	private float _cur_ovr_time;

	private string _stop_ovr_music_asap;

	private List<string> _ovr_musics = new List<string>();

	public float min_ovr_music_time = 20f;

	public float music_crossfade_time = 5f;

	private ObjectDefinition _cur_interaction_npc;

	private static SmartAudioEngine _me = null;

	public static SmartAudioEngine me
	{
		get
		{
			if (_me == null)
			{
				_me = Object.FindObjectOfType<SmartAudioEngine>();
			}
			return _me;
		}
	}

	private SmartAudioSound GetSoundByID(string id)
	{
		foreach (SmartAudioSound sound in sounds)
		{
			if (sound.id == id)
			{
				return sound;
			}
		}
		Debug.LogError("Smart sound not found, id = " + id);
		return null;
	}

	public void SetSoundWeight(string id, float weight)
	{
		GetSoundByID(id)?.SetSoundWeight(weight);
	}

	public void Update()
	{
		if (!MainGame.game_started)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		foreach (SmartAudioSound sound in sounds)
		{
			sound.CustomUpdate(deltaTime);
		}
		if (_ovr_musics.Count > 0)
		{
			_cur_ovr_time += Time.deltaTime;
			if (_cur_ovr_time > min_ovr_music_time && !string.IsNullOrEmpty(_stop_ovr_music_asap))
			{
				StopOvrMusic(_stop_ovr_music_asap);
			}
		}
	}

	public void PlaySound(string sound_group_id)
	{
		foreach (SmartAudioSound sound in sounds)
		{
			if (!(sound.sound_group != sound_group_id))
			{
				sound.Play();
			}
		}
	}

	public void PlaySoundWithFade(string sound_group_id)
	{
		foreach (SmartAudioSound sound in sounds)
		{
			if (!(sound.sound_group != sound_group_id))
			{
				sound.PlayWithFade();
			}
		}
	}

	public void SetSoundVolume(string sound_group_id, float volume)
	{
		foreach (SmartAudioSound sound in sounds)
		{
			if (!(sound.sound_group != sound_group_id))
			{
				sound.SetSoundVolume(volume);
			}
		}
	}

	public void StopSoundWithFade(string sound_group_id)
	{
		foreach (SmartAudioSound sound in sounds)
		{
			if (!(sound.sound_group != sound_group_id))
			{
				sound.StopWithFade();
			}
		}
	}

	public void SetDullMusicMode(bool dull_mode = true)
	{
		Debug.Log("SetDullMusicMode " + dull_mode);
		mixer.SetFloat("music_cutoff", dull_mode ? 900 : 44000);
		mixer.SetFloat("sfx_cutoff", dull_mode ? 5000 : 22000);
		mixer.SetFloat("environment_cutoff", dull_mode ? 5000 : 22000);
		if (dull_mode)
		{
			Sounds.OnWindowOpened();
		}
	}

	public void SetChannelVolume(string channel_name, float volume_0_1, float add = 0f)
	{
		float t = (Mathf.Log10(volume_0_1 + 0.1f) + 1f) * 0.96f;
		float num = Mathf.Lerp(-80f, 0f, t);
		mixer.SetFloat("volume_" + channel_name, num + add);
	}

	private void OnOvrMusicStopped(string music_id)
	{
		if (_ovr_musics.Count > 0 && _ovr_musics[_ovr_musics.Count - 1] == music_id)
		{
			_ovr_musics.RemoveAt(_ovr_musics.Count - 1);
		}
	}

	private void OnOvrMusicStarted(string music_id)
	{
		if (_ovr_musics.Contains(music_id))
		{
			_ovr_musics.Remove(music_id);
		}
		_ovr_musics.Add(music_id);
	}

	public void StopOvrMusic(string music_id = null, bool force_immediate = false)
	{
		Debug.Log("#snd# StopOvrMusic: " + music_id);
		_stop_ovr_music_asap = null;
		if (string.IsNullOrEmpty(music_id))
		{
			_ovr_musics.Clear();
		}
		else if (((_ovr_musics.Count == 0) ? null : _ovr_musics.Last()) == music_id)
		{
			if (!(_cur_ovr_time > min_ovr_music_time || force_immediate))
			{
				_stop_ovr_music_asap = music_id;
				return;
			}
			_ovr_musics.Remove(music_id);
		}
		else
		{
			_ovr_musics.Remove(music_id);
		}
		if (_ovr_musics.Count == 0)
		{
			pl_music_ovr.FadeToVolume(0f, music_crossfade_time);
			pl_music.FadeToVolume(1f, music_crossfade_time);
		}
		else
		{
			_last_ovr_music = _ovr_musics.Last();
			pl_music_ovr.TriggerPlaylistClip(_last_ovr_music);
		}
	}

	public void PlayOvrMusic(string music_id)
	{
		Debug.Log("#snd# PlayOvrMusic: " + music_id);
		_stop_ovr_music_asap = null;
		string text = ((_ovr_musics.Count == 0) ? null : _ovr_musics.Last());
		if (music_id == text)
		{
			return;
		}
		if (string.IsNullOrEmpty(text))
		{
			if (!pl_music_ovr.ActiveAudioSource.isPlaying)
			{
				pl_music_ovr.StartPlaylist("ovr_music");
			}
			pl_music_ovr.FadeToVolume(1f, music_crossfade_time);
			pl_music.FadeToVolume(0f, music_crossfade_time);
		}
		if (_last_ovr_music != music_id)
		{
			pl_music_ovr.TriggerPlaylistClip(music_id);
			_last_ovr_music = music_id;
		}
		if (_ovr_musics.Contains(music_id))
		{
			_ovr_musics.Remove(music_id);
		}
		_ovr_musics.Add(music_id);
		_cur_ovr_time = 0f;
		_stop_ovr_music_asap = null;
	}

	public void OnStartNPCInteraction(ObjectDefinition npc)
	{
		if (_cur_interaction_npc != npc)
		{
			_cur_interaction_npc = npc;
			if (!string.IsNullOrEmpty(npc.ovr_music))
			{
				PlayOvrMusic(npc.ovr_music);
			}
		}
	}

	public void OnEndNPCInteraction(ObjectDefinition npc = null)
	{
		if (_cur_interaction_npc != null && (npc == _cur_interaction_npc || npc == null))
		{
			if (!string.IsNullOrEmpty(_cur_interaction_npc.ovr_music))
			{
				StopOvrMusic(_cur_interaction_npc.ovr_music);
			}
			_cur_interaction_npc = null;
		}
	}

	public void TransitionToSnapshot(string snapshot_name, float time_to_reach = 0f)
	{
		AudioMixerSnapshot audioMixerSnapshot = mixer.FindSnapshot(snapshot_name);
		if (audioMixerSnapshot != null)
		{
			audioMixerSnapshot.TransitionTo(time_to_reach);
			Debug.Log("Snapshot " + snapshot_name + " transition with time: " + time_to_reach);
		}
		else
		{
			Debug.Log("Tried to transition to snapshot with name " + snapshot_name + " , but wasn't found");
		}
	}

	public void StopAllSmartSounds()
	{
		foreach (SmartAudioSound sound in sounds)
		{
			sound.Stop();
			sound.CustomUpdate(Time.deltaTime);
		}
	}
}
