using System;
using System.Collections.Generic;

[Serializable]
public class WeatherData
{
	public float currentPhaseLen;

	public string stateName = "";

	public List<string> enabledWeatherComponents = new List<string>();

	public bool hasForceState;

	public bool isSoundEnabled;

	public bool isIndoorSfxEnabled;

	public bool isWeatherPausedByTimeOfDay;

	public bool isWeatherPausedByCinematics;
}
