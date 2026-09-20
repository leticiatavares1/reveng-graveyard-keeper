using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class WindDependentSound : MonoBehaviour
{
	private static readonly List<WindDependentSound> activeSounds = new List<WindDependentSound>();

	[SerializeField]
	private string soundId;

	[SerializeField]
	[Range(0f, 1f)]
	private float maxVolume = 1f;

	[SerializeField]
	[Tooltip("Pitch at wind 0 (x) and wind 1 (y).")]
	private Vector2 pitchRange = new Vector2(0.7f, 1f);

	[SerializeField]
	[Tooltip("Resume playback from the last position when the sound starts again. Works when an AudioSource is taken from the pool.")]
	private bool continueFromSamePlace;

	[SerializeField]
	private SpatialType spatial;

	private SoundHandler soundHandler;

	private float cachedPlaybackTime;

	public static void UpdateSounds()
	{
		if (activeSounds.Count != 0)
		{
			float windIntensity = GetWindIntensity();
			for (int i = 0; i < activeSounds.Count; i++)
			{
				activeSounds[i].ApplyFromWind(windIntensity);
			}
		}
	}

	private void OnEnable()
	{
		activeSounds.Add(this);
		PlaySound();
		ApplyFromWind(GetWindIntensity());
	}

	private void OnDisable()
	{
		activeSounds.Remove(this);
		StopSound();
	}

	private void PlaySound()
	{
		if (!string.IsNullOrEmpty(soundId))
		{
			soundHandler = LazyAudio.PlayAtGameObject(soundId, base.transform, spatial, checkDelay: false);
			if (soundHandler != null && continueFromSamePlace)
			{
				soundHandler.SetTime(cachedPlaybackTime);
			}
		}
	}

	private void StopSound()
	{
		if (soundHandler != null && soundHandler.IsActive)
		{
			if (continueFromSamePlace)
			{
				cachedPlaybackTime = soundHandler.GetTime();
			}
			soundHandler.Stop();
		}
		soundHandler = null;
	}

	private void ApplyFromWind(float windIntensity)
	{
		if (soundHandler != null && soundHandler.IsActive)
		{
			soundHandler.SetVolume(maxVolume * windIntensity);
			soundHandler.SetPitch(Mathf.Lerp(pitchRange.x, pitchRange.y, windIntensity));
		}
	}

	private static float GetWindIntensity()
	{
		if (!(WeatherSystem.Instance != null))
		{
			return 0f;
		}
		return Mathf.Clamp01(WeatherSystem.Instance.WindValue);
	}
}
