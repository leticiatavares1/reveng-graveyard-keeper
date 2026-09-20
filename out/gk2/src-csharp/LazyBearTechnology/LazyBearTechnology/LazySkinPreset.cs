using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LazyBearTechnology;

[Serializable]
[CreateAssetMenu(menuName = "Skin Preset [Default]", fileName = "SkinPreset")]
public class LazySkinPreset : SkinPresetBase
{
	public SkinPresetPart body;

	public SkinPresetPart head;

	public SkinPresetPart bot;

	public SkinPresetPart top;

	public Texture2D palette;

	public float hue;

	public float saturation;

	public float velocity;

	public float contrast;

	public float brightness;

	public new static LazySkinPreset Load(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		LazySkinPreset lazySkinPreset = Addressables.LoadAssetAsync<LazySkinPreset>("Skins/" + id + ".asset").WaitForCompletion();
		if (lazySkinPreset == null)
		{
			Debug.LogError("Couldn't load skin preset = " + id);
			return null;
		}
		return lazySkinPreset;
	}

	public override int DefineSkinIdFor(char char4, char char5, char char6)
	{
		int result = -1;
		if (char4 == 'b' && char5 == 'd' && char6 == 'y')
		{
			result = body.id;
		}
		else if (char4 == 'h' && char5 == 'e' && char6 == 'd')
		{
			result = head.id;
		}
		else if (char4 == 'b' && char5 == 'o' && char6 == 't')
		{
			result = bot.id;
		}
		else if (char4 == 't' && char5 == 'o' && char6 == 'p')
		{
			result = top.id;
		}
		return result;
	}

	public override void ApplyShaderParametersTo(List<SpriteRenderer> sprites)
	{
		for (int i = 0; i < sprites.Count; i++)
		{
			if (!TryToApply(sprites[i], "Body", body) && !TryToApply(sprites[i], "Head", head) && !TryToApply(sprites[i], "Top", top))
			{
				TryToApply(sprites[i], "Bot", bot);
			}
		}
	}

	public bool TryToApply(SpriteRenderer sprite, string spriteName, SkinPresetPart skinPreset)
	{
		if (sprite.name == spriteName)
		{
			sprite.material.SetColor(SkinChanger.shaderColorId, skinPreset.color);
			sprite.material.SetFloat(SkinChanger.shaderHueShiftId, skinPreset.hue + hue);
			sprite.material.SetFloat(SkinChanger.shaderSaturationId, skinPreset.saturation + saturation);
			sprite.material.SetFloat(SkinChanger.shaderValueId, skinPreset.velocity + velocity);
			sprite.material.SetFloat(SkinChanger.shaderBrightnessId, brightness);
			sprite.material.SetFloat(SkinChanger.shaderContrastId, contrast);
			Texture2D texture2D = ((palette == null) ? skinPreset.palette : palette);
			if (texture2D == null)
			{
				sprite.material.DisableKeyword("USE_PALETTE_COLOR_REPLACE");
			}
			else
			{
				sprite.material.EnableKeyword("USE_PALETTE_COLOR_REPLACE");
				sprite.material.SetTexture(SkinChanger.shaderPaletteId, texture2D);
			}
			return true;
		}
		return false;
	}
}
