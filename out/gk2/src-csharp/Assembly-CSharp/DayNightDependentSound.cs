using System;
using LazyBearTechnology;
using UnityEngine;

[RequireComponent(typeof(DayNightLight))]
public class DayNightDependentSound : MonoBehaviour
{
	[SerializeField]
	private string soundId;

	[SerializeField]
	private SpatialType spatial = SpatialType.sound1D;

	[SerializeField]
	private DayNightLight dayNightLight;

	private SoundHandler soundHandler;

	private void Awake()
	{
		if ((object)dayNightLight == null)
		{
			dayNightLight = GetComponent<DayNightLight>();
		}
		DayNightLight obj = dayNightLight;
		obj.OnLightIntensityChanged = (Action<float>)Delegate.Combine(obj.OnLightIntensityChanged, new Action<float>(UpdateSound));
	}

	private void OnDestroy()
	{
		soundHandler?.Stop();
		if (dayNightLight != null)
		{
			DayNightLight obj = dayNightLight;
			obj.OnLightIntensityChanged = (Action<float>)Delegate.Remove(obj.OnLightIntensityChanged, new Action<float>(UpdateSound));
		}
	}

	private void OnEnable()
	{
		if (soundHandler == null)
		{
			soundHandler = LazyAudio.PlayAtGameObject(soundId, base.transform, spatial);
		}
		else
		{
			soundHandler.UnPause();
		}
	}

	private void OnDisable()
	{
		soundHandler?.Pause();
	}

	private void UpdateSound(float intensity)
	{
		if (soundHandler != null)
		{
			soundHandler.SetVolume(Mathf.Abs(intensity));
		}
	}
}
