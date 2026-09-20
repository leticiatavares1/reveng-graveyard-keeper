using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;

namespace LazyBearTechnology;

[ExecuteInEditMode]
public class LazySpeechEngine : MonoBehaviour
{
	private class SpeechSample
	{
		public bool runAgain;

		public float lastSampleStartTime;

		public float lastPlayedDuration;

		public VoiceData voiceData;

		private readonly List<SoundHandler> soundHandlers = new List<SoundHandler>();

		private const float MIN_TRIGGER_INTERVAL = 0.02f;

		public float MinTime => voiceData.sampleTime / voiceData.pitch;

		public float TriggerInterval
		{
			get
			{
				int num;
				float minTime;
				if (voiceData.playbackMode == VoicePlaybackMode.SampleLength)
				{
					num = ((lastPlayedDuration > 0f) ? 1 : 0);
					if (num != 0)
					{
						minTime = lastPlayedDuration;
						goto IL_002f;
					}
				}
				else
				{
					num = 0;
				}
				minTime = MinTime;
				goto IL_002f;
				IL_002f:
				float num2 = minTime;
				float b = ((num != 0) ? (num2 + voiceData.sampleDelay) : (num2 - voiceData.sampleDelay));
				return Mathf.Max(0.02f, b);
			}
		}

		public bool IsActive
		{
			get
			{
				for (int i = 0; i < soundHandlers.Count; i++)
				{
					if (soundHandlers[i].IsActive)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void RememberPlayedDuration(SoundHandler soundHandler)
		{
			lastPlayedDuration = 0f;
			if (soundHandler != null)
			{
				float clipLength = soundHandler.GetClipLength();
				if (!(clipLength <= 0f))
				{
					float pitchValue = soundHandler.GetPitchValue();
					lastPlayedDuration = ((pitchValue > 0.01f) ? (clipLength / pitchValue) : clipLength);
				}
			}
		}

		public void AddNewSoundHandler(SoundHandler newSoundHandler)
		{
			soundHandlers.Add(newSoundHandler);
		}
	}

	public enum VoicePlaybackMode
	{
		EqualLength,
		SampleLength
	}

	[Serializable]
	public class VoiceData
	{
		public VoiceID voiceId;

		public string soundId;

		public VoicePlaybackMode playbackMode;

		[Range(0.05f, 1f)]
		public float sampleTime = 0.05f;

		[Range(-1f, 1f)]
		public float sampleDelay;

		[Range(0.5f, 1.5f)]
		public float pitch = 1f;

		[Range(0f, 0.5f)]
		public float pitchVariation;

		[Range(0f, 1f)]
		public float volume = 1f;

		[Range(0f, 1f)]
		public float fadeInTime;

		[Range(0f, 1f)]
		public float fadeOutTime;

		[Space]
		public bool hasShortVoice;

		public VoiceID shortVoiceId;

		[Space]
		public bool useOneSampleOncePerCue;

		public void PlayOnce()
		{
			Instance.Play(voiceId);
		}

		public void PlaySeries()
		{
			Instance.Play(voiceId);
			Instance.editorOnly_endlessPlay = true;
		}

		public void Stop()
		{
			Instance.Stop(voiceId);
			Instance.editorOnly_endlessPlay = false;
		}

		public void LoadParametersFromJSON(string filename)
		{
			if (File.Exists(filename))
			{
				VoiceData voiceData = JsonUtility.FromJson<VoiceData>(File.ReadAllText(filename));
				pitch = voiceData.pitch;
				fadeInTime = voiceData.fadeInTime;
				fadeOutTime = voiceData.fadeOutTime;
				sampleTime = voiceData.sampleTime;
				pitchVariation = voiceData.pitchVariation;
			}
		}
	}

	[SerializeField]
	private List<VoiceData> voices;

	private List<SpeechSample> activeSpeechSamples = new List<SpeechSample>();

	private static LazySpeechEngine instance;

	private bool editorOnly_endlessPlay;

	public List<VoiceData> Develop_Voices => voices;

	public static LazySpeechEngine Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<LazySpeechEngine>();
			}
			return instance;
		}
	}

