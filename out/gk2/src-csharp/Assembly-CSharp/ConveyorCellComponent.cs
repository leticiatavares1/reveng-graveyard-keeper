using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ConveyorCellComponent : ConveyorComponent
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

	public ConveyorCellComponent(ConveyorWgoData conveyorWgoData)
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
				if ((conveyorComponent is ConveyorCellComponent || conveyorComponent is ConveyorCellUndergroundComponent || conveyorComponent is ConveyorPalletComponent || conveyorComponent is ConveyorChestComponent || conveyorComponent is ConveyorSplitterComponent || conveyorComponent is ConveyorChestOutComponent) && base.WgoData.Inventory.Data.Inventory.Count == 0)
				{
					List<Item> items = conveyorWgoData.ConveyorComponent.GiveItem(this);
					base.WgoData.Inventory.AddItemsToInventory(items);
					wasPerformedItemTransfer = true;
					UpdateInOutItemData(conveyorWgoData);
					break;
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
		if (!(conveyorComponent is ConveyorCellComponent) && !(conveyorComponent is ConveyorChestComponent) && !(conveyorComponent is ConveyorSplitterComponent) && !(conveyorComponent is ConveyorCellUndergroundComponent) && !(conveyorComponent is ConveyorCellStationComponent))
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
		UpdateWgoPartState();
		return true;
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

	public override void UpdateWgoPartState()
	{
		if (base.WgoData == null)
		{
			return;
		}
		ConveyorWgoData conveyorWgoData = ConnectedWgoData.Find(delegate(ConveyorWgoData x)
		{
			if (x != null)
			{
				ConveyorComponent conveyorComponent = x.ConveyorComponent;
				return conveyorComponent is ConveyorCellComponent || conveyorComponent is ConveyorCellUndergroundComponent;
			}
			return false;
		});
		int rotationIndex = base.WgoData.MainWgoPartData.rotationIndex;
		bool flag = false;
		bool flag2 = conveyorWgoData != null && conveyorWgoData.MainWgoPartData.rotationIndex == rotationIndex;
		if (parentsUniqueIds.Count > 0)
		{
			foreach (ConveyorWgoData value in base.ParentsData.Values)
			{
				switch (value.Definition.conveyorType)
				{
				case ConveyorElementType.Cell:
				case ConveyorElementType.UndergroundCell:
					if (value.MainWgoPartData.rotationIndex == rotationIndex)
					{
						flag = true;
					}
					break;
				case ConveyorElementType.Splitter:
					if (value.MainWgoPartData.rotationIndex % 2 == base.WgoData.MainWgoPartData.rotationIndex % 2)
					{
						flag = true;
					}
					break;
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (flag && flag2)
		{
			base.WgoData.ApplyWgoPartState("centre", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else if (!flag && flag2)
		{
			base.WgoData.ApplyWgoPartState("start", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else if (flag)
		{
			base.WgoData.ApplyWgoPartState("end", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else
		{
			base.WgoData.ApplyWgoPartState("single", base.WgoData.MainWgoPartData.rotationIndex);
		}
	}

	public override void AddParentData(ConveyorWgoData conveyorWgoData, Direction direction)
	{
		base.AddParentData(conveyorWgoData, direction);
		UpdateWgoPartState();
	}

	public override void RemoveParentData(ConveyorWgoData conveyorWgoData)
	{
		base.RemoveParentData(conveyorWgoData);
		UpdateWgoPartState();
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
		if (wasPerformedItemTransfer)
		{
			return false;
		}
		bool result = false;
		foreach (Item item in base.WgoData.Inventory.Data.Inventory)
		{
			if (conveyorComponent.WgoData.Definition.inventoryWhiteList.Contains(item.Definition) && !conveyorComponent.WgoData.Definition.inventoryBlackList.Contains(item.Definition))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void UpdateInOutItemData(ConveyorWgoData giver)
	{
		TryGetParentConnectionDirection(giver.UniqueId, out var direction);
		if (giver.Definition.conveyorType == ConveyorElementType.Chest || giver.Definition.conveyorType == ConveyorElementType.ChestOut || giver.Definition.conveyorType == ConveyorElementType.Workbench)
		{
			giver.ConveyorComponent.OutItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: false);
			base.InItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: false);
		}
		else if (giver.Definition.conveyorType == ConveyorElementType.Splitter)
		{
			giver.ConveyorComponent.OutItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: true);
			base.InItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: true);
		}
		else
		{
			giver.ConveyorComponent.OutItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: true);
			base.InItem = new ConveyorMovableItemData(base.WgoData.Inventory.Data.Inventory[0].id, direction, isCommon: true);
		}
	}
}
