using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class BuildManager : LazySingleton<BuildManager>
{
	[SerializeField]
	private BuildController buildController;

	private Wgo currentBuildDesk;

	private List<BuildData> buildDataList;

	private WorldZone worldZone;

	private List<Inventory> additionalInventories;

	private Func<List<Inventory>> getAdditionalInventories;

	public BuildController BuildController => buildController;

	public WorldZone WorldZone
	{
		get
		{
			return worldZone;
		}
		set
		{
			worldZone = value;
		}
	}

	public bool TryEnable(Wgo builder, Func<List<Inventory>> getAdditionalInventories = null)
	{
		if (!builder.TryGetNearestBuilderWorldZone(out worldZone))
		{
			Debug.LogError("Can not enable build mode, builder not in any world zones");
			return false;
		}
		if (!FormBuildData(builder))
		{
			Debug.LogError("Can not enable build mode, can not form build data");
			return false;
		}
		this.getAdditionalInventories = getAdditionalInventories;
		RefreshAdditionalInventories();
		OpenBuildingWindow(builder, additionalInventories);
		return true;
	}

	public void EnableBuildMode(Wgo currentBuildDesk, BuildData selectedBuildData, List<NeedItemData> selectedItems, List<Inventory> additionalInventories = null)
	{
		this.currentBuildDesk = currentBuildDesk;
		this.additionalInventories = additionalInventories;
		MultiInventory multiInventory = new MultiInventory(MainGame.PlayerController.PlayerData);
		if (additionalInventories != null)
		{
			for (int i = 0; i < additionalInventories.Count; i++)
			{
				multiInventory.Add(additionalInventories[i]);
			}
		}
		MainGame.PlayerController.SetControlTakenType(TakenControlType.ByBuilding, isEnabled: false);
		buildController.EnableBuildMode(selectedBuildData, worldZone, selectedItems, multiInventory);
	}

	public void Disable()
	{
		MainGame.PlayerController.SetControlTakenType(TakenControlType.ByBuilding, isEnabled: true);
		FormBuildData(currentBuildDesk);
		RefreshAdditionalInventories();
		OpenBuildingWindow(currentBuildDesk, additionalInventories);
	}

	private bool FormBuildData(Wgo buildDesk)
	{
		if (worldZone == null)
		{
			Debug.LogError("Can not form BuildData, player not in any world zones");
			return false;
		}
		List<BuildData> list = new List<BuildData>();
		if (buildDesk.Id == "test_playground_builder")
		{
			foreach (BuildingDef buildingDef in GameBalance.Me.buildingDefs)
			{
				BuildingDef.BuildingMode buildingMode = buildingDef.buildingMode;
				if (buildingMode != 0 && buildingMode != BuildingDef.BuildingMode.Remove)
				{
					list.Add(BuildData.GetDataForBuild(buildingDef));
				}
			}
		}
		else
		{
			list = BuildingDef.GetBuildingsInBuilder(buildDesk);
		}
		buildDataList = list;
		return true;
	}

	private void OpenBuildingWindow(Wgo builder, List<Inventory> additionalInventories = null)
	{
		UIBuildingWindow buildWindow = LazyUI.GetWindow<UIBuildingWindow>();
		UIBuildingWindowData data = new UIBuildingWindowData(builder, MainGame.PlayerController.PlayerData, buildDataList, OnBuildPressed, CanBuild, additionalInventories);
		buildWindow.Open(data);
		bool CanBuild(BuildData buildData, List<NeedItemData> selectedNeedItems)
		{
			MultiInventory multiInventory = new MultiInventory(MainGame.PlayerController.PlayerData);
			if (additionalInventories != null)
			{
				for (int i = 0; i < additionalInventories.Count; i++)
				{
					multiInventory.Add(additionalInventories[i]);
				}
			}
			if (selectedNeedItems == null || multiInventory.HasItemsById(selectedNeedItems))
			{
				BuildingDef definition = buildData.Definition;
				if (definition == null || !definition.HasLimits)
				{
					return true;
				}
				return buildData.Definition.limitMax > buildData.Definition.currentLimitExpression.EvaluateInt();
			}
			return false;
		}
		void OnBuildPressed(BuildData buildData, List<NeedItemData> selectedNeedItems)
		{
			if (buildData.BuildingMode != BuildingDef.BuildingMode.Script)
			{
				LazySingleton<BuildManager>.Instance.EnableBuildMode(builder, buildData, selectedNeedItems, additionalInventories);
			}
			else
			{
				MultiInventory multiInventory2 = new MultiInventory(MainGame.PlayerController.PlayerData);
				if (additionalInventories != null)
				{
					for (int j = 0; j < additionalInventories.Count; j++)
					{
						multiInventory2.Add(additionalInventories[j]);
					}
				}
				if (selectedNeedItems != null)
				{
					multiInventory2.RemoveItems(selectedNeedItems);
				}
				if (buildData.Definition != null)
				{
					foreach (LazyExpression item in buildData.Definition.expressionAfterBuilding)
					{
						item.EvaluateBool();
					}
					GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.BuildBuilding, buildData.Definition.wgoId);
				}
			}
			buildWindow.Close();
		}
	}

	private void RefreshAdditionalInventories()
	{
		additionalInventories = getAdditionalInventories?.Invoke();
	}
}
