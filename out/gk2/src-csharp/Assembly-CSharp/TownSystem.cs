using System;
using UnityEngine;

[Serializable]
public class TownSystem
{
	public const string CHALK_BOARD_ID = "town_chalk_board";

	[SerializeField]
	private int quality;

	public static int moneyForPalettes;

	public int Quality
	{
		get
		{
			return quality;
		}
		set
		{
			bool num = quality != value;
			quality = value;
			if (num)
			{
				TownSystem.OnQualityChanged?.Invoke();
			}
			if (GUIElements.Instance != null)
			{
				GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
			}
		}
	}

	public static event Action OnClearTownPalettes;

	public static event Action OnQualityChanged;

	public void UpdateSystemAtTheEndOfDay(int day)
	{
	}

	public void ResetVendors()
	{
		for (int i = 0; i < MainGame.Instance.GameSave.vendorSystem.vendors.Count; i++)
		{
			Vendor vendor = MainGame.Instance.GameSave.vendorSystem.vendors[i];
			if (vendor.Definition.townVendor)
			{
				vendor.UsedHappinessThisWeek = 0f;
				vendor.SoldItemsWithHappinessThisWeek.Clear();
			}
		}
		Debug.Log("#economy# ResetVendors");
	}

	public void ClearTownPalettes()
	{
		moneyForPalettes = 0;
		MainGame.Instance.GameSave.vendorSystem.TryResolveOrders();
		TownSystem.OnClearTownPalettes?.Invoke();
		if (moneyForPalettes > 0)
		{
			MainGame.Instance.GameSave.worldData.GetWgoData("town_chalk_board").StorePaletteTradingResult(moneyForPalettes);
		}
		Debug.Log($"#economy# ClearTownPalettes moneyForPalettes:[{moneyForPalettes}]");
	}

	public void StartTownBuildingCraftOnWgoFromScript(TownBuildingDef def, WgoData wgoData)
	{
		if (def == null)
		{
			Debug.LogError("Trying StartTownBuildingCraftOnWgoFromScript with null def!!!");
			return;
		}
		if (wgoData == null)
		{
			Debug.LogError("Trying StartTownBuildingCraftOnWgoFromScript with null wgoData!!!");
			return;
		}
		Debug.Log("StartTownBuildingCraftOnWgoFromScript def:[" + def.id + "] wgoData:[" + wgoData.id + "]");
		string craftId = "town_building_craft:" + def.id;
		CraftElementBase craftElementBase = new CraftElement(craftId, 1, new CraftParamsData(craftId, wgoData));
		if (wgoData.CraftComponent.TryStartCraft(craftElementBase))
		{
			wgoData.CraftComponent.LastStartedCraftWithRequirements = craftElementBase;
		}
	}

