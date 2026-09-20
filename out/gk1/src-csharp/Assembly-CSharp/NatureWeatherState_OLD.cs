using System;
using System.Collections.Generic;

[Serializable]
public class NatureWeatherState_OLD : WeatherStateBase
{
	public string preset_name;

	public float v_zero;

	public bool was_activated;

	public static List<NatureWeatherState_OLD> GetStatesFromPreset(float start_time, WeatherPreset preset)
	{
		List<NatureWeatherState_OLD> list = new List<NatureWeatherState_OLD>();
		foreach (WeatherPresetAtom preset_atom in preset.preset_atoms)
		{
			list.Add(new NatureWeatherState_OLD
			{
				preset_name = preset.preset_name,
				t_start = start_time,
				lut_texture = preset_atom.lut_texture,
				type = preset_atom.type,
				value = preset_atom.value,
				t_atk = preset_atom.t_atk
			});
		}
		return list;
	}

	public override string ToString()
	{
		return type.ToString() + "=" + value + "(" + t_atk + ")";
	}
}
