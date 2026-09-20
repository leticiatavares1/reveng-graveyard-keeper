using System;

[Serializable]
public class CPLightEnvironmentPreset : ControllableParameter
{
	public LightEnvironmentPreset preset;

	public override void UpdateParameter(float v, WeatherComponent weatherComponent)
	{
		if (v == 0f)
		{
			EnvironmentEngine.Instance.ApplyOverridePreset((LightEnvironmentPreset)null, 0f);
		}
		else
		{
			EnvironmentEngine.Instance.ApplyOverridePreset(preset, v);
		}
	}
}
