using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SmartWeatherEngine
{
	private static SmartWeatherEngine _me;

	public SmartWeatherSettings night;

	public SmartWeatherSettings morning;

	public SmartWeatherSettings day;

	public SmartWeatherSettings evening;

	public static SmartWeatherEngine me
	{
		get
		{
			if (_me == null)
			{
				_me = new SmartWeatherEngine();
				_me.Init();
			}
			return _me;
		}
	}

	public void Init()
	{
		night = SmartWeatherSettings.GetSettingsPreset("night");
		morning = SmartWeatherSettings.GetSettingsPreset("morning");
		day = SmartWeatherSettings.GetSettingsPreset("day");
		evening = SmartWeatherSettings.GetSettingsPreset("evening");
	}

	public void UpdateWeather()
	{
		int num = MainGame.me.save.day;
		for (int i = 0; i < 4; i++)
		{
			float num2 = num;
			SmartWeatherSettings smartWeatherSettings;
			switch (i)
			{
			case 0:
				smartWeatherSettings = night;
				num2 += 0f;
				break;
			case 1:
				smartWeatherSettings = morning;
				num2 += 0.15f;
				break;
			case 2:
				smartWeatherSettings = day;
				num2 += 0.35f;
				break;
			case 3:
				smartWeatherSettings = evening;
				num2 += 0.7f;
				break;
			default:
				Debug.LogError("HOW THE FUCK THIS CAN HAPEN???!!!");
				return;
			}
			List<string> list = EnvironmentEngine.me.FindNatureWithoutRemoves();
			if (list != null && list.Count > 0)
			{
				foreach (string item in list)
				{
					EnvironmentEngine.me.TryRemoveNatureWeatherState(item, num2, TimeOfDay.FromSecondsToTimeK(10f));
				}
			}
			WeatherPreset weatherPreset = smartWeatherSettings.GetWeatherPreset();
			if (weatherPreset == null)
			{
				Debug.LogError("SmartWeatherEngine::" + smartWeatherSettings.name + " weather_preset is null!");
				break;
			}
			Debug.Log("#weather#Weather for " + smartWeatherSettings.name + ": [" + num2 + ":" + weatherPreset.name + "]");
			foreach (SwitchableWeatherState item2 in SwitchableWeatherState.GetStatesFromPreset(num2, weatherPreset))
			{
				EnvironmentEngine.me.AddNatureWeatherState(item2);
			}
		}
	}
}
