using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeatherPreset", menuName = "WeatherPreset", order = 1)]
public class WeatherPreset : ScriptableObject
{
	public List<WeatherPresetAtom> preset_atoms;

	public string preset_name => base.name;

	public static WeatherPreset GetPreset(string preset_name)
	{
		if (string.IsNullOrEmpty(preset_name))
		{
			Debug.LogError("Preset name is empty!");
			return null;
		}
		WeatherPreset weatherPreset = Resources.Load<WeatherPreset>("Weather/" + preset_name);
		if (weatherPreset == null)
		{
			Debug.LogError("Failed to load WeatherPreset Weather/" + preset_name + ".asset");
		}
		return weatherPreset;
	}

	public override string ToString()
	{
		string text = preset_name + " = {";
		if (preset_atoms.Count > 0)
		{
			foreach (WeatherPresetAtom preset_atom in preset_atoms)
			{
				text += preset_atom.ToString();
				if (preset_atoms[preset_atoms.Count - 1] != preset_atom)
				{
					text += ", ";
				}
			}
		}
		else
		{
			text += "NOTHING";
		}
		return text + "}";
	}
}
