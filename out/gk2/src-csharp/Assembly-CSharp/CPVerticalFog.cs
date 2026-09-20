using System;
using UnityEngine;

[Serializable]
public class CPVerticalFog : ControllableParameter
{
	public VerticalFog verticalFog;

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		if (verticalFog.isGlobalController)
		{
			Debug.LogError("Global Vertical Fog controller cannot be controlled by a controllable parameter.", verticalFog);
			return;
		}
		verticalFog.fogEnabled = v > 0f;
		verticalFog.ApplyFogParametersWithIntensity(v);
	}
}
