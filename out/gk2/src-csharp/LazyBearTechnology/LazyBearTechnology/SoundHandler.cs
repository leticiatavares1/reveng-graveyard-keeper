using System;
using UnityEngine;
using UnityEngine.Audio;

namespace LazyBearTechnology;

public class SoundHandler
{
	private SoundController sourceHolder;

	private bool isActive;

	private float defaultVolume;

	private float defaultPitch;

	private float defaultPanning;

	private Action onSoundPlayed;

	public bool IsActive => isActive;

	public float DefaultVolume => defaultVolume;

	public Action OnSoundPlayed
	{
		get
		{
			return onSoundPlayed;
		}
		set
		{
			onSoundPlayed = value;
		}
	}

	public bool IsPaused
	{
		get
		{
			if (sourceHolder != null)
			{
				return sourceHolder.IsPaused;
			}
			return false;
		}
	}

	public SoundHandler(SoundController sourceHolder)
	{
		this.sourceHolder = sourceHolder;
		defaultVolume = sourceHolder.audioSource.volume;
		defaultPitch = sourceHolder.audioSource.pitch;
		defaultPanning = sourceHolder.audioSource.panStereo;
		sourceHolder.OnSoundPlayed += OnSoundPlayedHandler;
		isActive = true;
	}

	public bool SetVolume(float volume)
	{
		if (!isActive)
		{
			return false;
		}
		sourceHolder.audioSource.volume = defaultVolume * volume;
		return true;
	}

	public float GetVolume()
	{
		return sourceHolder.audioSource.volume;
	}

	public bool SetPitch(float pitch)
	{
		if (!isActive)
		{
			return false;
		}
		sourceHolder.audioSource.pitch = defaultPitch * pitch;
		return true;
	}

	public bool SetPanning(float panning)
	{
		if (!isActive)
		{
			return false;
		}
		sourceHolder.audioSource.panStereo = defaultPanning * panning;
		return true;
	}

	public float GetClipLength()
	{
		return sourceHolder.audioSource.clip.length;
	}

	public float GetTime()
	{
		if (!isActive || sourceHolder == null || sourceHolder.audioSource == null)
		{
			return 0f;
		}
		return sourceHolder.audioSource.time;
	}

	public void SetTime(float time)
	{
		if (isActive && !(sourceHolder == null) && !(sourceHolder.audioSource == null) && !(sourceHolder.audioSource.clip == null))
		{
			sourceHolder.audioSource.time = Mathf.Clamp(time, 0f, sourceHolder.audioSource.clip.length);
		}
	}

	public float GetPitchValue()
	{
		return sourceHolder.audioSource.pitch;
	}

	public bool Pause()
	{
		if (!isActive)
		{
			return false;
		}
		sourceHolder.Pause();
		return true;
	}

	public bool UnPause()
	{
		if (!isActive)
		{
			return false;
		}
		sourceHolder.UnPause();
		return true;
	}

	public void PauseIfActive()
	{
		if (isActive)
		{
			sourceHolder.Pause();
		}
	}

	public void UnPauseIfActive()
	{
		if (isActive)
		{
			sourceHolder.UnPause();
		}
	}

	public bool Stop()
	{
		if (!isActive)
		{
			return false;
		}
		sourceHolder.Stop();
		return true;
	}

	private void OnSoundPlayedHandler()
	{
		isActive = false;
		sourceHolder.OnSoundPlayed -= OnSoundPlayedHandler;
		OnSoundPlayed?.Invoke();
	}

	public void SetFadeInFadeOut(float fadeInSeconds, float fadeOutSeconds)
	{
		sourceHolder.SetFadeInFadeOut(fadeInSeconds, fadeOutSeconds);
		sourceHolder.UpdateVolumeWhilePlaying();
	}

	public bool SetMixerGroup(AudioMixerGroup group)
	{
		if (!isActive || sourceHolder == null || sourceHolder.audioSource == null)
		{
			return false;
		}
		sourceHolder.audioSource.outputAudioMixerGroup = group;
		return true;
	}
}
