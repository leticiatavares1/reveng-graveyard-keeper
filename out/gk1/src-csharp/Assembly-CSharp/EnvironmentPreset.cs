using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Environment Preset")]
public class EnvironmentPreset : ScriptableObject
{
	public Texture lut;

	public float lut_morph_speed = 4f;

	public bool disable_timeofday_lut = true;

	public string music_id = "";

	[Space(5f)]
	public bool light_override;

	public Gradient light_grd = new Gradient();

	[Space(5f)]
	public bool ambient_light_override;

	public Gradient ambient_grd = new Gradient();

	[Space(5f)]
	public bool light_sprites_override;

	public Gradient light_sprites = new Gradient();

	[Space(5f)]
	public bool force_static_time;

	public float static_time_value;

	[Space(5f)]
	public bool force_shadows_alpha;

	public float shadows_alpha = 1f;

	[Space(5f)]
	public bool force_light_intensity;

	public float light_intensity = 1f;

	[Space(5f)]
	public bool force_global_shadows_alpha;

	public float global_shadows_alpha = 1f;

	public static EnvironmentPreset Load(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		EnvironmentPreset environmentPreset = Resources.Load<EnvironmentPreset>("Environment presets/" + id);
		if (environmentPreset == null)
		{
			Debug.LogError("Couldn't load env.preset = " + id);
			return null;
		}
		return environmentPreset;
	}
}
