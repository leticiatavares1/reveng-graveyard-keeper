using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIInfoWidgetData : LazyWidgetDataBase
{
	public CraftComponent CraftComponent { get; set; }

	public Sprite Icon { get; set; }

	public string Header { get; private set; }

	public string Description { get; private set; }

	public string WorldZoneQuality { get; private set; }

	public WgoData WgoData { get; private set; }

	public CraftDef CraftDef { get; private set; }

	public bool ShowTickDuration { get; private set; }

	public bool ExcludePlayerFromMultiinventoryWhenCountItemsForFuel { get; set; } = true;


	public IWorker ForcedWorker { get; set; }

	public bool DefineIconBackgroundFromWgo { get; private set; }

	public IWorker Worker
	{
		get
		{
			if (ForcedWorker != null)
			{
				return ForcedWorker;
			}
			return WgoData.Worker;
		}
	}

	public UIInfoWidgetData(WgoData wgoData, string customBuildDeskIcon = null, bool defineIconBackgroundFromWgo = true)
	{
		Header = LLBase.L(wgoData.id);
		string text = wgoData.id + "_d";
		Description = LLBase.L(wgoData.id + "_d");
		if (Description == text)
		{
			Description = string.Empty;
		}
		CraftComponent = wgoData.CraftComponent;
		WgoData = wgoData;
		CraftDef = null;
		ShowTickDuration = true;
		BuildingDef buildingDef;
		if (!string.IsNullOrEmpty(customBuildDeskIcon))
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(customBuildDeskIcon);
		}
		else if (wgoData.Definition.TryGetBuildingDefForWgo(out buildingDef))
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(buildingDef.BuildResultIcon);
		}
		else if (wgoData.Definition.interactionType == WGODef.InteractionType.Craft)
		{
			if (!string.IsNullOrEmpty(wgoData.Definition.craftIconId))
			{
				Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(wgoData.Definition.craftIconId);
			}
			else
			{
				Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("i_b_" + wgoData.id);
			}
		}
		else if (wgoData.WorldZoneData != null)
		{
			if (!string.IsNullOrEmpty(wgoData.WorldZoneData.Definition.qualityIcon))
			{
				WorldZoneQuality = $"{wgoData.WorldZoneData.Definition.qualityIcon.FontIcon()}{wgoData.WorldZoneData.GetTotalQuality()}";
			}
			string spriteName = (string.IsNullOrEmpty(wgoData.WorldZoneData.Definition.buildDeskIcon) ? ("i_z_" + wgoData.WorldZoneData.id) : wgoData.WorldZoneData.Definition.buildDeskIcon);
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(spriteName);
		}
		else
		{
			WGODef.InteractionType interactionType = wgoData.Definition.interactionType;
			if ((interactionType == WGODef.InteractionType.Builder || interactionType == WGODef.InteractionType.FightBuilder) && wgoData.TryGetNearestBuilderWorldZone(out var worldZoneData))
			{
				Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(worldZoneData.Definition.buildDeskIcon);
			}
		}
		TryFillWorldZoneQuality(wgoData);
		if (Icon == null)
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("i_b_hammer");
		}
		DefineIconBackgroundFromWgo = defineIconBackgroundFromWgo;
	}

	private void TryFillWorldZoneQuality(WgoData wgoData)
	{
		if (!string.IsNullOrEmpty(WorldZoneQuality) || wgoData == null)
		{
			return;
		}
		WorldZoneData worldZoneData = wgoData.WorldZoneData;
		if (worldZoneData == null)
		{
			wgoData.TryGetNearestBuilderWorldZone(out worldZoneData);
		}
		if (worldZoneData?.Definition != null && !string.IsNullOrEmpty(worldZoneData.Definition.qualityIcon))
		{
			string qualityIcon = worldZoneData.Definition.qualityIcon;
			if (!(qualityIcon != "gear") || !(qualityIcon != "corpse") || !(qualityIcon != "body"))
			{
				WorldZoneQuality = $"{qualityIcon.FontIcon()}{worldZoneData.GetTotalQuality()}";
			}
		}
	}

	public UIInfoWidgetData(WgoData wgoData, Item item, bool defineIconBackgroundFromWgo = true)
	{
		Header = item.Definition.GetHeader();
		string descriptionLocale = item.Definition.GetDescriptionLocale();
		Description = LLBase.L(descriptionLocale);
		if (Description == descriptionLocale)
		{
			Description = string.Empty;
		}
		if (wgoData != null && wgoData.Definition.TryGetBuildingDefForWgo(out var buildingDef))
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(buildingDef.BuildResultIcon);
		}
		else
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("i_b_hammer");
		}
		WgoData = wgoData;
		CraftDef = null;
		DefineIconBackgroundFromWgo = defineIconBackgroundFromWgo;
	}

	public UIInfoWidgetData(WgoData wgoData, CraftDef craftDef, bool defineIconBackgroundFromWgo = true)
	{
		Header = LLBase.L(wgoData.id);
		string text = craftDef.id + "_d";
		Description = LLBase.L(text);
		if (Description == text)
		{
			Description = string.Empty;
		}
		BuildingDef buildingDef;
		if (!string.IsNullOrEmpty(wgoData.Definition.craftIconId))
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(wgoData.Definition.craftIconId);
		}
		else if (wgoData.Definition.TryGetBuildingDefForWgo(out buildingDef))
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(buildingDef.BuildResultIcon);
		}
		else
		{
			Icon = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("i_b_hammer");
		}
		CraftComponent = wgoData.CraftComponent;
		WgoData = wgoData;
		CraftDef = craftDef;
		ShowTickDuration = false;
		DefineIconBackgroundFromWgo = defineIconBackgroundFromWgo;
	}
}
