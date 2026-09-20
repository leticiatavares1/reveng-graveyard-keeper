using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CPWindValue : ControllableParameter
{
	private class Wind
	{
		public float value;

		public WeatherComponent weatherComponent;

		public Wind(float value, WeatherComponent weatherComponent)
		{
			this.value = value;
			this.weatherComponent = weatherComponent;
		}
	}

	[Range(0f, 1f)]
	public float windValue;

	private static Dictionary<CPWindValue, Wind> values = new Dictionary<CPWindValue, Wind>();

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		float value = v * windValue;
		values[this] = new Wind(value, weatherComponent);
	}

	public static void ApplyParameters()
	{
		if (values.Count == 0)
		{
			return;
		}
		Wind wind = null;
		Wind wind2 = null;
		foreach (KeyValuePair<CPWindValue, Wind> value in values)
		{
			if (value.Value.weatherComponent.Animating == WeatherComponent.AnimationType.FadeIn)
			{
				wind = value.Value;
			}
			else if (value.Value.weatherComponent.Animating == WeatherComponent.AnimationType.FadeOut)
			{
				wind2 = value.Value;
			}
		}
		float a = 0f;
		foreach (Wind value2 in values.Values)
		{
			a = Mathf.Max(a, value2.value);
		}
		if (wind != null && wind2 != null && wind.weatherComponent.intensity > 0f && wind2.weatherComponent.intensity > 0f)
		{
			CPWindValue controllableParameterOfType = wind.weatherComponent.GetControllableParameterOfType<CPWindValue>();
			a = Mathf.Lerp(wind2.weatherComponent.GetControllableParameterOfType<CPWindValue>().windValue, controllableParameterOfType.windValue, wind.weatherComponent.intensity);
		}
		WeatherSystem.Instance.WindValue = a;
	}
}
