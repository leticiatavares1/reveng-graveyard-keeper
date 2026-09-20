using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class PlayerCustomizationData
{
	public static Action<Item> OnCustomizationUnlocked;

	private static readonly PlayerColorCustomizationType[] BodyColorCustomizationTypes = new PlayerColorCustomizationType[3]
	{
		PlayerColorCustomizationType.Bdy1,
		PlayerColorCustomizationType.Bdy2,
		PlayerColorCustomizationType.Bdy3
	};

	public List<PlayerCustomizationPartData> customizationPartsData = new List<PlayerCustomizationPartData>();

	public List<PlayerColorCustomizationPairData> colorCustomizationPairData = new List<PlayerColorCustomizationPairData>();

	public List<UnlockedCustomizationPartData> unlockedCustomizationPartsData = new List<UnlockedCustomizationPartData>();

	public List<UnlockedColorCustomizationData> unlockedColorCustomizationData = new List<UnlockedColorCustomizationData>();

	public static PlayerCustomizationData CreateDefault()
	{
		PlayerCustomizationData playerCustomizationData = new PlayerCustomizationData();
		foreach (PlayerCustomizationPartData defaultCustomizationPart in PlayerSkinHelper.DefaultCustomizationParts)
		{
			playerCustomizationData.customizationPartsData.Add(new PlayerCustomizationPartData(defaultCustomizationPart.id, defaultCustomizationPart.type));
			playerCustomizationData.AddUnlockedPartId(defaultCustomizationPart.id, defaultCustomizationPart.type);
		}
		foreach (PlayerColorCustomizationType value in Enum.GetValues(typeof(PlayerColorCustomizationType)))
		{
			playerCustomizationData.SetColorCustomizationIndexForType(value, 0);
			int partSkinIdForColorCustomizationType = playerCustomizationData.GetPartSkinIdForColorCustomizationType(value);
			string colorPaletteName = GetColorPaletteName(value, partSkinIdForColorCustomizationType, 0, PlayerSkinHelper.CharacterCustomizationData);
			playerCustomizationData.AddUnlockedColorName(value, partSkinIdForColorCustomizationType, colorPaletteName);
		}
		playerCustomizationData.UnlockHairAndBeardCustomization(PlayerSkinHelper.CharacterCustomizationData);
		return playerCustomizationData;
	}

	public static PlayerCustomizationData Copy(PlayerCustomizationData source)
	{
		PlayerCustomizationData playerCustomizationData = new PlayerCustomizationData();
		for (int i = 0; i < source.customizationPartsData.Count; i++)
		{
			playerCustomizationData.customizationPartsData.Add(new PlayerCustomizationPartData
			{
				id = source.customizationPartsData[i].id,
				type = source.customizationPartsData[i].type
			});
		}
		for (int j = 0; j < source.colorCustomizationPairData.Count; j++)
		{
			playerCustomizationData.colorCustomizationPairData.Add(new PlayerColorCustomizationPairData
			{
				index = source.colorCustomizationPairData[j].index,
				type = source.colorCustomizationPairData[j].type
			});
		}
		for (int k = 0; k < source.unlockedCustomizationPartsData.Count; k++)
		{
			playerCustomizationData.unlockedCustomizationPartsData.Add(new UnlockedCustomizationPartData(source.unlockedCustomizationPartsData[k].type, new List<string>(source.unlockedCustomizationPartsData[k].unlockedIds)));
		}
		for (int l = 0; l < source.unlockedColorCustomizationData.Count; l++)
		{
			UnlockedColorCustomizationData unlockedColorCustomizationData = new UnlockedColorCustomizationData(source.unlockedColorCustomizationData[l].type, source.unlockedColorCustomizationData[l].partSkinId, new List<string>(source.unlockedColorCustomizationData[l].unlockedNames));
			unlockedColorCustomizationData.unlockedIndices = new List<int>(source.unlockedColorCustomizationData[l].unlockedIndices);
			playerCustomizationData.unlockedColorCustomizationData.Add(unlockedColorCustomizationData);
		}
		return playerCustomizationData;
	}

	public bool UnlockCustomizationPart(string id, CustomizablePartType type)
	{
		if (string.IsNullOrEmpty(id))
		{
			return false;
		}
		if (!AddUnlockedPartId(id, type))
		{
			return false;
		}
		if (type == CustomizablePartType.Body)
		{
			string id2 = id.Replace("bdy_", "arm_");
			AddUnlockedPartId(id2, CustomizablePartType.Arms);
		}
		return true;
	}

	public bool UnlockCustomizationColor(PlayerColorCustomizationType type, string paletteName, int partSkinId)
	{
		return UnlockCustomizationColor(type, paletteName, partSkinId, PlayerSkinHelper.CharacterCustomizationData);
	}

	public bool UnlockCustomizationColor(PlayerColorCustomizationType type, string paletteName, int partSkinId, PlayerColorCustomizationData colorCustomizationData)
	{
		if (!IsValidColorPaletteName(type, partSkinId, paletteName, colorCustomizationData))
		{
			return false;
		}
		return AddUnlockedColorName(type, partSkinId, paletteName);
	}

	public void UnlockAllCustomization(PlayerColorCustomizationData colorCustomizationData)
	{
		IList<CustomizablePart> list = Addressables.LoadAssetsAsync<CustomizablePart>("player_skins").WaitForCompletion();
		for (int i = 0; i < list.Count; i++)
		{
			UnlockCustomizationPart(list[i].name, list[i].type);
		}
		for (int j = 0; j < colorCustomizationData.customizationElements.Count; j++)
		{
			PlayerColorCustomizationElementData playerColorCustomizationElementData = colorCustomizationData.customizationElements[j];
			for (int k = 0; k < playerColorCustomizationElementData.SkinElements.Count; k++)
			{
				PlayerColorCustomizationSkinElementData playerColorCustomizationSkinElementData = playerColorCustomizationElementData.SkinElements[k];
				for (int l = 0; l < playerColorCustomizationSkinElementData.palettes.Count; l++)
				{
					Texture2D texture2D = playerColorCustomizationSkinElementData.palettes[l];
					if (texture2D != null)
					{
						UnlockCustomizationColor(playerColorCustomizationElementData.playerColorCustomizationType, texture2D.name, playerColorCustomizationSkinElementData.skinId, colorCustomizationData);
					}
				}
			}
		}
	}

	public void UnlockHairAndBeardCustomization(PlayerColorCustomizationData colorCustomizationData)
	{
		IList<CustomizablePart> list = Addressables.LoadAssetsAsync<CustomizablePart>("player_skins").WaitForCompletion();
		List<int> list2 = new List<int>();
		for (int i = 0; i < list.Count; i++)
		{
			CustomizablePart customizablePart = list[i];
			if (!(customizablePart == null))
			{
				if (customizablePart.type == CustomizablePartType.Hair || customizablePart.type == CustomizablePartType.Beard)
				{
					UnlockCustomizationPart(customizablePart.name, customizablePart.type);
				}
				if (TryGetHeadColorSkinId(customizablePart.name, customizablePart.type, out var partSkinId) && !list2.Contains(partSkinId))
				{
					list2.Add(partSkinId);
				}
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			UnlockAllColorPalettes(PlayerColorCustomizationType.Hed, list2[j], colorCustomizationData);
		}
	}

	public bool SuperUnlockBodyCustomization(string id, PlayerColorCustomizationData colorCustomizationData, Item sourceItem = null)
	{
		if (!TryParsePartSkinId(id, "bdy_", out var partSkinId))
		{
			return false;
		}
		bool flag = UnlockCustomizationPart(id, CustomizablePartType.Body);
		for (int i = 0; i < BodyColorCustomizationTypes.Length; i++)
		{
			flag |= UnlockAllColorPalettes(BodyColorCustomizationTypes[i], partSkinId, colorCustomizationData);
		}
		if (sourceItem != null && flag)
		{
			OnCustomizationUnlocked?.Invoke(sourceItem);
		}
		return flag;
	}

	public List<string> GetUnlockedPartIds(CustomizablePartType type)
	{
		UnlockedCustomizationPartData unlockedCustomizationPartData = unlockedCustomizationPartsData.Find((UnlockedCustomizationPartData x) => x.type == type);
		if (unlockedCustomizationPartData == null)
		{
			return new List<string>();
		}
		return unlockedCustomizationPartData.unlockedIds;
	}

	public List<string> GetUnlockedColorNames(PlayerColorCustomizationType type, int partSkinId)
	{
		UnlockedColorCustomizationData unlockedColorCustomizationData = this.unlockedColorCustomizationData.Find((UnlockedColorCustomizationData x) => x.type == type && x.partSkinId == partSkinId);
		if (unlockedColorCustomizationData == null)
		{
			return new List<string>();
		}
		return unlockedColorCustomizationData.unlockedNames;
	}

	public List<int> GetUnlockedColorIndices(PlayerColorCustomizationType type, int partSkinId, PlayerColorCustomizationData colorCustomizationData)
	{
		List<int> list = new List<int>();
		if (colorCustomizationData == null)
		{
			return list;
		}
		PlayerColorCustomizationSkinElementData playerColorCustomizationSkinElementData = colorCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type)?.GetSkinElement(partSkinId);
		if (playerColorCustomizationSkinElementData == null)
		{
			return list;
		}
		UnlockedColorCustomizationData unlockedColorCustomizationData = this.unlockedColorCustomizationData.Find((UnlockedColorCustomizationData x) => x.type == type && x.partSkinId == partSkinId);
		List<string> list2 = ((unlockedColorCustomizationData != null) ? unlockedColorCustomizationData.unlockedNames : new List<string>());
		for (int i = 0; i < playerColorCustomizationSkinElementData.palettes.Count; i++)
		{
			Texture2D texture2D = playerColorCustomizationSkinElementData.palettes[i];
			if (texture2D != null && list2.Contains(texture2D.name))
			{
				list.Add(i);
			}
		}
		if (unlockedColorCustomizationData != null)
		{
			for (int j = 0; j < unlockedColorCustomizationData.unlockedIndices.Count; j++)
			{
				int num = unlockedColorCustomizationData.unlockedIndices[j];
				if (num >= 0 && num < playerColorCustomizationSkinElementData.palettes.Count && !list.Contains(num))
				{
					list.Add(num);
				}
			}
			list.Sort();
		}
		return list;
	}

	public string GetCustomizationPartId(CustomizablePartType type)
	{
		PlayerCustomizationPartData playerCustomizationPartData = customizationPartsData.Find((PlayerCustomizationPartData x) => x.type == type);
		if (playerCustomizationPartData != null)
		{
			return playerCustomizationPartData.id;
		}
		Debug.LogError($"Cannot find skin part in player data with type[{type}]");
		return string.Empty;
	}

	public int GetColorCustomizationIndexByType(PlayerColorCustomizationType type)
	{
		return colorCustomizationPairData.Find((PlayerColorCustomizationPairData d) => d.type == type)?.index ?? 0;
	}

	public void SetColorCustomizationIndexForType(PlayerColorCustomizationType type, int index)
	{
		PlayerColorCustomizationPairData playerColorCustomizationPairData = colorCustomizationPairData.Find((PlayerColorCustomizationPairData d) => d.type == type);
		if (playerColorCustomizationPairData != null)
		{
			playerColorCustomizationPairData.index = index;
			return;
		}
		playerColorCustomizationPairData = new PlayerColorCustomizationPairData(index, type);
		colorCustomizationPairData.Add(playerColorCustomizationPairData);
	}

	public int GetPartSkinIdForColorCustomizationType(PlayerColorCustomizationType type)
	{
		CustomizablePartType customizationPartTypeForColorCustomizationType = GetCustomizationPartTypeForColorCustomizationType(type);
		string customizationPartId = GetCustomizationPartId(customizationPartTypeForColorCustomizationType);
		int num = customizationPartId.LastIndexOf('_');
		if (num >= 0 && int.TryParse(customizationPartId.Substring(num + 1), out var result))
		{
			return result;
		}
		Debug.LogError($"Cannot parse skin id from customization part id:[{customizationPartId}] for color type:[{type}]");
		return -1;
	}

	private bool AddUnlockedPartId(string id, CustomizablePartType type)
	{
		UnlockedCustomizationPartData unlockedCustomizationPartData = unlockedCustomizationPartsData.Find((UnlockedCustomizationPartData x) => x.type == type);
		if (unlockedCustomizationPartData == null)
		{
			unlockedCustomizationPartData = new UnlockedCustomizationPartData(type, new List<string>());
			unlockedCustomizationPartsData.Add(unlockedCustomizationPartData);
		}
		if (unlockedCustomizationPartData.unlockedIds.Contains(id))
		{
			return false;
		}
		unlockedCustomizationPartData.unlockedIds.Add(id);
		return true;
	}

	private bool AddUnlockedColorName(PlayerColorCustomizationType type, int partSkinId, string paletteName)
	{
		if (string.IsNullOrEmpty(paletteName))
		{
			return false;
		}
		UnlockedColorCustomizationData unlockedColorCustomizationData = this.unlockedColorCustomizationData.Find((UnlockedColorCustomizationData x) => x.type == type && x.partSkinId == partSkinId);
		if (unlockedColorCustomizationData == null)
		{
			unlockedColorCustomizationData = new UnlockedColorCustomizationData(type, partSkinId, new List<string>());
			this.unlockedColorCustomizationData.Add(unlockedColorCustomizationData);
		}
		if (unlockedColorCustomizationData.unlockedNames.Contains(paletteName))
		{
			return false;
		}
		unlockedColorCustomizationData.unlockedNames.Add(paletteName);
		unlockedColorCustomizationData.unlockedNames.Sort();
		return true;
	}

	private bool UnlockAllColorPalettes(PlayerColorCustomizationType type, int partSkinId, PlayerColorCustomizationData colorCustomizationData)
	{
		if (partSkinId < 0 || colorCustomizationData == null)
		{
			return false;
		}
		PlayerColorCustomizationSkinElementData playerColorCustomizationSkinElementData = colorCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type)?.GetSkinElement(partSkinId);
		if (playerColorCustomizationSkinElementData == null)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < playerColorCustomizationSkinElementData.palettes.Count; i++)
		{
			Texture2D texture2D = playerColorCustomizationSkinElementData.palettes[i];
			if (texture2D != null)
			{
				flag |= AddUnlockedColorName(type, partSkinId, texture2D.name);
			}
		}
		return flag;
	}

	private static bool IsValidColorPaletteName(PlayerColorCustomizationType type, int partSkinId, string paletteName, PlayerColorCustomizationData colorCustomizationData)
	{
		if (partSkinId < 0 || string.IsNullOrEmpty(paletteName) || colorCustomizationData == null)
		{
			return false;
		}
		PlayerColorCustomizationElementData playerColorCustomizationElementData = colorCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type);
		if (playerColorCustomizationElementData == null)
		{
			Debug.LogError($"Cannot find color customization element for type[{type}]");
			return false;
		}
		return playerColorCustomizationElementData.GetSkinElement(partSkinId)?.palettes.Exists((Texture2D palette) => palette != null && palette.name == paletteName) ?? false;
	}

	private static string GetColorPaletteName(PlayerColorCustomizationType type, int partSkinId, int index, PlayerColorCustomizationData colorCustomizationData)
	{
		if (partSkinId < 0 || index < 0 || colorCustomizationData == null)
		{
			return string.Empty;
		}
		PlayerColorCustomizationSkinElementData playerColorCustomizationSkinElementData = colorCustomizationData.customizationElements.Find((PlayerColorCustomizationElementData e) => e.playerColorCustomizationType == type)?.GetSkinElement(partSkinId);
		if (playerColorCustomizationSkinElementData == null || index >= playerColorCustomizationSkinElementData.palettes.Count || playerColorCustomizationSkinElementData.palettes[index] == null)
		{
			return string.Empty;
		}
		return playerColorCustomizationSkinElementData.palettes[index].name;
	}

	private static bool TryParsePartSkinId(string id, string prefix, out int partSkinId)
	{
		partSkinId = -1;
		if (!string.IsNullOrEmpty(id) && id.StartsWith(prefix, StringComparison.Ordinal))
		{
			return int.TryParse(id.Substring(prefix.Length), out partSkinId);
		}
		return false;
	}

	private static bool TryGetHeadColorSkinId(string id, CustomizablePartType type, out int partSkinId)
	{
		switch (type)
		{
		case CustomizablePartType.Hair:
			return TryParsePartSkinId(id, "hrs_", out partSkinId);
		case CustomizablePartType.Beard:
			return TryParsePartSkinId(id, "brd_", out partSkinId);
		default:
			partSkinId = -1;
			return false;
		}
	}

	private static CustomizablePartType GetCustomizationPartTypeForColorCustomizationType(PlayerColorCustomizationType type)
	{
		switch (type)
		{
		case PlayerColorCustomizationType.Hed:
			return CustomizablePartType.Hair;
		case PlayerColorCustomizationType.Bdy1:
		case PlayerColorCustomizationType.Bdy2:
		case PlayerColorCustomizationType.Bdy3:
			return CustomizablePartType.Body;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}
}
