using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConveyorWgoData : WgoData
{
	[SerializeField]
	private ConveyorComponent conveyorComponent;

	[SerializeField]
	private List<SGuid> hardConnectedWGOs = new List<SGuid>();

	public List<SGuid> HardConnectedWGOs
	{
		get
		{
			if (hardConnectedWGOs == null)
			{
				hardConnectedWGOs = new List<SGuid>();
			}
			return hardConnectedWGOs;
		}
	}

	public ConveyorComponent ConveyorComponent => conveyorComponent;

	public ConveyorWgoData(ConveyorElementType conveyorElementType, string id, Vector3 position, string worldId)
		: base(id, position, worldId)
	{
		TryCreateConveyorComponent(conveyorElementType);
	}

	public override void PrepareForGame()
	{
		base.PrepareForGame();
		TryCreateConveyorComponent(base.Definition.conveyorType);
		conveyorComponent.WgoData = this;
		conveyorComponent.Init();
	}

	public override void DeInit()
	{
		base.DeInit();
		conveyorComponent.DeInit();
	}

	public override MultiInventory GetCraftableMultiInventory(bool excludeWorkerInventory = false)
	{
		MultiInventory multiInventory = new MultiInventory();
		if (base.WorldZoneData != null)
		{
			foreach (SGuid wgoData2 in base.WorldZoneData.wgoDataList)
			{
				WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoData2);
				if (wgoData != null && wgoData.Inventory.Data.HasProperty<FuelContainerSerializedItemProperty>())
				{
					multiInventory.Add(wgoData.Inventory);
					multiInventory.Add(wgoData.CraftableObjectCraftInventory);
				}
			}
			multiInventory.Add(base.Inventory);
			multiInventory.Add(base.CraftInventory);
		}
		else
		{
			multiInventory.Add(new MultiInventory(new List<Inventory> { base.Inventory, base.CraftInventory }));
		}
		return multiInventory;
	}

	protected override float GetQuality()
	{
		if (base.Definition.conveyorType == ConveyorElementType.PowerSource)
		{
			List<DockPointData> dockPoints = base.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyOccupied, DockPointData.Filter.OnlyZombie);
			return (int)base.Definition.quality.EvaluateFloat(this) * dockPoints.Count;
		}
		return base.Definition.quality.EvaluateFloat(this) + (base.Definition.considerInventoryQuality ? base.Inventory.GetTotalQuality() : 0f);
	}

	public void UpdateAttachedWgoViewWidgets()
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(base.UniqueId);
		if (wgoViewGlobal != null)
		{
			wgoViewGlobal.DrawWidgets();
		}
	}

	private void TryCreateConveyorComponent(ConveyorElementType conveyorElementType)
	{
		if (ConveyorComponent == null)
		{
			switch (conveyorElementType)
			{
			case ConveyorElementType.Cell:
				conveyorComponent = new ConveyorCellComponent(this);
				break;
			case ConveyorElementType.Workbench:
				conveyorComponent = new ConveyorWorkbenchComponent(this);
				break;
			case ConveyorElementType.Pallet:
				conveyorComponent = new ConveyorPalletComponent(this);
				break;
			case ConveyorElementType.Chest:
				conveyorComponent = new ConveyorChestComponent(this);
				break;
			case ConveyorElementType.PowerSource:
				conveyorComponent = new ConveyorPowerSourceComponent(this);
				break;
			case ConveyorElementType.Splitter:
				conveyorComponent = new ConveyorSplitterComponent(this);
				break;
			case ConveyorElementType.UndergroundCell:
				conveyorComponent = new ConveyorCellUndergroundComponent(this);
				break;
			case ConveyorElementType.StationCell:
				conveyorComponent = new ConveyorCellStationComponent(this);
				break;
			case ConveyorElementType.ChestOut:
				conveyorComponent = new ConveyorChestOutComponent(this);
				break;
			}
		}
	}

	public override void SetDataFromDefinition()
	{
		base.SetDataFromDefinition();
		if (ConveyorComponent is ConveyorChestOutComponent conveyorChestOutComponent)
		{
			conveyorChestOutComponent.UpdateSlotsData();
		}
	}
}
