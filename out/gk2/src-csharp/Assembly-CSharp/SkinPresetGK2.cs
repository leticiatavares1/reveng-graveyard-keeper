using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Skin Preset [GK2]", fileName = "SkinPreset")]
public class SkinPresetGK2 : SkinPresetBase
{
	public bool isPlayerPreset;

	public SkinPresetPartGK2 body;

	public SkinPresetPartGK2 head;

	public SkinPresetPartGK2 arms;

	public SkinPresetPartGK2 beard;

	public SkinPresetPartGK2 hairstyle;

	[Space]
	public Texture2D palette;

	public float hue;

	public float saturation;

	public float velocity;

	public float contrast;

	public float brightness;

	private static Dictionary<SkinPresetGK2, (AsyncOperationHandle<SkinPresetGK2>, int)> loadedSkinPresets = new Dictionary<SkinPresetGK2, (AsyncOperationHandle<SkinPresetGK2>, int)>();

	public override int DefineSkinIdFor(char char4, char char5, char char6)
	{
		int num = (isPlayerPreset ? (-1) : DefineSkinIdForNotPlayer(char4, char5, char6));
		if (num == -1)
		{
			if (char4 == 'b' && char5 == 'd' && char6 == 'y')
			{
				num = body.id;
			}
			else if (char4 == 'h' && char5 == 'e' && char6 == 'd')
			{
				num = head.id;
			}
			else if (char4 == 'a' && char5 == 'r' && char6 == 'm')
			{
				num = arms.id;
			}
			else if (char4 == 'b' && char5 == 'r' && char6 == 'd')
			{
				num = beard.id;
			}
			else if (char4 == 'h' && char5 == 'r' && char6 == 's')
			{
				num = hairstyle.id;
			}
		}
		return num;
	}

	public override void ApplyShaderParametersTo(List<SpriteRenderer> sprites)
	{
		for (int i = 0; i < sprites.Count; i++)
		{
			SpriteRenderer sprite = sprites[i];
			if (!TryToApply(sprite, "hed", head) && !TryToApply(sprite, "brd", beard) && !TryToApply(sprite, "hrs", hairstyle) && !TryToApply(sprite, "bdy", body) && !TryToApply(sprite, "arm", arms) && !TryToApply(sprite, "bdy_over", body) && !TryToApply(sprite, "bdy_ovr_hor", body))
			{
				TryToApply(sprite, "leg", body);
			}
		}
	}

	public void ApplyShaderParametersTo(List<Image> images)
	{
		for (int i = 0; i < images.Count; i++)
		{
			Image image = images[i];
			if (!TryToApply(image, "hed", head) && !TryToApply(image, "brd", beard) && !TryToApply(image, "hrs", hairstyle) && !TryToApply(image, "bdy", body) && !TryToApply(image, "arm", arms) && !TryToApply(image, "bdy_over", body) && !TryToApply(image, "bdy_ovr_hor", body))
			{
				TryToApply(image, "leg", body);
			}
		}
	}

	public static SkinPresetGK2 LoadAsset(string id)
	{
		AsyncOperationHandle<SkinPresetGK2> item = Addressables.LoadAssetAsync<SkinPresetGK2>("Assets/AddressableAssets/Skins/" + id + ".asset");
		SkinPresetGK2 skinPresetGK = item.WaitForCompletion();
		if (skinPresetGK == null)
		{
			Debug.LogError("Couldn't load skin preset = " + id);
			return null;
		}
		if (item.Status == AsyncOperationStatus.Succeeded)
		{
			if (loadedSkinPresets.TryGetValue(skinPresetGK, out var value))
			{
				loadedSkinPresets[skinPresetGK] = (value.Item1, value.Item2 + 1);
			}
			else
			{
				loadedSkinPresets[skinPresetGK] = (item, 1);
			}
		}
		return skinPresetGK;
	}

