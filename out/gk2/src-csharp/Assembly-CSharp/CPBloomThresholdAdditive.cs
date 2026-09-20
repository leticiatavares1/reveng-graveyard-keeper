using System;
using UnityEngine;

[Serializable]
public class CPBloomThresholdAdditive : ControllableParameter
{
	[Range(-1f, 1f)]
	public float value;

	private float effectiveValue;

	public override void Init()
	{
		CameraSystem.Instance.MainCamera.AddAdditionalBloomThresholdGetter(() => effectiveValue);
	}

	public override void OnDisable()
	{
		effectiveValue = 0f;
	}

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		effectiveValue = Mathf.Lerp(0f, value, v);
		CameraSystem.Instance.MainCamera.UpdateBloomThreshold();
	}
}
