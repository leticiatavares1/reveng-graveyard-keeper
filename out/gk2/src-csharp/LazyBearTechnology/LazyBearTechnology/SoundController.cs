using System;
using UnityEngine;

namespace LazyBearTechnology;

public class SoundController : MonoBehaviour
{
	public string id;

	public float startTime;

	public AudioSource audioSource;

	public SoundHandler soundHandler;

	public Transform target;

	public SpatialType spatial;

	private bool isPaused;

	private float targetVolume;

	private float fadeInTime;

	private float fadeOutTime;

	public bool IsPaused => isPaused;

	public bool IsAlive
	{
		get
		{
			if (!audioSource.isPlaying)
			{
				return isPaused;
			}
			return true;
		}
	}

	public event Action OnSoundPlayed;

	public void Play()
	{
		startTime = Time.time;
		isPaused = false;
		audioSource.Play();
	}

	public void Pause()
	{
		audioSource.Pause();
		isPaused = true;
	}

	public void UnPause()
	{
		audioSource.UnPause();
		isPaused = false;
	}

	public void Stop()
	{
		if (audioSource != null)
		{
			audioSource.Stop();
		}
		isPaused = false;
	}

	public void Reset()
	{
		this.OnSoundPlayed?.Invoke();
		target = null;
		soundHandler = null;
		id = string.Empty;
		base.transform.position = Vector3.zero;
	}

	public void UpdatePosition(Transform microphone)
	{
		if (!(target == null))
		{
			Vector3 position = default(Vector3);
			switch (spatial)
			{
			case SpatialType.sound3D:
				position = target.position;
				break;
			case SpatialType.sound2D:
				position.x = target.position.x;
				position.y = target.position.y;
				position.z = microphone.position.z;
				break;
			case SpatialType.sound1D:
				position.x = target.position.x;
				position.y = microphone.position.y;
				position.z = microphone.position.z;
				break;
			default:
				position = base.transform.position;
				break;
			}
			base.transform.position = position;
		}
	}

	public void SetFadeInFadeOut(float fadeInSeconds, float fadeOutSeconds)
	{
		if (!(audioSource == null) && !(audioSource.clip == null))
		{
			float num = audioSource.clip.length / audioSource.pitch;
			if (fadeInSeconds * 2f > num)
			{
				fadeInSeconds = num / 2f;
			}
			if (fadeOutSeconds * 2f > num)
			{
				fadeOutSeconds = num / 2f;
			}
			fadeInTime = fadeInSeconds;
			fadeOutTime = fadeOutSeconds;
			targetVolume = audioSource.volume;
		}
	}

	public void UpdateVolumeWhilePlaying()
	{
		float num = Time.time - startTime;
		if (num < fadeInTime)
		{
			audioSource.volume = ((fadeInTime > 0.01f) ? Mathf.Lerp(0f, targetVolume, num / fadeInTime) : targetVolume);
		}
		else if (fadeOutTime > 0.01f)
		{
			float num2 = audioSource.clip.length / audioSource.pitch - num;
			if (num2 < fadeOutTime)
			{
				audioSource.volume = Mathf.Lerp(0f, targetVolume, num2 / fadeOutTime);
			}
			else
			{
				audioSource.volume = targetVolume;
			}
		}
	}
}