	public new static SkinPresetGK2 Load(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		AsyncOperationHandle<SkinPresetGK2> item = Addressables.LoadAssetAsync<SkinPresetGK2>("Assets/AddressableAssets/Skins/" + id + ".asset");
		SkinPresetGK2 skinPresetGK = item.WaitForCompletion();
		if (skinPresetGK == null)
		{
			Debug.LogError("Couldn't load skin preset = " + id);
			return null;
		}
		if (item.Status == AsyncOperationStatus.Succeeded)
		{
			if (loadedSkinPresets.TryGetValue(skinPresetGK, out var value))
			{
				loadedSkinPresets[skinPresetGK] = (value.Item1, value.Item2 + 1);
			}
			else
			{
				loadedSkinPresets[skinPresetGK] = (item, 1);
			}
		}
		return skinPresetGK;
	}

	public static void ReleaseAsset(SkinPresetGK2 skinPreset)
	{
		if (skinPreset != null && loadedSkinPresets.TryGetValue(skinPreset, out var value))
		{
			int num = value.Item2 - 1;
			Addressables.Release(value.Item1);
			if (num <= 0)
			{
				loadedSkinPresets.Remove(skinPreset);
			}
			else
			{
				loadedSkinPresets[skinPreset] = (value.Item1, num);
			}
		}
	}

	public bool TryToApply(SpriteRenderer sprite, string spriteName, SkinPresetPartGK2 skinPreset)
	{
		return TryToApply(sprite.name, sprite.material, spriteName, skinPreset);
	}

	public bool TryToApply(Image image, string spriteName, SkinPresetPartGK2 skinPreset)
	{
		return TryToApply(image.name, image.material, spriteName, skinPreset);
	}

	private bool TryToApply(string spriteObjName, Material material, string spriteName, SkinPresetPartGK2 skinPreset)
	{
		if (spriteObjName == spriteName)
		{
			material.SetColor(SkinChangerGK2.shaderColorId, skinPreset.color);
			material.SetFloat(SkinChangerGK2.shaderHueShiftId, skinPreset.hue + hue);
			material.SetFloat(SkinChangerGK2.shaderSaturationId, skinPreset.saturation + saturation);
			material.SetFloat(SkinChangerGK2.shaderValueId, skinPreset.velocity + velocity);
			material.SetFloat(SkinChangerGK2.shaderBrightnessId, brightness);
			material.SetFloat(SkinChangerGK2.shaderContrastId, contrast);
			Texture2D texture2D = ((palette == null) ? skinPreset.palette : palette);
			material.DisableKeyword(ColorReplaceType.USE_LUT_COLOR_REPLACE.ToString());
			material.DisableKeyword(ColorReplaceType.USE_PALETTE_COLOR_REPLACE.ToString());
			if (texture2D == null)
			{
				material.DisableKeyword(skinPreset.colorReplaceType.ToString());
			}
			else
			{
				material.EnableKeyword(skinPreset.colorReplaceType.ToString());
				if (skinPreset.colorReplaceType == ColorReplaceType.USE_LUT_COLOR_REPLACE)
				{
					material.SetTexture(SkinChangerGK2.shaderPaletteLutId, texture2D);
				}
				else
				{
					material.SetTexture(SkinChangerGK2.shaderPaletteId, texture2D);
				}
			}
			return true;
		}
		return false;
	}

	public int GetSkinPresetPartId(PlayerColorCustomizationType colorCustomizationType)
	{
		switch (colorCustomizationType)
		{
		case PlayerColorCustomizationType.Hed:
			return hairstyle.id;
		case PlayerColorCustomizationType.Bdy1:
		case PlayerColorCustomizationType.Bdy2:
		case PlayerColorCustomizationType.Bdy3:
			return body.id;
		default:
			throw new ArgumentOutOfRangeException("colorCustomizationType", colorCustomizationType, null);
		}
	}

	private int DefineSkinIdForNotPlayer(char char4, char char5, char char6)
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
		return result;
	}
}
