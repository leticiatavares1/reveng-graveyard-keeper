using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LazyBearTechnology;

public class SkinChanger
{
	protected List<SpriteRenderer> sprites;

	protected GameObject gameObject;

	private SkinPresetBase skin;

	private SkinPresetBase fallbackSkin;

	private Dictionary<int, Sprite> skinnedSpriteTopLevelHash = new Dictionary<int, Sprite>();

	private static Dictionary<int, Sprite> spriteHash = new Dictionary<int, Sprite>();

	private static Dictionary<int, bool> validSpriteHash = new Dictionary<int, bool>();

	private static char[] chars = new char[100];

	public static readonly int shaderColorId = Shader.PropertyToID("_Color");

	public static readonly int shaderHueShiftId = Shader.PropertyToID("_HueShift");

	public static readonly int shaderSaturationId = Shader.PropertyToID("_Sat");

	public static readonly int shaderValueId = Shader.PropertyToID("_Val");

	public static readonly int shaderPaletteId = Shader.PropertyToID("_Palette");

	public static readonly int shaderBrightnessId = Shader.PropertyToID("_Brightness");

	public static readonly int shaderContrastId = Shader.PropertyToID("_Contrast");

	public SkinChanger(GameObject gameObject, bool applyShader = true)
	{
		sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true).ToList();
		InitSkinChanger(gameObject, applyShader);
	}

	public SkinChanger(GameObject gameObject, List<SpriteRenderer> spriteRenderers, bool applyShader = true)
	{
		sprites = spriteRenderers;
		InitSkinChanger(gameObject, applyShader);
	}

	public void ApplySkin(SkinPresetBase skinPreset, SkinPresetBase fallbackSkin = null)
	{
		skin = skinPreset;
		this.fallbackSkin = fallbackSkin;
		if (skin != null)
		{
			ApplyShaderParameters();
		}
		skinnedSpriteTopLevelHash.Clear();
	}

	public void CustomLateUpdate()
	{
		if (skin == null)
		{
			return;
		}
		for (int i = 0; i < sprites.Count; i++)
		{
			SpriteRenderer spriteRenderer = sprites[i];
			bool value = false;
			int key = 0;
			if (spriteRenderer.sprite != null)
			{
				key = spriteRenderer.sprite.GetInstanceID();
				if (!validSpriteHash.TryGetValue(key, out value))
				{
					value = IsValidSprite(spriteRenderer);
					validSpriteHash.Add(key, value);
				}
			}
			if (!value)
			{
				continue;
			}
			if (!skinnedSpriteTopLevelHash.TryGetValue(key, out var value2))
			{
				string text = spriteRenderer.sprite.name;
				char @char = text[4];
				char char2 = text[5];
				char char3 = text[6];
				int num = skin.DefineSkinIdFor(@char, char2, char3);
				int num2 = -1;
				if (num == 0 || string.IsNullOrEmpty(text))
				{
					spriteRenderer.enabled = false;
					spriteRenderer.sprite = null;
				}
				else
				{
					spriteRenderer.enabled = true;
					if (num != -1)
					{
						GarbagelessStrings.StringToChars(ref text, ref chars);
						GarbagelessStrings.IntToCharsWithLeadingZeros(num, ref chars, 3, 0);
						int hashCode = GarbagelessStrings.GetHashCode(ref chars);
						if (!spriteHash.TryGetValue(hashCode, out value2))
						{
							string spriteName = GarbagelessStrings.CharsToString(ref chars);
							value2 = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
							if (value2 == null)
							{
								if (fallbackSkin != null)
								{
									num2 = fallbackSkin.DefineSkinIdFor(@char, char2, char3);
									if (num2 != 0)
									{
										GarbagelessStrings.IntToCharsWithLeadingZeros(num2, ref chars, 3, 0);
										hashCode = GarbagelessStrings.GetHashCode(ref chars);
										if (!spriteHash.TryGetValue(hashCode, out value2))
										{
											spriteName = GarbagelessStrings.CharsToString(ref chars);
											value2 = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
											if (value2 == null)
											{
												spriteRenderer.enabled = false;
											}
										}
									}
								}
								else
								{
									spriteRenderer.enabled = false;
									spriteRenderer.sprite = null;
								}
							}
							if (!spriteHash.ContainsKey(hashCode))
							{
								spriteHash.Add(hashCode, value2);
							}
						}
						spriteRenderer.sprite = value2;
					}
				}
				skinnedSpriteTopLevelHash.Add(key, (num == -1 || num2 == -1) ? spriteRenderer.sprite : value2);
			}
			else
			{
				spriteRenderer.sprite = value2;
				spriteRenderer.enabled = value2 != null;
			}
		}
	}

	public void ApplyShaderToAllSprites(Shader shader)
	{
		ApplyShaderToSprites(shader, sprites);
	}

	public void ApplyShaderToSprites(Shader shader, List<SpriteRenderer> sprites = null)
	{
		for (int i = 0; i < sprites?.Count; i++)
		{
			sprites[i].material.shader = shader;
		}
	}

	public void ApplyShaderParameters()
	{
		skin.ApplyShaderParametersTo(sprites);
	}

	public void SetSpriteLayerPosition(string layerName, Vector2 layerPosition)
	{
		SpriteRenderer spriteRenderer = sprites.Find((SpriteRenderer s) => s.name == layerName);
		if (spriteRenderer == null)
		{
			Debug.LogError("$Trying to set position for non exists layer:[" + layerName + "]");
		}
		else
		{
			spriteRenderer.transform.localPosition = new Vector3(layerPosition.x, layerPosition.y, spriteRenderer.transform.localPosition.z);
		}
	}

	private void InitSkinChanger(GameObject gameObject, bool applyShader)
	{
		this.gameObject = gameObject;
		for (int i = 0; i < sprites.Count; i++)
		{
			if (sprites[i].gameObject.name.StartsWith("-"))
			{
				sprites.RemoveAt(i);
				i--;
			}
		}
		if (applyShader)
		{
			ApplyShaderToAllSprites(Shader.Find("Sprites/ColorAdjust"));
		}
	}

	private bool IsValidSprite(SpriteRenderer spriteRenderer)
	{
		if (spriteRenderer.sprite == null)
		{
			return false;
		}
		string name = spriteRenderer.sprite.name;
		if (name.Length <= 7)
		{
			return false;
		}
		for (int i = 0; i < 3; i++)
		{
			char c = name[i];
			if (c < '0' || c > '9')
			{
				return false;
			}
		}
		return true;
	}
}
