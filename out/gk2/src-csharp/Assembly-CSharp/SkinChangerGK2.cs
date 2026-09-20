using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

public class SkinChangerGK2
{
	private const int MAX_STATIC_SPRITE_CACHE_SIZE = 4096;

	private const string STATIC_TOKEN = "_static_";

	protected List<SpriteRenderer> sprites;

	protected GameObject gameObject;

	private SkinPresetBase skin;

	private SkinPresetBase fallbackSkin;

	private Dictionary<int, Sprite> skinnedSpriteTopLevelHash = new Dictionary<int, Sprite>();

	private Dictionary<int, bool> validSpriteHash = new Dictionary<int, bool>();

	private Dictionary<(int sourceSpriteId, int frame), Sprite> headFrameVariantCache = new Dictionary<(int, int), Sprite>();

	private SpriteRenderer headSpriteRenderer;

	private SpriteRenderer beardSpriteRenderer;

	private int headFrameOverride = 1;

	private bool hasCustomBeardFrames;

	private static Dictionary<int, Sprite> spriteHash = new Dictionary<int, Sprite>();

	private static Dictionary<int, bool> customHeadFramesAvailabilityCache = new Dictionary<int, bool>();

	private static Dictionary<int, bool> customBeardFramesAvailabilityCache = new Dictionary<int, bool>();

	private static char[] chars = new char[100];

	public static readonly int shaderColorId = Shader.PropertyToID("_Color");

	public static readonly int shaderHueShiftId = Shader.PropertyToID("_HueShift");

	public static readonly int shaderSaturationId = Shader.PropertyToID("_Sat");

	public static readonly int shaderValueId = Shader.PropertyToID("_Val");

	public static readonly int shaderPaletteId = Shader.PropertyToID("_Palette");

	public static readonly int shaderPaletteLutId = Shader.PropertyToID("_ReplaceLUT");

	public static readonly int shaderBrightnessId = Shader.PropertyToID("_Brightness");

	public static readonly int shaderContrastId = Shader.PropertyToID("_Contrast");

	public bool HasCustomHeadFrames { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStaticCaches()
	{
		spriteHash.Clear();
		customHeadFramesAvailabilityCache.Clear();
		customBeardFramesAvailabilityCache.Clear();
	}

	public static void ClearStaticCaches()
	{
		spriteHash.Clear();
		customHeadFramesAvailabilityCache.Clear();
		customBeardFramesAvailabilityCache.Clear();
	}

	public SkinChangerGK2(GameObject gameObject, bool applyShader = false)
	{
		sprites = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true).ToList();
		InitSkinChanger(gameObject, applyShader);
	}

