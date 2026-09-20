using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SmartWeatherSettings", menuName = "SmartWeatherSettings", order = 1)]
public class SmartWeatherSettings : ScriptableObject
{
	public List<SmartWeatherSettingsAtom> weather_settings;

	public static SmartWeatherSettings GetSettingsPreset(string preset_name)
	{
		if (string.IsNullOrEmpty(preset_name))
		{
			Debug.LogError("Preset name is empty!");
			return null;
		}
		SmartWeatherSettings smartWeatherSettings = Resources.Load<SmartWeatherSettings>("Weather/Settings/" + preset_name);
		if (smartWeatherSettings == null)
		{
			Debug.LogError("Failed to load SmartWeatherSettings Weather/Settings/" + preset_name + ".asset");
		}
		return smartWeatherSettings;
	}

	public WeatherPreset GetWeatherPreset()
	{
		if (weather_settings == null || weather_settings.Count == 0)
		{
			Debug.LogError("SmartWeatherPreset " + base.name + " is FUCKING BROKEN!!!");
			return null;
		}
		if (weather_settings.Count == 1)
		{
			return weather_settings[0].weather_preset;
		}
		float num = 0f;
		foreach (SmartWeatherSettingsAtom weather_setting in weather_settings)
		{
			num += weather_setting.weight;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (SmartWeatherSettingsAtom weather_setting2 in weather_settings)
		{
			num3 += weather_setting2.weight;
			if (num3 > num2)
			{
				return weather_setting2.weather_preset;
			}
		}
		Debug.LogError("Some weird stuff happen! [rand = " + num2 + "], [max = " + num + "]");
		return null;
	}
}
