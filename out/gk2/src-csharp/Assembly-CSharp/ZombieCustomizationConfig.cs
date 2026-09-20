using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ZombieCustomizationConfig : LazySingletonSO<ZombieCustomizationConfig>
{
	[Serializable]
	private class ZombieRolledData
	{
		public string id;

		public List<Texture2D> bodyTextures;

		public List<Texture2D> headTextures;

		public List<int> bodyIds;

		public List<int> headIds;

		public Dictionary<string, Texture2D> bodyTexturesDict;

		public Dictionary<string, Texture2D> headTexturesDict;

		public void CreateCache()
		{
			bodyTexturesDict = new Dictionary<string, Texture2D>();
			for (int i = 0; i < bodyTextures.Count; i++)
			{
				bodyTexturesDict.Add(bodyTextures[i].name, bodyTextures[i]);
			}
			headTexturesDict = new Dictionary<string, Texture2D>();
			for (int j = 0; j < headTextures.Count; j++)
			{
				headTexturesDict.Add(headTextures[j].name, headTextures[j]);
			}
		}
	}

	[Serializable]
	public class ZombieFighterBodyPallete
	{
		public enum ColorType
		{
			Blue = 0,
			Red = 2,
			Green = 3,
			Violet = 4,
			Turquoise = 5
		}

		public ColorType colorType;

		public ArmorColorPalette palette;
	}

	[Serializable]
	public class ZombieFighterArmsArmorPallete
	{
		public enum Tier
		{
			Leather = 0,
			Rusty = 2,
			Metall = 3,
			Iron = 4,
			Elite = 5
		}

		public Tier tier;

		public ArmorColorPalette palette;
	}

	[SerializeField]
	private List<ZombieRolledData> zombieRolledDatas;

	private Dictionary<string, ZombieRolledData> zombieRolledDatasDict;

	[Space]
	[SerializeField]
	private List<ZombieFighterBodyPallete> fightersBodyPalettes;

	[SerializeField]
	private Texture2D sourceColorPalette;

	[Space]
	[SerializeField]
	private List<ZombieFighterArmsArmorPallete> fightersArmsArmorPalettes;

	[SerializeField]
	private Texture2D sourceArmorPalette;

	private bool isCacheCreated;

	private void TryCreateCache()
	{
		if (isCacheCreated)
		{
			return;
		}
		isCacheCreated = true;
		zombieRolledDatasDict = new Dictionary<string, ZombieRolledData>();
		foreach (ZombieRolledData zombieRolledData in zombieRolledDatas)
		{
			zombieRolledDatasDict.Add(zombieRolledData.id, zombieRolledData);
			zombieRolledData.CreateCache();
		}
	}

	public static Texture2D GetBodyTextureByName(string dataId, string textureName)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		return LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].bodyTexturesDict[textureName];
	}

	public static Texture2D GetHeadTextureByName(string dataId, string textureName)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		return LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].headTexturesDict[textureName];
	}

	public static int GetRandomBody(string dataId)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		return LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].bodyIds.GetRandom();
	}

	public static int GetRandomHead(string dataId)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		return LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].headIds.GetRandom();
	}

	public static Texture2D GetRandomBodyLut(string dataId)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		if (LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].bodyTextures.Count <= 0)
		{
			return null;
		}
		return LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].bodyTextures.GetRandom();
	}

	public static Texture2D GetRandomHeadLut(string dataId)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		if (LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].headTextures.Count <= 0)
		{
			return null;
		}
		return LazySingletonSO<ZombieCustomizationConfig>.Instance.zombieRolledDatasDict[dataId].headTextures.GetRandom();
	}

	public static ColorReplacePalette GetBodyReplacePalette(string flagVariationId)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		ZombieFighterBodyPallete.ColorType colorType = ZombieFighterBodyPallete.ColorType.Blue;
		switch (flagVariationId)
		{
		case "blue":
			colorType = ZombieFighterBodyPallete.ColorType.Blue;
			break;
		case "red":
			colorType = ZombieFighterBodyPallete.ColorType.Red;
			break;
		case "green":
			colorType = ZombieFighterBodyPallete.ColorType.Green;
			break;
		case "violet":
			colorType = ZombieFighterBodyPallete.ColorType.Violet;
			break;
		case "turquoise":
			colorType = ZombieFighterBodyPallete.ColorType.Turquoise;
			break;
		}
		ZombieFighterBodyPallete zombieFighterBodyPallete = LazySingletonSO<ZombieCustomizationConfig>.Instance.fightersBodyPalettes.Find((ZombieFighterBodyPallete x) => x.colorType == colorType);
		if (zombieFighterBodyPallete == null)
		{
			Debug.LogError($"ZombieFighterColorPallete for color [{colorType}] doesn't exist");
			return null;
		}
		return PaletteReplaceHelper.CombinePalettes(zombieFighterBodyPallete.palette.GetPalettes(), LazySingletonSO<ZombieCustomizationConfig>.Instance.sourceColorPalette);
	}

	public static ColorReplacePalette GetArmsArmorReplacePalette(string armorItemId)
	{
		LazySingletonSO<ZombieCustomizationConfig>.Instance.TryCreateCache();
		ZombieFighterArmsArmorPallete.Tier tier = ZombieFighterArmsArmorPallete.Tier.Leather;
		switch (armorItemId)
		{
		case "armor_0":
			tier = ZombieFighterArmsArmorPallete.Tier.Leather;
			break;
		case "armor_1":
			tier = ZombieFighterArmsArmorPallete.Tier.Rusty;
			break;
		case "armor_2":
			tier = ZombieFighterArmsArmorPallete.Tier.Metall;
			break;
		case "armor_3":
			tier = ZombieFighterArmsArmorPallete.Tier.Iron;
			break;
		case "armor_4":
			tier = ZombieFighterArmsArmorPallete.Tier.Elite;
			break;
		}
		ZombieFighterArmsArmorPallete zombieFighterArmsArmorPallete = LazySingletonSO<ZombieCustomizationConfig>.Instance.fightersArmsArmorPalettes.Find((ZombieFighterArmsArmorPallete x) => x.tier == tier);
		if (zombieFighterArmsArmorPallete == null)
		{
			Debug.LogError($"ZombieFighterArmorPallete for tier [{tier}] doesn't exist");
			return null;
		}
		return PaletteReplaceHelper.CombinePalettes(zombieFighterArmsArmorPallete.palette.GetPalettes(), LazySingletonSO<ZombieCustomizationConfig>.Instance.sourceArmorPalette);
	}
}
