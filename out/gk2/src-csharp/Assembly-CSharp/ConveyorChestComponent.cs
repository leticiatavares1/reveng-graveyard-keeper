using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ConveyorChestComponent : ConveyorComponent
{
	[SerializeField]
	private List<SGuid> connectedWgoDataUniqueId = new List<SGuid>();

	[SerializeField]
	private List<ConveyorChestSlotData> slotsData;

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

	public List<ConveyorChestSlotData> SlotsData
	{
		get
		{
			if (slotsData == null)
			{
				slotsData = new List<ConveyorChestSlotData>
				{
					new ConveyorChestSlotData(Direction.Left),
					new ConveyorChestSlotData(Direction.Up),
					new ConveyorChestSlotData(Direction.Right),
					new ConveyorChestSlotData(Direction.Down)
				};
			}
			return slotsData;
		}
	}

	public ConveyorChestComponent(ConveyorWgoData conveyorWgoData)
		: base(conveyorWgoData)
	{
	}

	public override void DoJob(ConveyorComponent visitor = null)
	{
		base.CurrentVisitState = VisitState.Visiting;
		PerformItemTransfer();
		foreach (ConveyorWgoData value in base.ParentsData.Values)
		{
			if (value != null)
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
		}
		base.CurrentVisitState = VisitState.Visited;
	}

	public override void PerformItemTransfer()
	{
		if (base.WgoData.CraftComponent.IsDestroyingCraftActive)
		{
			return;
		}
		foreach (ConveyorWgoData value in base.ParentsData.Values)
		{
			if (value != null && value.ConveyorComponent.CanGiveItem(this))
			{
				ConveyorComponent conveyorComponent = value.ConveyorComponent;
				if ((conveyorComponent is ConveyorCellComponent || conveyorComponent is ConveyorSplitterComponent) && base.WgoData.Inventory.CanAddItemToInventory(value.Inventory.Data.Inventory[0].id, 1))
				{
					TryGetParentConnectionDirection(value.UniqueId, out var direction);
					List<Item> list = value.Inventory.RemoveItemById(value.Inventory.Data.Inventory[0].id, 1);
					value.ConveyorComponent.OutItem = new ConveyorMovableItemData(list[0].id, direction, isCommon: false);
					base.WgoData.Inventory.AddItemsToInventory(list);
					GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.ConveyorChestItemAdded, base.WgoData.id + ":" + list[0].id);
					wasPerformedItemTransfer = true;
				}
			}
		}
	}

	public override bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction = Direction.None)
	{
		if (connectedWgoDataUniqueId.Contains(conveyorWgoData.UniqueId))
		{
			Debug.LogWarning($"Trying to connect  {conveyorWgoData.UniqueId} that is already connected");
			return false;
		}
		ConveyorComponent conveyorComponent = conveyorWgoData.ConveyorComponent;
		if (!(conveyorComponent is ConveyorCellComponent) && !(conveyorComponent is ConveyorSplitterComponent) && !(conveyorComponent is ConveyorCellUndergroundComponent))
		{
			return false;
		}
		Debug.Log($"Connected {conveyorWgoData.id} with guid {conveyorWgoData.UniqueId} to {base.WgoData.id} with guid {wgoDataUniqueId}");
		connectedWgoDataUniqueId.Add(conveyorWgoData.UniqueId);
		if (!ConnectedWgoData.Contains(conveyorWgoData))
		{
			ConnectedWgoData.Add(conveyorWgoData);
		}
		conveyorWgoData.ConveyorComponent.AddParentData(base.WgoData, direction);
		ConveyorChestSlotData conveyorChestSlotData = SlotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == direction);
		if (conveyorChestSlotData != null)
		{
			conveyorChestSlotData.conveyorWgoDataUniqueId = conveyorWgoData.UniqueId;
		}
		occupiedConnectorsDirections.Add(direction);
		OnConnectedEvent();
		return true;
	}

	public override bool Disconnect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType)
	{
		if (!connectedWgoDataUniqueId.Contains(conveyorWgoData.UniqueId))
		{
			Debug.LogWarning($"Trying to disconnect  {conveyorWgoData.UniqueId} that is not connected");
			return false;
		}
		Debug.Log($"Disconnected {conveyorWgoData.UniqueId} as {connectionType}");
		connectedWgoDataUniqueId.Remove(conveyorWgoData.UniqueId);
		ConnectedWgoData.Remove(conveyorWgoData);
		Direction direction;
		bool num = conveyorWgoData.ConveyorComponent.TryGetParentConnectionDirection(wgoDataUniqueId, out direction);
		conveyorWgoData.ConveyorComponent.RemoveParentData(base.WgoData);
		SlotsData.Find((ConveyorChestSlotData x) => x.conveyorWgoDataUniqueId == conveyorWgoData.UniqueId)?.Clear();
		if (num)
		{
			occupiedConnectorsDirections.Remove(direction);
		}
		OnDisconnectedEvent();
		return true;
	}

	public override bool DisconnectChilds()
	{
		foreach (ConveyorWgoData connectedWgoDatum in ConnectedWgoData)
		{
			if (connectedWgoDatum != null)
			{
				Debug.Log($"Disconnected {connectedWgoDatum.id} with guid {connectedWgoDatum.UniqueId} from {base.WgoData.id} with guid {wgoDataUniqueId}");
				connectedWgoDatum.ConveyorComponent.RemoveParentData(base.WgoData);
			}
		}
		connectedWgoDataUniqueId.Clear();
		ConnectedWgoData.Clear();
		foreach (ConveyorChestSlotData slotsDatum in SlotsData)
		{
			slotsDatum.Clear();
		}
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
		string itemId;
		return TryGetItemIdToGive(conveyorComponent, out itemId);
	}

	public override List<Item> GiveItem(ConveyorComponent conveyorComponent)
	{
		if (TryGetItemIdToGive(conveyorComponent, out var itemId))
		{
			return base.WgoData.Inventory.RemoveItemById(itemId, 1);
		}
		return new List<Item>();
	}

	public override void AddParentData(ConveyorWgoData conveyorWgoData, Direction direction)
	{
		base.AddParentData(conveyorWgoData, direction);
		Direction oppositeDir = direction.OppositeDir();
		ConveyorChestSlotData conveyorChestSlotData = SlotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == oppositeDir);
		if (conveyorChestSlotData != null)
		{
			conveyorChestSlotData.conveyorWgoDataUniqueId = conveyorWgoData.UniqueId;
		}
		occupiedConnectorsDirections.Add(oppositeDir);
		OnConnectedEvent();
	}

	public override void RemoveParentData(ConveyorWgoData conveyorWgoData)
	{
		Direction direction;
		bool num = TryGetParentConnectionDirection(conveyorWgoData.UniqueId, out direction);
		base.RemoveParentData(conveyorWgoData);
		SlotsData.Find((ConveyorChestSlotData x) => x.conveyorWgoDataUniqueId == conveyorWgoData.UniqueId)?.Clear();
		if (num)
		{
			occupiedConnectorsDirections.Remove(direction.OppositeDir());
		}
		OnDisconnectedEvent();
	}

	public override bool HasChildsInDirection(Direction direction)
	{
		SGuid conveyorWgoDataUniqueId = slotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == direction).conveyorWgoDataUniqueId;
		if (SGuid.IsNullOrEmpty(conveyorWgoDataUniqueId))
		{
			return false;
		}
		return !parentsUniqueIds.Contains(conveyorWgoDataUniqueId);
	}

	public override bool HasParentsInDirection(Direction direction)
	{
		SGuid conveyorWgoDataUniqueId = slotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == direction).conveyorWgoDataUniqueId;
		if (SGuid.IsNullOrEmpty(conveyorWgoDataUniqueId))
		{
			return false;
		}
		return parentsUniqueIds.Contains(conveyorWgoDataUniqueId);
	}

	private bool TryGetItemIdToGive(ConveyorComponent conveyorComponent, out string itemId)
	{
		itemId = null;
		ConveyorChestSlotData conveyorChestSlotData = SlotsData.Find((ConveyorChestSlotData x) => x.conveyorWgoDataUniqueId == conveyorComponent.WgoData.UniqueId);
		if (conveyorChestSlotData == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(conveyorChestSlotData.slotItemId))
		{
			if (!base.WgoData.Inventory.Data.HasItemQuantityInInventory(conveyorChestSlotData.slotItemId, 1))
			{
				return false;
			}
			itemId = conveyorChestSlotData.slotItemId;
			return true;
		}
		if (base.WgoData.Inventory.Data.Inventory.Count == 0)
		{
			return false;
		}
		itemId = base.WgoData.Inventory.Data.Inventory[0].id;
		return true;
	}
}
