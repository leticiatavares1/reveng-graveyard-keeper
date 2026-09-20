using System;
using System.Collections.Generic;

[Serializable]
public class SwitchableWeatherState : WeatherStateBase
{
	public string preset_name;

	public bool do_dec_now;

	public float start_removing_time;

	public float t_dec;

	public static List<SwitchableWeatherState> GetStatesFromPreset(float start_time, WeatherPreset preset)
	{
		List<SwitchableWeatherState> list = new List<SwitchableWeatherState>();
		foreach (WeatherPresetAtom preset_atom in preset.preset_atoms)
		{
			list.Add(new SwitchableWeatherState
			{
				preset_name = preset.preset_name,
				t_start = start_time,
				lut_texture = preset_atom.lut_texture,
				type = preset_atom.type,
				value = preset_atom.value,
				t_atk = preset_atom.t_atk,
				do_dec_now = false,
				start_removing_time = -1f,
				t_dec = 0f
			});
		}
		return list;
	}

	public override string ToString()
	{
		return type.ToString() + "=" + value + "(" + t_atk + ")";
	}

	public bool HasRemoveCommand()
	{
		if (do_dec_now)
		{
			return true;
		}
		if (start_removing_time > MainGame.game_time)
		{
			return true;
		}
		return false;
	}
}
