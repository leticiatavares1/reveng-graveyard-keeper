using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ConveyorSplitterComponent : ConveyorComponent
{
	private const int MAX_CHILDREN_COUNT = 2;

	public List<SGuid> connectedWgoDataUniqueId = new List<SGuid>();

	[SerializeField]
	private int currentTransferIndexFrom;

	[SerializeField]
	private int currentChildTransferIndex;

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

	public ConveyorSplitterComponent(ConveyorWgoData conveyorWgoData)
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
			currentTransferIndexFrom = MathUtilities.ClampCycle(currentTransferIndexFrom, 0, list.Count - 1);
			ConveyorWgoData conveyorWgoData = list[currentTransferIndexFrom];
			currentTransferIndexFrom++;
			if (!conveyorWgoData.CraftComponent.IsDestroyingCraftActive && conveyorWgoData.ConveyorComponent.CanGiveItem(this))
			{
				ConveyorComponent conveyorComponent = conveyorWgoData.ConveyorComponent;
				if ((conveyorComponent is ConveyorCellComponent || conveyorComponent is ConveyorCellUndergroundComponent || conveyorComponent is ConveyorPalletComponent || conveyorComponent is ConveyorChestComponent || conveyorComponent is ConveyorSplitterComponent) && base.WgoData.Inventory.Data.Inventory.Count == 0)
				{
					base.WgoData.Inventory.AddItemsToInventory(conveyorWgoData.ConveyorComponent.GiveItem(this));
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
		if (!(conveyorComponent is ConveyorCellComponent) && !(conveyorComponent is ConveyorChestComponent) && !(conveyorComponent is ConveyorSplitterComponent) && !(conveyorComponent is ConveyorCellUndergroundComponent))
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
		int rotationIndex = base.WgoData.MainWgoPartData.rotationIndex;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		foreach (ConveyorWgoData connectedWgoDatum in ConnectedWgoData)
		{
			ConveyorComponent conveyorComponent = connectedWgoDatum?.ConveyorComponent;
			if (!(conveyorComponent is ConveyorCellComponent) && !(conveyorComponent is ConveyorCellUndergroundComponent))
			{
				continue;
			}
			switch (rotationIndex)
			{
			case 0:
				if (!flag3)
				{
					flag3 = connectedWgoDatum.MainWgoPartData.rotationIndex == 2;
				}
				if (!flag4)
				{
					flag4 = connectedWgoDatum.MainWgoPartData.rotationIndex == 0;
				}
				break;
			case 1:
				if (!flag2)
				{
					flag2 = connectedWgoDatum.MainWgoPartData.rotationIndex == 1;
				}
				if (!flag)
				{
					flag = connectedWgoDatum.MainWgoPartData.rotationIndex == 3;
				}
				break;
			}
		}
		if ((flag3 && flag4) || (flag2 && flag))
		{
			base.WgoData.ApplyWgoPartState("centre", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else if (flag3)
		{
			base.WgoData.ApplyWgoPartState("centre_down_end", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else if (flag4)
		{
			base.WgoData.ApplyWgoPartState("centre_up_end", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else if (flag2)
		{
			base.WgoData.ApplyWgoPartState("centre_right_end", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else if (flag)
		{
			base.WgoData.ApplyWgoPartState("centre_left_end", base.WgoData.MainWgoPartData.rotationIndex);
		}
		else
		{
			base.WgoData.ApplyWgoPartState("end", base.WgoData.MainWgoPartData.rotationIndex);
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
		if (ConnectedWgoData.Count == 2 && ConnectedWgoData[currentChildTransferIndex] == conveyorComponent.WgoData)
		{
			return base.WgoData.Inventory.Data.Inventory.Count > 0;
		}
		if (ConnectedWgoData.Count == 1 && ConnectedWgoData[0] == conveyorComponent.WgoData)
		{
			return base.WgoData.Inventory.Data.Inventory.Count > 0;
		}
		return false;
	}

	public void SwitchDirection()
	{
		currentChildTransferIndex++;
		currentChildTransferIndex = MathUtilities.ClampCycle(currentChildTransferIndex, 0, 1);
	}

	public override bool CanBeVisitedBy(ConveyorComponent conveyorComponent)
	{
		if (conveyorComponent == null)
		{
			return base.CurrentVisitState == VisitState.NotVisited;
		}
		if (ConnectedWgoData.Count == 2 && conveyorComponent.WgoData == ConnectedWgoData[currentChildTransferIndex])
		{
			return true;
		}
		if (ConnectedWgoData.Count == 1 && ConnectedWgoData[0] == conveyorComponent.WgoData)
		{
			return true;
		}
		return false;
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