	public void CreateTownBuildingOnWgo(WgoData wgoData)
	{
		TownBuildingWgoComponent townBuildingWgoComponent = wgoData.TownBuildingWgoComponent;
		townBuildingWgoComponent.TownBuildingId = wgoData.CraftComponent.LastStartedCraftWithRequirements.CraftId.Replace("town_building_craft:", "");
		TownBuildingDef data = GameBalance.Me.GetData<TownBuildingDef>(townBuildingWgoComponent.TownBuildingId);
		int tierIndex = townBuildingWgoComponent.TierIndex;
		string customTag = wgoData.CustomTag;
		string oldValue = ((wgoData.Definition.interactionType == WGODef.InteractionType.TownBuildingPlace) ? "t_b_signboard_" : "t_b_character_");
		if (townBuildingWgoComponent.SceneConfiguration.tierDataList.Count > 0)
		{
			TownBuildingTierSceneConfiguration townBuildingTierSceneConfiguration = null;
			if (tierIndex > 0)
			{
				townBuildingTierSceneConfiguration = townBuildingWgoComponent.SceneConfiguration.tierDataList[tierIndex - 1];
			}
			TownBuildingTierSceneConfiguration townBuildingTierSceneConfiguration2 = townBuildingWgoComponent.SceneConfiguration.tierDataList[tierIndex];
			if (!string.IsNullOrEmpty(townBuildingTierSceneConfiguration2.yard.wgoId))
			{
				WgoData wgoData2 = new WgoData(townBuildingTierSceneConfiguration2.yard.wgoId, townBuildingTierSceneConfiguration2.yard.position, wgoData.WorldId);
				wgoData2.Scale = ((townBuildingTierSceneConfiguration2.yard.scale == Vector3.zero) ? Vector3.one : townBuildingTierSceneConfiguration2.yard.scale);
				wgoData2.CustomTag = customTag.Replace(oldValue, "t_b_yard_");
				wgoData2.MainWgoPartData.variationId = data.variationId;
				wgoData2.MainWgoPartData.rotationIndex = -1;
				MainGame.WorldData.AddWgoData(wgoData2);
				townBuildingTierSceneConfiguration2.yard.createdWgoUniqueId = wgoData2.UniqueId;
			}
			if (!string.IsNullOrEmpty(townBuildingTierSceneConfiguration2.decor1.wgoId))
			{
				WgoData wgoData3 = new WgoData(townBuildingTierSceneConfiguration2.decor1.wgoId, townBuildingTierSceneConfiguration2.decor1.position, wgoData.WorldId);
				wgoData3.CustomTag = customTag.Replace(oldValue, "t_b_decor_1_");
				wgoData3.Scale = ((townBuildingTierSceneConfiguration2.decor1.scale == Vector3.zero) ? Vector3.one : townBuildingTierSceneConfiguration2.decor1.scale);
				wgoData3.MainWgoPartData.variationId = data.variationId;
				wgoData3.MainWgoPartData.rotationIndex = -1;
				MainGame.WorldData.AddWgoData(wgoData3);
				townBuildingTierSceneConfiguration2.decor1.createdWgoUniqueId = wgoData3.UniqueId;
			}
			if (!string.IsNullOrEmpty(townBuildingTierSceneConfiguration2.decor2.wgoId))
			{
				WgoData wgoData4 = new WgoData(townBuildingTierSceneConfiguration2.decor2.wgoId, townBuildingTierSceneConfiguration2.decor2.position, wgoData.WorldId);
				wgoData4.Scale = ((townBuildingTierSceneConfiguration2.decor2.scale == Vector3.zero) ? Vector3.one : townBuildingTierSceneConfiguration2.decor2.scale);
				wgoData4.CustomTag = customTag.Replace(oldValue, "t_b_decor_2_");
				wgoData4.MainWgoPartData.variationId = data.variationId;
				wgoData4.MainWgoPartData.rotationIndex = -1;
				MainGame.WorldData.AddWgoData(wgoData4);
				townBuildingTierSceneConfiguration2.decor2.createdWgoUniqueId = wgoData4.UniqueId;
			}
			if (!string.IsNullOrEmpty(townBuildingTierSceneConfiguration2.decor3.wgoId))
			{
				WgoData wgoData5 = new WgoData(townBuildingTierSceneConfiguration2.decor3.wgoId, townBuildingTierSceneConfiguration2.decor3.position, wgoData.WorldId);
				wgoData5.CustomTag = customTag.Replace(oldValue, "t_b_decor_3_");
				wgoData5.Scale = ((townBuildingTierSceneConfiguration2.decor3.scale == Vector3.zero) ? Vector3.one : townBuildingTierSceneConfiguration2.decor3.scale);
				wgoData5.MainWgoPartData.variationId = data.variationId;
				wgoData5.MainWgoPartData.rotationIndex = -1;
				MainGame.WorldData.AddWgoData(wgoData5);
				townBuildingTierSceneConfiguration2.decor3.createdWgoUniqueId = wgoData5.UniqueId;
			}
			if (!string.IsNullOrEmpty(townBuildingTierSceneConfiguration2.sign.wgoId))
			{
				WgoData wgoData6 = new WgoData(townBuildingTierSceneConfiguration2.sign.wgoId, townBuildingTierSceneConfiguration2.sign.position, wgoData.WorldId);
				wgoData6.Scale = ((townBuildingTierSceneConfiguration2.sign.scale == Vector3.zero) ? Vector3.one : townBuildingTierSceneConfiguration2.sign.scale);
				wgoData6.CustomTag = customTag.Replace(oldValue, "t_b_sign_");
				wgoData6.MainWgoPartData.variationId = data.variationId;
				wgoData6.MainWgoPartData.rotationIndex = -1;
				MainGame.WorldData.AddWgoData(wgoData6);
				townBuildingTierSceneConfiguration2.sign.createdWgoUniqueId = wgoData6.UniqueId;
			}
			WgoData wgoData7 = new WgoData(townBuildingTierSceneConfiguration2.tent.wgoId, townBuildingTierSceneConfiguration2.tent.position, wgoData.WorldId);
			wgoData7.Scale = ((townBuildingTierSceneConfiguration2.tent.scale == Vector3.zero) ? Vector3.one : townBuildingTierSceneConfiguration2.tent.scale);
			wgoData7.CustomTag = customTag.Replace(oldValue, "t_b_tent_");
			wgoData7.TownBuildingWgoComponent = wgoData.TownBuildingWgoComponent;
			wgoData7.TownBuildingWgoComponent.TierIndex++;
			wgoData7.MainWgoPartData.variationId = data.variationId;
			wgoData7.MainWgoPartData.rotationIndex = -1;
			MainGame.WorldData.AddWgoData(wgoData7);
			townBuildingTierSceneConfiguration2.tent.createdWgoUniqueId = wgoData7.UniqueId;
			if (townBuildingTierSceneConfiguration != null)
			{
				WgoData wgoData8 = MainGame.WorldData.GetWgoData(townBuildingTierSceneConfiguration.tent.createdWgoUniqueId);
				if (!wgoData8.LinkedFromTownBuildingUniqueId.IsEmpty)
				{
					WgoData wgoData9 = MainGame.WorldData.GetWgoData(wgoData8.LinkedFromTownBuildingUniqueId);
					wgoData9.LinkedToTownBuildingUniqueId = wgoData7.UniqueId;
					wgoData7.LinkedFromTownBuildingUniqueId = wgoData8.LinkedFromTownBuildingUniqueId;
					wgoData7.SetGameRes("available_by_time", wgoData9.GetGameResInt("available_by_time"));
				}
				if (!townBuildingTierSceneConfiguration.yard.createdWgoUniqueId.IsEmpty)
				{
					MainGame.WorldData.RemoveWgoDataFromGameScene(townBuildingTierSceneConfiguration.yard.createdWgoUniqueId);
				}
				if (!townBuildingTierSceneConfiguration.decor1.createdWgoUniqueId.IsEmpty)
				{
					MainGame.WorldData.RemoveWgoDataFromGameScene(townBuildingTierSceneConfiguration.decor1.createdWgoUniqueId);
				}
				if (!townBuildingTierSceneConfiguration.decor2.createdWgoUniqueId.IsEmpty)
				{
					MainGame.WorldData.RemoveWgoDataFromGameScene(townBuildingTierSceneConfiguration.decor2.createdWgoUniqueId);
				}
				if (!townBuildingTierSceneConfiguration.decor3.createdWgoUniqueId.IsEmpty)
				{
					MainGame.WorldData.RemoveWgoDataFromGameScene(townBuildingTierSceneConfiguration.decor3.createdWgoUniqueId);
				}
				if (!townBuildingTierSceneConfiguration.sign.createdWgoUniqueId.IsEmpty)
				{
					MainGame.WorldData.RemoveWgoDataFromGameScene(townBuildingTierSceneConfiguration.sign.createdWgoUniqueId);
				}
				MainGame.WorldData.RemoveWgoDataFromGameScene(townBuildingTierSceneConfiguration.tent.createdWgoUniqueId);
				foreach (LazyExpression item in data.expressionOnCharCreate)
				{
					item.EvaluateBool(wgoData);
				}
			}
			else
			{
				WgoData wgoData10 = new WgoData(data.characterId, wgoData.Position, wgoData.WorldId);
				wgoData10.CustomTag = customTag.Replace(oldValue, "t_b_character_");
				wgoData10.LinkedToTownBuildingUniqueId = wgoData7.UniqueId;
				wgoData7.LinkedFromTownBuildingUniqueId = wgoData10.UniqueId;
				MainGame.WorldData.AddWgoData(wgoData10);
				foreach (LazyExpression item2 in data.expressionOnCharCreate)
				{
					item2.EvaluateBool(wgoData10);
				}
				MainGame.WorldData.RemoveWgoDataFromGameScene(wgoData.UniqueId);
			}
		}
		else
		{
			WgoData wgoData11 = new WgoData(data.id, wgoData.Position, wgoData.WorldId);
			wgoData11.CustomTag = customTag.Replace(oldValue, "t_b_tent_");
			wgoData11.TownBuildingWgoComponent = wgoData.TownBuildingWgoComponent;
			wgoData11.TownBuildingWgoComponent.TierIndex++;
			wgoData11.MainWgoPartData.variationId = data.variationId;
			wgoData11.MainWgoPartData.rotationIndex = -1;
			MainGame.WorldData.AddWgoData(wgoData11);
			if (!string.IsNullOrEmpty(data.characterId))
			{
				WgoData wgoData12 = new WgoData(data.characterId, wgoData.Position, wgoData.WorldId);
				wgoData12.CustomTag = customTag.Replace(oldValue, "t_b_character_");
				wgoData12.LinkedToTownBuildingUniqueId = wgoData11.UniqueId;
				wgoData11.LinkedFromTownBuildingUniqueId = wgoData12.UniqueId;
				MainGame.WorldData.AddWgoData(wgoData12);
				foreach (LazyExpression item3 in data.expressionOnCharCreate)
				{
					item3.EvaluateBool(wgoData12);
				}
			}
			else if (!wgoData.LinkedFromTownBuildingUniqueId.IsEmpty)
			{
				WgoData wgoData13 = MainGame.WorldData.GetWgoData(wgoData.LinkedFromTownBuildingUniqueId);
				wgoData13.LinkedToTownBuildingUniqueId = wgoData11.UniqueId;
				wgoData11.LinkedFromTownBuildingUniqueId = wgoData.LinkedFromTownBuildingUniqueId;
				wgoData11.SetGameRes("available_by_time", wgoData13.GetGameResInt("available_by_time"));
			}
			MainGame.WorldData.RemoveWgoDataFromGameScene(wgoData.UniqueId);
		}
		UpdateTownBuildingVendor(data, tierIndex > 0);
	}

	private void UpdateTownBuildingVendor(TownBuildingDef def, bool forceLevelUp)
	{
		if (!string.IsNullOrEmpty(def.vendorId))
		{
			KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
			if (!knowledgeSystem.IsVendorForOrdersUnlocked(def.vendorId))
			{
				knowledgeSystem.UnlockVendorForOrders(def.vendorId);
			}
			if (forceLevelUp)
			{
				MainGame.Instance.GameSave.vendorSystem.ForceLevelUpVendor(def.vendorId);
			}
		}
	}
}
