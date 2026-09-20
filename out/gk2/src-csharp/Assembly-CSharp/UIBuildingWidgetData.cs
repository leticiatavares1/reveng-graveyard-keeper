using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine.UI;

public class UIBuildingWidgetData : LazyWidgetDataBase
{
	public BuildData BuildData { get; private set; }

	public Action<List<NeedItemData>> OnPress { get; private set; }

	public Func<List<NeedItemData>, bool> CanBuild { get; private set; }

	public Action OnOver { get; private set; }

	public Action OnOut { get; private set; }

	public List<UICraftItemCellData> CraftItemCellsData { get; private set; }

	public MultiInventory MultiInventory { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public string DescriptionModules { get; private set; }

	public WorldZoneData WorldZoneData { get; private set; }

	public UIBuildingWidgetData(BuildData buildData, MultiInventory multiInventory, Action<BuildData, List<NeedItemData>> onPress, Func<BuildData, List<NeedItemData>, bool> canBuild, Action onOver, Action onOut, WorldZoneData worldZoneData)
	{
		UIBuildingWidgetData uIBuildingWidgetData = this;
		BuildData = buildData;
		MultiInventory = multiInventory;
		WorldZoneData = worldZoneData;
		OnPress = OnPressAction;
		CanBuild = CanBuildFunc;
		OnOver = onOver;
		OnOut = onOut;
		FillCraftItemCellsData();
		Name = ((buildData.BuildingMode == BuildingDef.BuildingMode.Remove) ? LLBase.L("remove") : buildData.Definition.id);
		if (buildData.Definition == null)
		{
			return;
		}
		WgoPartBakedData wgoPartBakedData = LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.Get(buildData.Definition.wgoId);
		if (wgoPartBakedData != null && wgoPartBakedData.ModuleBuildingTypes != null && !wgoPartBakedData.ModuleBuildingTypes.IsEmpty())
		{
			DescriptionModules = string.Empty;
			foreach (GameResAtom item in wgoPartBakedData.ModuleBuildingTypes.List)
			{
				DescriptionModules += $"{item.type.FontIcon()}{item.value} ";
			}
			DescriptionModules.TrimEnd();
			return;
		}
		WGODef data = GameBalance.Me.GetData<WGODef>(buildData.Definition.wgoId);
		if (data != null && data.replaceToWgoOnDie.HasExpression)
		{
			data = GameBalance.Me.GetData<WGODef>(data.replaceToWgoOnDie.Evaluate());
		}
		if (worldZoneData != null && data != null && !string.IsNullOrEmpty(worldZoneData.Definition.qualityIcon) && data.qualityDisplayType == WGODef.QualityDisplayType.Show)
		{
			float num = data.quality.EvaluateFloat();
			if (num != 0f)
			{
				Description = string.Format("{0}{1}{2}", worldZoneData.Definition.qualityIcon.FontIcon(), (num > 0f) ? "+" : "-", Math.Abs(num));
			}
		}
		bool CanBuildFunc(List<NeedItemData> needItems)
		{
			return canBuild(uIBuildingWidgetData.BuildData, needItems);
		}
		void OnPressAction(List<NeedItemData> needItems)
		{
			onPress?.Invoke(uIBuildingWidgetData.BuildData, needItems);
		}
	}

	public List<NeedItemData> GetCurrentNeedItems()
	{
		List<NeedItemData> list = new List<NeedItemData>();
		for (int i = 0; i < CraftItemCellsData.Count; i++)
		{
			list.Add(CraftItemCellsData[i].currentItem);
		}
		return list;
	}

	private void FillCraftItemCellsData()
	{
		if (CraftItemCellsData != null)
		{
			CraftItemCellsData.Clear();
		}
		else
		{
			CraftItemCellsData = new List<UICraftItemCellData>();
		}
		if (BuildData.NeedItems == null)
		{
			return;
		}
		foreach (NeedItemData needItem in BuildData.NeedItems)
		{
			CraftItemCellsData.Add(new UICraftItemCellData(needItem, MultiInventory, null));
		}
	}
}
