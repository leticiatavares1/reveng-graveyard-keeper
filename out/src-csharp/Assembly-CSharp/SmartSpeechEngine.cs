using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SmartSpeechEngine : MonoBehaviour
{
	public enum VoiceID
	{
		None = 0,
		Skull = 1,
		Bishop = 2,
		Inquisitor = 3,
		Actress = 4,
		Merchant = 5,
		Cultist = 6,
		Astrologer = 7,
		Guard = 8,
		Donkey = 9,
		CellPhone = 10,
		TavernKeeper = 11,
		Blacksmith = 12,
		Actor = 13,
		RedEye = 14,
		Dig = 15,
		Carpenter = 16,
		FarmersSon = 17,
		FarmersDaughter = 18,
		Engineer = 19,
		Capitan = 20,
		Witch = 21,
		LightKeeper = 22,
		Miller = 23,
		Farmer = 24,
		WoodCutter = 25,
		Ghost = 26,
		Zombie = 27,
		LordCommander = 28,
		Satyr = 29,
		Gypsy = 30,
		MsChain = 31,
		Gunter = 32,
		Bella = 33,
		Jove = 34,
		Lucius = 35,
		Teacher = 36,
		WitchYoung = 37,
		MasterAlarich = 38,
		Beatris = 39,
		MarquisTeodoroJr = 40,
		Hunchback = 41,
		GhostPriest = 42,
		WomanBlackGold = 43,
		Shepherd = 44,
		VampireCommon = 45,
		RefugeeCoffinMaker = 46,
		RefugeeCook = 47,
		RefugeeTanner = 48,
		MarquisTeodoroJrAfflicted = 49,
		RefugeeMoneylender = 50,
		RefugeeMan = 51,
		ShepherdsWife = 52,
		VampireCarl = 53,
		Potter = 54,
		BeeKeeper = 55,
		Euric = 56,
		Smiler = 57,
		EuricInBag = 58,
		Player = 100,
		Unused = 101,
		Unused2 = 102,
		Unused3 = 103,
		Unused4 = 104,
		Unused5 = 105
	}

	[Serializable]
	public class VoiceDefinition
	{
		public VoiceID id;

		[Range(0.05f, 1f)]
		public float min_period = 0.13f;

		[Range(0f, 1f)]
		public float volume = 1f;

		[Range(0.5f, 1.5f)]
		public float pitch = 1f;

		public AudioClipDefinition[] sounds;

		private float _last_time_played;

		private string _play_button_symbol = "►►";

		[NonSerialized]
		public int prev_random = -1;

		private void Play()
		{
			me.PlayVoiceSound(id);
		}

		private void PlaySeries()
		{
			_play_button_symbol = "...";
			me.PlayVoiceSeries(id, UnityEngine.Random.Range(0.4f, 1.5f), delegate
			{
				_play_button_symbol = "►►";
			});
		}

		public bool IsAvailableToPlay()
		{
			if (Time.realtimeSinceStartup > _last_time_played + min_period / pitch)
			{
				_last_time_played = Time.realtimeSinceStartup;
				return true;
			}
			return false;
		}
	}

	[Serializable]
	public class AudioClipDefinition
	{
		public AudioClip audio;

		[Range(0f, 1f)]
		public float volume = 1f;

		private void Play()
		{
			float voice_volume = 1f;
			float pitch = 1f;
			foreach (VoiceDefinition voice in me.voices)
			{
				AudioClipDefinition[] sounds = voice.sounds;
				for (int i = 0; i < sounds.Length; i++)
				{
					if (sounds[i] == this)
					{
						voice_volume = voice.volume;
						pitch = voice.pitch;
					}
				}
			}
			me.PlaySound(0, this, voice_volume, pitch);
		}
	}

	public List<VoiceDefinition> voices = new List<VoiceDefinition>();

	private static SmartSpeechEngine _me;

	private AudioSource[] _audio_sources;

	private uint[] _audio_queue;

	private uint _cur_audio_qn;

	private bool _playing_series;

	private VoiceID _playing_series_id;

	private float _playing_series_end_time;

	private Action _on_series_finished;

	public static SmartSpeechEngine me
	{
		get
		{
			if (_me == null)
			{
				_me = UnityEngine.Object.FindObjectOfType<SmartSpeechEngine>();
			}
			return _me;
		}
	}

	[ContextMenu("Init")]
	public void Init()
	{
		_audio_sources = GetComponentsInChildren<AudioSource>(includeInactive: true);
		_audio_queue = new uint[_audio_sources.Length];
		_cur_audio_qn = 0u;
	}

	public void PlayVoiceSound(VoiceID voice_id, float volume = 1f)
	{
		if (_audio_sources == null || _audio_sources.Length == 0)
		{
			Init();
		}
		int channel_number = 0;
		uint num = _audio_queue[0];
		for (int i = 1; i < _audio_sources.Length; i++)
		{
			uint num2 = _audio_queue[i];
			if (num2 < num)
			{
				channel_number = i;
				num = num2;
			}
		}
		foreach (VoiceDefinition voice in voices)
		{
			if (voice.id == voice_id)
			{
				PlayVoiceSoundInternal(channel_number, voice, volume);
				break;
			}
		}
	}

	private void PlayVoiceSoundInternal(int channel_number, VoiceDefinition vd, float volume = 1f)
	{
		if (!vd.IsAvailableToPlay())
		{
			return;
		}
		int num = 0;
		if (vd.sounds.Length == 0)
		{
			return;
		}
		if (vd.sounds.Length > 1)
		{
			do
			{
				num = NGUITools.RandomRange(0, vd.sounds.Length - 1);
			}
			while (num == vd.prev_random);
			vd.prev_random = num;
		}
		AudioClipDefinition sound = vd.sounds[num];
		PlaySound(channel_number, sound, vd.volume * volume, vd.pitch);
	}

	private void PlaySound(int channel_number, AudioClipDefinition sound, float voice_volume = 1f, float pitch = 1f)
	{
		_audio_queue[channel_number] = ++_cur_audio_qn;
		AudioSource audioSource = _audio_sources[channel_number];
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
		}
		audioSource.clip = sound.audio;
		audioSource.volume = sound.volume * voice_volume;
		audioSource.pitch = pitch;
		audioSource.Play();
	}

	public void PlayVoiceSeries(VoiceID voice_id, float length, Action on_series_finished = null)
	{
		_playing_series = true;
		_playing_series_id = voice_id;
		_playing_series_end_time = Time.realtimeSinceStartup + length;
		_on_series_finished = on_series_finished;
		PlayVoiceSound(voice_id);
	}

	private void UpdatePlayingSeries()
	{
		if (_me == null || !_playing_series)
		{
			return;
		}
		PlayVoiceSound(_playing_series_id);
		if (_playing_series_end_time < Time.realtimeSinceStartup)
		{
			_playing_series = false;
			if (_on_series_finished != null)
			{
				Action on_series_finished = _on_series_finished;
				_on_series_finished = null;
				on_series_finished();
			}
		}
	}

	public void Update()
	{
		UpdatePlayingSeries();
	}
}
