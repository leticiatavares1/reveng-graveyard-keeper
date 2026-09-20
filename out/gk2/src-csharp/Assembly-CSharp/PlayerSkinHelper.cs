using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class PlayerSkinHelper
{
	private static SkinPresetGK2 currentPreset;

	private static SkinPresetGK2 armorPreset;

	private static SkinPresetGK2 armorNoHelmetPreset;

	private static SkinPresetGK2 defaultPreset;

	public static readonly List<PlayerCustomizationPartData> DefaultCustomizationParts = new List<PlayerCustomizationPartData>
	{
		new PlayerCustomizationPartData("bdy_9003", CustomizablePartType.Body),
		new PlayerCustomizationPartData("arm_9003", CustomizablePartType.Arms),
		new PlayerCustomizationPartData("hrs_9002", CustomizablePartType.Hair),
		new PlayerCustomizationPartData("brd_9002", CustomizablePartType.Beard)
	};

	public static readonly PlayerCustomizationData playerStandardCustomizationData = PlayerCustomizationData.CreateDefault();

	public static SkinPresetGK2 CurrentPreset => currentPreset;

	private static SkinPresetGK2 DefaultPreset
	{
		get
		{
			if (defaultPreset == null)
			{
				defaultPreset = SkinPresetGK2.LoadAsset("9003_main_character");
			}
			return defaultPreset;
		}
	}

	public static SkinPresetGK2 ArmorPreset
	{
		get
		{
			if (armorPreset == null)
			{
				armorPreset = SkinPresetGK2.LoadAsset("9019_main_character_armor");
			}
			return armorPreset;
		}
	}

	public static PlayerColorCustomizationData CharacterCustomizationData => MainGame.PlayerController.CharacterCustomizationData;

	public static void ApplySkin(PlayerCustomizationData customization, bool onlyForCustomizationCharacter)
	{
		currentPreset = GetPresetForCustomizationData(customization);
		MainGame.PlayerController.View.SetPlayerPreset(currentPreset, onlyForCustomizationCharacter);
	}

	public static void ApplyPlayerColorsByData(PlayerCustomizationData customizationData, bool onlyForCustomizationCharacter)
	{
		ApplyPlayerColors(GetColorReplacementPalette(customizationData), CharacterCustomizationData.affectedPartTypes, onlyForCustomizationCharacter);
	}

	public static void ApplyPlayerColors(ColorReplacePalette palette, List<CustomizablePartType> affectedPartTypes, bool onlyForCustomizationCharacter)
	{
		MainGame.PlayerController.View.ApplyPlayerColors(palette.palette, affectedPartTypes, onlyForCustomizationCharacter);
	}

	public static ColorReplacePalette GetColorReplacementPalette(PlayerCustomizationData customization)
	{
		List<Texture2D> list = new List<Texture2D>();
		for (int i = 0; i < CharacterCustomizationData.customizationElements.Count; i++)
		{
			PlayerColorCustomizationType playerColorCustomizationType = CharacterCustomizationData.customizationElements[i].playerColorCustomizationType;
			int skinPresetPartId = CurrentPreset.GetSkinPresetPartId(playerColorCustomizationType);
			list.Add(CharacterCustomizationData.customizationElements[i].GetPaletteByIndex(GetSavedColorPaletteIndex(playerColorCustomizationType, customization), skinPresetPartId));
		}
		return PaletteReplaceHelper.CombinePalettes(list, CharacterCustomizationData.sourcePalette);
	}

	public static void ApplyArmorColorsByIndex(int index, bool includeHead = true)
	{
		ColorReplacePalette colorReplacePalette = CharacterCustomizationData.armorPresetData.GetColorReplacePalette(index);
		List<CustomizablePartType> list = CharacterCustomizationData.armorPresetData.affectedPartTypes;
		if (!includeHead)
		{
			list = list.FindAll((CustomizablePartType type) => type == CustomizablePartType.Body || type == CustomizablePartType.Arms);
		}
		MainGame.PlayerController.View.PlayerAnimation.ApplyPlayerColors(colorReplacePalette.palette, list, includeHead ? ArmorPreset : GetArmorNoHelmetPreset());
	}

	public static SkinPresetGK2 GetArmorNoHelmetPreset()
	{
		if (armorNoHelmetPreset == null)
		{
			armorNoHelmetPreset = ScriptableObject.CreateInstance<SkinPresetGK2>();
		}
		SkinPresetGK2 skinPresetGK = ((currentPreset != null) ? currentPreset : DefaultPreset);
		armorNoHelmetPreset.isPlayerPreset = true;
		armorNoHelmetPreset.body = CopyPart(ArmorPreset.body);
		armorNoHelmetPreset.arms = CopyPart(ArmorPreset.arms);
		armorNoHelmetPreset.head = CopyPart(skinPresetGK.head);
		armorNoHelmetPreset.hairstyle = CopyPart(skinPresetGK.hairstyle);
		armorNoHelmetPreset.beard = CopyPart(skinPresetGK.beard);
		return armorNoHelmetPreset;
	}

	private static SkinPresetPartGK2 CopyPart(SkinPresetPartGK2 source)
	{
		if (source == null)
		{
			return new SkinPresetPartGK2();
		}
		return new SkinPresetPartGK2
		{
			id = source.id,
			color = source.color,
			hue = source.hue,
			saturation = source.saturation,
			velocity = source.velocity,
			palette = source.palette,
			colorReplaceType = source.colorReplaceType
		};
	}

	private static int GetSavedColorPaletteIndex(PlayerColorCustomizationType type, PlayerCustomizationData data)
	{
		return data.GetColorCustomizationIndexByType(type);
	}

	public static SkinPresetGK2 GetPresetForCustomizationData(PlayerCustomizationData customization)
	{
		SkinPresetGK2 skinPresetGK = ScriptableObject.CreateInstance<SkinPresetGK2>();
		SkinPresetPartGK2 skinPresetPartGK = LoadSkinPresetPart(customization.GetCustomizationPartId(CustomizablePartType.Body));
		SkinPresetPartGK2 skinPresetPartGK2 = LoadSkinPresetPart(customization.GetCustomizationPartId(CustomizablePartType.Arms));
		SkinPresetPartGK2 skinPresetPartGK3 = LoadSkinPresetPart(customization.GetCustomizationPartId(CustomizablePartType.Hair));
		SkinPresetPartGK2 skinPresetPartGK4 = LoadSkinPresetPart(customization.GetCustomizationPartId(CustomizablePartType.Beard));
		skinPresetGK.head = DefaultPreset.head;
		skinPresetGK.isPlayerPreset = true;
		skinPresetGK.body = new SkinPresetPartGK2
		{
			id = skinPresetPartGK.id,
			colorReplaceType = ColorReplaceType.USE_PALETTE_COLOR_REPLACE
		};
		skinPresetGK.hairstyle = new SkinPresetPartGK2
		{
			id = skinPresetPartGK3.id,
			colorReplaceType = ColorReplaceType.USE_PALETTE_COLOR_REPLACE
		};
		skinPresetGK.arms = new SkinPresetPartGK2
		{
			id = skinPresetPartGK2.id,
			colorReplaceType = ColorReplaceType.USE_PALETTE_COLOR_REPLACE
		};
		skinPresetGK.beard = new SkinPresetPartGK2
		{
			id = skinPresetPartGK4.id,
			colorReplaceType = ColorReplaceType.USE_PALETTE_COLOR_REPLACE
		};
		return skinPresetGK;
	}

	private static SkinPresetPartGK2 LoadSkinPresetPart(string id)
	{
		CustomizablePart customizablePart = LoadCustomizablePart(id);
		if (customizablePart == null)
		{
			Debug.LogError("Couldn't load skin preset part: " + id);
			return null;
		}
		return customizablePart.skinPart;
	}

	private static CustomizablePart LoadCustomizablePart(string id)
	{
		return Addressables.LoadAssetAsync<CustomizablePart>("Assets/AddressableAssets/PlayerSkinParts/" + id + ".asset").WaitForCompletion();
	}
}
