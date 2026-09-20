using System.Collections.Generic;
using UnityEngine;

public class LUTController : MonoBehaviour
{
	public const int COLOR_EFFECTS_ON_INIT = 4;

	public List<AmplifyColorEffect> color_effects;

	public GameObject main_camera;

	public void InitLUTController()
	{
		if (main_camera == null)
		{
			main_camera = base.gameObject;
		}
		color_effects = new List<AmplifyColorEffect>();
		for (int i = 0; i < 4; i++)
		{
			AmplifyColorEffect amplifyColorEffect = main_camera.AddComponent<AmplifyColorEffect>();
			amplifyColorEffect.enabled = false;
			color_effects.Add(amplifyColorEffect);
		}
	}

	public void UpdateColorEffects(List<LUTAtom> effects)
	{
		if (color_effects == null || color_effects.Count == 0)
		{
			InitLUTController();
		}
		while (effects.Count > color_effects.Count)
		{
			AddColorEffect();
		}
		for (int i = 0; i < color_effects.Count; i++)
		{
			if (i >= effects.Count)
			{
				color_effects[i].enabled = false;
				continue;
			}
			color_effects[i].enabled = true;
			if (color_effects[i].LutBlendTexture != effects[i].lut_texture)
			{
				color_effects[i].LutBlendTexture = effects[i].lut_texture;
			}
			color_effects[i].BlendAmount = effects[i].value;
		}
	}

	private void AddColorEffect()
	{
		if (main_camera == null)
		{
			Debug.LogError("main_camera is null!");
			return;
		}
		AmplifyColorEffect amplifyColorEffect = main_camera.AddComponent<AmplifyColorEffect>();
		amplifyColorEffect.enabled = false;
		color_effects.Add(amplifyColorEffect);
	}
}