	public SkinChangerGK2(GameObject gameObject, List<SpriteRenderer> spriteRenderers, bool applyShader = false)
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
		headFrameVariantCache.Clear();
		headFrameOverride = 1;
		DetectCustomHeadFrames();
		DetectCustomBeardFrames();
		UpdateSkinnedSprites();
	}

	public void SetHeadFrameOverride(int frame)
	{
		headFrameOverride = ((frame < 1) ? 1 : frame);
	}

	public void CustomLateUpdate()
	{
		if (!(skin == null))
		{
			UpdateSkinnedSprites();
		}
	}

	private void UpdateSkinnedSprites()
	{
		for (int i = 0; i < sprites.Count; i++)
		{
			SpriteRenderer spriteRenderer = sprites[i];
			if (!TryGetAnimationSpriteId(spriteRenderer, out var id))
			{
				continue;
			}
			Sprite sprite = ResolveSkinnedSprite(spriteRenderer, id);
			if (headFrameOverride > 1 && sprite != null)
			{
				if (HasCustomHeadFrames && (object)spriteRenderer == headSpriteRenderer)
				{
					sprite = ResolveHeadFrameVariant(sprite, id, headFrameOverride) ?? sprite;
				}
				else if (hasCustomBeardFrames && (object)spriteRenderer == beardSpriteRenderer)
				{
					sprite = ResolveHeadFrameVariant(sprite, id, headFrameOverride) ?? sprite;
				}
			}
			ApplyResolvedSprite(spriteRenderer, sprite);
		}
	}

	private Sprite ResolveHeadFrameVariant(Sprite resolvedSprite, int sourceSpriteId, int frame)
	{
		(int, int) key = (sourceSpriteId, frame);
		if (headFrameVariantCache.TryGetValue(key, out var value))
		{
			return value;
		}
		string text = SanitizeSpriteName(resolvedSprite.name);
		if (text.IndexOf("_static_", StringComparison.Ordinal) < 0)
		{
			headFrameVariantCache[key] = null;
			return null;
		}
		string spriteName = $"{text}_{frame:D2}";
		Sprite sprite = null;
		if (LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(spriteName))
		{
			sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
		}
		headFrameVariantCache[key] = sprite;
		return sprite;
	}

	private static string SanitizeSpriteName(string spriteName)
	{
		if (string.IsNullOrEmpty(spriteName))
		{
			return string.Empty;
		}
		int num = spriteName.IndexOf("(Clone)", StringComparison.Ordinal);
		if (num >= 0)
		{
			spriteName = spriteName.Substring(0, num);
		}
		return spriteName.Trim();
	}

	private void DetectCustomHeadFrames()
	{
		HasCustomHeadFrames = false;
		if (!(skin is SkinPresetGK2 skinPresetGK))
		{
			return;
		}
		int id = skinPresetGK.head.id;
		if (id > 0)
		{
			if (customHeadFramesAvailabilityCache.TryGetValue(id, out var value))
			{
				HasCustomHeadFrames = value;
				return;
			}
			string text = id.ToString("D4");
			bool flag = LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(text + "_hed_static_down_02") || LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(text + "_hed_static_down_03");
			customHeadFramesAvailabilityCache[id] = flag;
			HasCustomHeadFrames = flag;
		}
	}

	private void DetectCustomBeardFrames()
	{
		hasCustomBeardFrames = false;
		if (!(skin is SkinPresetGK2 skinPresetGK))
		{
			return;
		}
		int id = skinPresetGK.beard.id;
		if (id > 0)
		{
			if (customBeardFramesAvailabilityCache.TryGetValue(id, out var value))
			{
				hasCustomBeardFrames = value;
				return;
			}
			string text = id.ToString("D4");
			bool value2 = LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(text + "_brd_static_down_02") || LazySingletonSO<EasySpritesCollection>.Instance.HasSprite(text + "_brd_static_down_03");
			customBeardFramesAvailabilityCache[id] = value2;
			hasCustomBeardFrames = value2;
		}
	}

	private bool TryGetAnimationSpriteId(SpriteRenderer spriteRenderer, out int id)
	{
		id = 0;
		if (spriteRenderer.sprite == null)
		{
			return false;
		}
		id = spriteRenderer.sprite.GetInstanceID();
		if (!validSpriteHash.TryGetValue(id, out var value))
		{
			value = IsValidSprite(spriteRenderer);
			validSpriteHash.Add(id, value);
		}
		return value;
	}

	private Sprite ResolveSkinnedSprite(SpriteRenderer spriteRenderer, int sourceSpriteId)
	{
		if (skinnedSpriteTopLevelHash.TryGetValue(sourceSpriteId, out var value))
		{
			return value;
		}
		string name = spriteRenderer.sprite.name;
		char c = name[5];
		char c2 = name[6];
		char c3 = name[7];
		int num = skin.DefineSkinIdFor(c, c2, c3);
		value = ((num == 0 || string.IsNullOrEmpty(name)) ? null : ((num != -1) ? GetSkinnedSpriteFromName(name, c, c2, c3, num) : spriteRenderer.sprite));
		skinnedSpriteTopLevelHash.Add(sourceSpriteId, value);
		return value;
	}

	private Sprite GetSkinnedSpriteFromName(string originalSpriteName, char char5, char char6, char char7, int newSkinId)
	{
		GarbagelessStrings.StringToChars(ref originalSpriteName, ref chars);
		GarbagelessStrings.IntToCharsWithLeadingZeros(newSkinId, ref chars, 4, 0);
		int hashCode = GarbagelessStrings.GetHashCode(ref chars);
		if (spriteHash.TryGetValue(hashCode, out var value))
		{
			return value;
		}
		string spriteName = GarbagelessStrings.CharsToString(ref chars);
		value = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
		if (value == null && fallbackSkin != null)
		{
			int num = fallbackSkin.DefineSkinIdFor(char5, char6, char7);
			if (num != 0)
			{
				GarbagelessStrings.IntToCharsWithLeadingZeros(num, ref chars, 4, 0);
				hashCode = GarbagelessStrings.GetHashCode(ref chars);
				if (!spriteHash.TryGetValue(hashCode, out value))
				{
					spriteName = GarbagelessStrings.CharsToString(ref chars);
					value = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
				}
			}
		}
		CacheSprite(hashCode, value);
		return value;
	}

	private static void CacheSprite(int hash, Sprite sprite)
	{
		if (spriteHash.Count >= 4096)
		{
			spriteHash.Clear();
		}
		spriteHash[hash] = sprite;
	}

	private static void ApplyResolvedSprite(SpriteRenderer spriteRenderer, Sprite sprite)
	{
		if (spriteRenderer.sprite != sprite)
		{
			spriteRenderer.sprite = sprite;
		}
		bool flag = sprite != null;
		if (spriteRenderer.enabled != flag)
		{
			spriteRenderer.enabled = flag;
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

	private void InitSkinChanger(GameObject gameObject, bool applyShader = false)
	{
		this.gameObject = gameObject;
		for (int num = sprites.Count - 1; num >= 0; num--)
		{
			if (sprites[num].gameObject.name.StartsWith("-"))
			{
				sprites.RemoveAt(num);
			}
		}
		headSpriteRenderer = FindMarkedRenderer<HeadSpriteComponentMarker>();
		beardSpriteRenderer = FindMarkedRenderer<BeardSpriteComponentMarker>();
		if (applyShader)
		{
			ApplyShaderToAllSprites(Shader.Find("Sprites/ColorReplace"));
		}
	}

	private SpriteRenderer FindMarkedRenderer<T>() where T : Component
	{
		for (int i = 0; i < sprites.Count; i++)
		{
			if (sprites[i] != null && sprites[i].GetComponent<T>() != null)
			{
				return sprites[i];
			}
		}
		T componentInChildren = gameObject.GetComponentInChildren<T>(includeInactive: true);
		if (!(componentInChildren != null))
		{
			return null;
		}
		return componentInChildren.GetComponent<SpriteRenderer>();
	}

	private bool IsValidSprite(SpriteRenderer spriteRenderer)
	{
		if (spriteRenderer.sprite == null)
		{
			return false;
		}
		string name = spriteRenderer.sprite.name;
		if (name.Length <= 8)
		{
			return false;
		}
		for (int i = 0; i < 4; i++)
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
