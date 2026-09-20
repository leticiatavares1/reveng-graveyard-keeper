using System.Collections.Generic;

public static class ZombieDeliveryIndication
{
	private static string cachedStationIconId;

	public static bool IsCaretakerStation(WgoData wgoData)
	{
		if (wgoData?.Definition != null)
		{
			return wgoData.Definition.interactionType == WGODef.InteractionType.Station;
		}
		return false;
	}

	public static bool ShouldShowNoCaretakerAssigned(WgoData wgoData)
	{
		if (!IsCaretakerStation(wgoData) || wgoData.isTempObject || wgoData.IsHidden)
		{
			return false;
		}
		return wgoData.Worker == null;
	}

	public static bool IsCraftStalledWithoutCaretaker(WgoData workbench)
	{
		if (workbench?.Definition == null || workbench.isTempObject || workbench.IsHidden)
		{
			return false;
		}
		if (!(workbench.Worker is ZombieWgoData { ZombieType: ZombieType.Crafter } zombieWgoData))
		{
			return false;
		}
		CraftElementBase currentCraftElement = workbench.CraftComponent.CurrentCraftElement;
		if (currentCraftElement == null || currentCraftElement.IsStarted || !(currentCraftElement.Def is CraftDef { isConveyorCraft: false }))
		{
			return false;
		}
		if (!(zombieWgoData.CrafterCurrentOrder is DeliveryOrder))
		{
			return false;
		}
		WorldZoneData worldZoneData = workbench.WorldZoneData;
		if (worldZoneData != null)
		{
			return !HasCaretakerInZone(worldZoneData);
		}
		return false;
	}

	public static bool HasCaretakerInZone(WorldZoneData zone)
	{
		if (zone == null)
		{
			return false;
		}
		foreach (SGuid zombieOnSceneWgoId in MainGame.ZombieSystemData.zombieOnSceneWgoIds)
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(zombieOnSceneWgoId);
			if (zombie != null && zombie.ZombieType == ZombieType.Caretaker)
			{
				WorldZoneData worldZoneData = zombie.AttachedWgoData?.WorldZoneData ?? zombie.WorldZoneData;
				if (worldZoneData != null && worldZoneData.id == zone.id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetStationIconId()
	{
		if (!string.IsNullOrEmpty(cachedStationIconId))
		{
			return cachedStationIconId;
		}
		WGODef wGODef = GameBalance.Me?.GetDataOrNull<WGODef>("zombie_supplier_station");
		if (wGODef != null && wGODef.TryGetBuildingDefForWgo(out var buildingDef))
		{
			cachedStationIconId = buildingDef.BuildResultIcon;
		}
		else
		{
			cachedStationIconId = "i_b_blueprint_placeholder";
		}
		return cachedStationIconId;
	}

	public static void RedrawCrafterWorkbenchesInZone(WorldZoneData zone)
	{
		if (zone?.wgoDataList == null)
		{
			return;
		}
		List<SGuid> wgoDataList = zone.wgoDataList;
		for (int i = 0; i < wgoDataList.Count; i++)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(wgoDataList[i]);
			if (wgoData?.Worker is ZombieWgoData { ZombieType: ZombieType.Crafter })
			{
				Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(wgoData.UniqueId);
				if (wgoViewGlobal != null)
				{
					wgoViewGlobal.DrawWidgets();
				}
			}
		}
	}
}