	public void Play(VoiceID voiceId, float remainingPlayingTime = -1f)
	{
		if (voiceId == VoiceID.None)
		{
			return;
		}
		VoiceData voiceData = GetVoiceData(voiceId);
		bool flag = false;
		bool flag2 = true;
		float speechRemainingTime = GetSpeechRemainingTime(voiceId);
		float speechAveragePlayingTime = GetSpeechAveragePlayingTime(voiceId);
		if (voiceData.hasShortVoice && remainingPlayingTime > -1f)
		{
			flag = !IsSpeechPlaying(voiceId) && (IsSpeechPlaying(voiceData.shortVoiceId) || speechAveragePlayingTime > remainingPlayingTime - speechRemainingTime);
			flag2 = ((!IsSpeechPlaying(voiceData.shortVoiceId)) ? (!flag && speechAveragePlayingTime + speechRemainingTime <= remainingPlayingTime) : (GetSpeechAveragePlayingTime(voiceData.shortVoiceId) + GetSpeechRemainingTime(voiceData.shortVoiceId) <= remainingPlayingTime));
		}
		if (flag)
		{
			voiceId = voiceData.shortVoiceId;
			voiceData = GetVoiceData(voiceId);
		}
		bool flag3 = false;
		for (int i = 0; i < activeSpeechSamples.Count; i++)
		{
			SpeechSample speechSample = activeSpeechSamples[i];
			if (speechSample.voiceData.voiceId == voiceId)
			{
				if (flag2)
				{
					speechSample.runAgain = true;
				}
				flag3 = true;
				break;
			}
		}
		if (!flag3)
		{
			SpeechSample speechSample2 = new SpeechSample
			{
				voiceData = voiceData
			};
			PlaySpeechSample(speechSample2);
			activeSpeechSamples.Add(speechSample2);
		}
	}

	public void Stop(VoiceID voiceId)
	{
		for (int i = 0; i < activeSpeechSamples.Count; i++)
		{
			if (activeSpeechSamples[i].voiceData.voiceId == voiceId)
			{
				activeSpeechSamples.RemoveAt(i);
				break;
			}
		}
	}

	public bool IsSpeechPlaying(VoiceID voiceId)
	{
		for (int i = 0; i < activeSpeechSamples.Count; i++)
		{
			if (activeSpeechSamples[i].voiceData.voiceId == voiceId)
			{
				return true;
			}
		}
		return false;
	}

	public float GetSpeechRemainingTime(VoiceID voiceId)
	{
		float result = 0f;
		for (int i = 0; i < activeSpeechSamples.Count; i++)
		{
			SpeechSample speechSample = activeSpeechSamples[i];
			if (speechSample.voiceData.voiceId == voiceId)
			{
				result = speechSample.TriggerInterval - (Time.realtimeSinceStartup - speechSample.lastSampleStartTime);
				break;
			}
		}
		return result;
	}

	public float GetSpeechAveragePlayingTime(VoiceID voiceId)
	{
		for (int i = 0; i < voices.Count; i++)
		{
			if (voices[i].voiceId == voiceId && !string.IsNullOrEmpty(voices[i].soundId))
			{
				return LazySingletonSO<AudioConfig>.Instance.Get(voices[i].soundId).AveragePlayingTime;
			}
		}
		return 0f;
	}

	public VoiceData GetVoiceData(VoiceID voiceId)
	{
		return voices.Find((VoiceData x) => x.voiceId == voiceId);
	}

	private void PlaySpeechSample(SpeechSample speechSample)
	{
		SoundHandler soundHandler = LazyAudio.Play(speechSample.voiceData.soundId);
		if (soundHandler != null)
		{
			soundHandler.SetVolume(speechSample.voiceData.volume);
			soundHandler.SetPitch(UnityEngine.Random.Range(speechSample.voiceData.pitch - speechSample.voiceData.pitchVariation, speechSample.voiceData.pitch + speechSample.voiceData.pitchVariation));
			soundHandler.SetFadeInFadeOut(speechSample.voiceData.fadeInTime, speechSample.voiceData.fadeOutTime);
			AudioMixerGroup audioMixerGroup = ((LazySingletonSO<AudioConfig>.Instance != null) ? LazySingletonSO<AudioConfig>.Instance.mumbleGroup : null);
			if (audioMixerGroup != null)
			{
				soundHandler.SetMixerGroup(audioMixerGroup);
			}
			speechSample.RememberPlayedDuration(soundHandler);
			speechSample.lastSampleStartTime = Time.realtimeSinceStartup;
			speechSample.AddNewSoundHandler(soundHandler);
		}
	}

	private void Update()
	{
		for (int i = 0; i < activeSpeechSamples.Count; i++)
		{
			SpeechSample speechSample = activeSpeechSamples[i];
			if (Time.realtimeSinceStartup - speechSample.lastSampleStartTime > speechSample.TriggerInterval || !speechSample.IsActive)
			{
				if (speechSample.runAgain)
				{
					speechSample.runAgain = false;
					PlaySpeechSample(speechSample);
				}
				else if (!speechSample.IsActive)
				{
					activeSpeechSamples.RemoveAt(i);
					i--;
				}
			}
		}
	}
}
