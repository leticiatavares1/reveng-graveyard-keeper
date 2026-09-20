using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ConveyorCellStationComponent : ConveyorComponent
{
	public List<SGuid> connectedWgoDataUniqueId = new List<SGuid>();

	[SerializeField]
	private int currentTransferIndex;

	[NonSerialized]
	public List<ConveyorWgoData> connectedWgoData = new List<ConveyorWgoData>();

	public List<ConveyorWgoData> ConnectedWgoData
	{
		get
		{
			if (connectedWgoData == null)
			{
				connectedWgoData = new List<ConveyorWgoData>();
			}
			if (connectedWgoData.Count == 0 && connectedWgoDataUniqueId.Count > 0)
			{
				for (int i = 0; i < connectedWgoDataUniqueId.Count; i++)
				{
					connectedWgoData.Add(MainGame.Instance.GameSave.worldData.GetWgoData(connectedWgoDataUniqueId[i]) as ConveyorWgoData);
				}
			}
			return connectedWgoData;
		}
	}

	public ConveyorCellStationComponent(ConveyorWgoData conveyorWgoData)
		: base(conveyorWgoData)
	{
	}

	public override void DoJob(ConveyorComponent visitor = null)
	{
		base.CurrentVisitState = VisitState.Visiting;
		PerformItemTransfer();
		foreach (ConveyorWgoData value in base.ParentsData.Values)
		{
			ConveyorComponent conveyorComponent = value.ConveyorComponent;
			if (conveyorComponent.CanBeVisitedBy(this))
			{
				conveyorComponent.DoJob(this);
			}
			else if (conveyorComponent.CurrentVisitState == VisitState.Visiting)
			{
				conveyorComponent.HandleCycleDependency(conveyorComponent);
			}
		}
		base.CurrentVisitState = VisitState.Visited;
	}

	public override void PerformItemTransfer()
	{
		if (wasPerformedItemTransfer)
		{
			return;
		}
		List<ConveyorWgoData> list = new List<ConveyorWgoData>(base.ParentsData.Values.ToList());
		for (int i = 0; i < list.Count; i++)
		{
			currentTransferIndex = MathUtilities.ClampCycle(currentTransferIndex, 0, list.Count - 1);
			ConveyorWgoData conveyorWgoData = list[currentTransferIndex];
			currentTransferIndex++;
			if (!conveyorWgoData.CraftComponent.IsDestroyingCraftActive && conveyorWgoData.ConveyorComponent.CanGiveItem(this))
			{
				ConveyorComponent conveyorComponent = conveyorWgoData.ConveyorComponent;
				if ((conveyorComponent is ConveyorCellComponent || conveyorComponent is ConveyorCellUndergroundComponent || conveyorComponent is ConveyorPalletComponent || conveyorComponent is ConveyorChestComponent || conveyorComponent is ConveyorSplitterComponent) && base.WgoData.Inventory.Data.Inventory.Count == 0)
				{
					List<Item> items = conveyorWgoData.ConveyorComponent.GiveItem(this);
					base.WgoData.Inventory.AddItemsToInventory(items);
					wasPerformedItemTransfer = true;
					UpdateInOutItemData(conveyorWgoData);
					TryPlaceConveyorPickupOrder();
					break;
				}
			}
		}
	}

	private void TryPlaceConveyorPickupOrder()
	{
		WorldZoneData worldZoneData = base.WgoData.WorldZoneData;
		if (worldZoneData != null && base.WgoData.Inventory.Data.Inventory.Count != 0 && worldZoneData.FindOrdersByTarget(base.WgoData.UniqueId, typeof(ConveyorPickupOrder)).Count <= 0)
		{
			Item item = base.WgoData.Inventory.Data.Inventory[0];
			worldZoneData.PlaceNewOrder(new ConveyorPickupOrder(base.WgoData.UniqueId, new Item(item.id, item.Count)));
		}
	}

	public override bool Disconnect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType)
	{
		if (!connectedWgoDataUniqueId.Contains(conveyorWgoData.UniqueId))
		{
			Debug.Log($"Trying to disconnect  {conveyorWgoData.UniqueId} that is not connected");
			return false;
		}
		Debug.Log($"Disconnected {conveyorWgoData.id} with guid {conveyorWgoData.UniqueId} from {base.WgoData.id} with guid {wgoDataUniqueId}");
		connectedWgoDataUniqueId.Remove(conveyorWgoData.UniqueId);
		ConnectedWgoData.Remove(conveyorWgoData);
		conveyorWgoData.ConveyorComponent.RemoveParentData(base.WgoData);
		UpdateWgoPartState();
		return true;
	}

	public override bool Contains(ConveyorComponent conveyorComponent)
	{
		if (ConnectedWgoData.Contains(conveyorComponent.WgoData))
		{
			return true;
		}
		return false;
	}

	public override void GetEndElement(ref List<ConveyorComponent> endElements)
	{
		base.CurrentVisitState = VisitState.Visiting;
		if (ConnectedWgoData.Count == 0)
		{
			endElements.Add(this);
		}
		foreach (ConveyorWgoData connectedWgoDatum in ConnectedWgoData)
		{
			ConveyorComponent conveyorComponent = connectedWgoDatum.ConveyorComponent;
			if (conveyorComponent.CurrentVisitState == VisitState.NotVisited)
			{
				conveyorComponent.GetEndElement(ref endElements);
			}
			else if (conveyorComponent.CurrentVisitState == VisitState.Visiting && !endElements.Contains(conveyorComponent))
			{
				endElements.Add(this);
			}
		}
		base.CurrentVisitState = VisitState.Visited;
	}

	public override void HandleCycleDependency(ConveyorComponent cyrcleComponent)
	{
		if (wasPerformedItemTransfer)
		{
			return;
		}
		PerformItemTransfer();
		if (base.ParentsData.Values.Contains(cyrcleComponent.WgoData))
		{
			return;
		}
		foreach (ConveyorWgoData value in base.ParentsData.Values)
		{
			value.ConveyorComponent.HandleCycleDependency(cyrcleComponent);
		}
	}

	public override bool CanGiveItem(ConveyorComponent conveyorComponent)
	{
		return false;
	}

	private void UpdateInOutItemData(ConveyorWgoData giver)
	{
		TryGetParentConnectionDirection(giver.UniqueId, out var direction);
		if (giver.Definition.conveyorType == ConveyorElementType.Chest || giver.Definition.conveyorType == ConveyorElementType.Workbench)
		{
			giver.ConveyorComponent.OutItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: false);
			base.InItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: false);
		}
		else
		{
			giver.ConveyorComponent.OutItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: true);
			base.InItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: true);
		}
	}
}
