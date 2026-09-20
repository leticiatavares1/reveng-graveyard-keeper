using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class CPCloudsDensity : ControllableParameter
{
	private class Clouds
	{
		public float value;

		public WeatherComponent weatherComponent;

		public Clouds(float value, WeatherComponent weatherComponent)
		{
			this.value = value;
			this.weatherComponent = weatherComponent;
		}
	}

	[Range(0f, 1f)]
	public float density = 0.5f;

	private static readonly Dictionary<CPCloudsDensity, Clouds> values = new Dictionary<CPCloudsDensity, Clouds>();

	public static float AppliedDensity { get; private set; }

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		float value = v * density;
		values[this] = new Clouds(value, weatherComponent);
	}

	public static void ApplyParameters()
	{
		if (values.Count == 0)
		{
			return;
		}
		Clouds clouds = null;
		Clouds clouds2 = null;
		foreach (KeyValuePair<CPCloudsDensity, Clouds> value in values)
		{
			if (value.Value.weatherComponent.Animating == WeatherComponent.AnimationType.FadeIn)
			{
				clouds = value.Value;
			}
			else if (value.Value.weatherComponent.Animating == WeatherComponent.AnimationType.FadeOut)
			{
				clouds2 = value.Value;
			}
		}
		float num = 0f;
		foreach (Clouds value2 in values.Values)
		{
			num = Mathf.Max(num, value2.value);
		}
		if (clouds != null && clouds2 != null && clouds.weatherComponent.intensity > 0f && clouds2.weatherComponent.intensity > 0f)
		{
			CPCloudsDensity controllableParameterOfType = clouds.weatherComponent.GetControllableParameterOfType<CPCloudsDensity>();
			num = Mathf.Lerp(clouds2.weatherComponent.GetControllableParameterOfType<CPCloudsDensity>().density, controllableParameterOfType.density, clouds.weatherComponent.intensity);
		}
		AppliedDensity = num;
		if (LazySingletonSO<GlobalResources>.Instance != null && LazySingletonSO<GlobalResources>.Instance.cloudsMaterial != null)
		{
			LazySingletonSO<GlobalResources>.Instance.cloudsMaterial.SetFloat("_CDensitydef05", num);
		}
	}

	public static float ResolveDensityForClouds()
	{
		if (values.Count > 0)
		{
			return AppliedDensity;
		}
		Material material = ((LazySingletonSO<GlobalResources>.Instance != null) ? LazySingletonSO<GlobalResources>.Instance.cloudsMaterial : null);
		if (material != null && material.HasProperty("_CDensitydef05"))
		{
			return material.GetFloat("_CDensitydef05");
		}
		return AppliedDensity;
	}
}
