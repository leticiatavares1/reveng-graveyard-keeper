using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConveyorChestOutComponent : ConveyorComponent
{
	private const string GARDEN_BAGS_STORAGE1_ID = "garden_bags_storage_1";

	private const string GARDEN_BAGS_STORAGE2_ID = "garden_bags_storage_2";

	private const string GARDEN_BAGS_STORAGE3_ID = "garden_bags_storage_3";

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
				UpdateSlotsData();
			}
			return slotsData;
		}
	}

	public ConveyorChestOutComponent(ConveyorWgoData conveyorWgoData)
		: base(conveyorWgoData)
	{
	}

	public override void DoJob(ConveyorComponent visitor = null)
	{
		base.CurrentVisitState = VisitState.Visited;
	}

	public override bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction = Direction.None)
	{
		return Connect(conveyorWgoData, connectionType, direction, -1);
	}

	public override bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction, int slotIndex)
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
		UpdateSlotsData();
		ConveyorChestSlotData conveyorChestSlotData = ((slotIndex > 0) ? SlotsData.Find((ConveyorChestSlotData x) => x.slotIndex == slotIndex) : SlotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == direction && x.conveyorWgoDataUniqueId == null));
		if (conveyorChestSlotData == null || conveyorChestSlotData.conveyorWgoDataUniqueId != null)
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
		foreach (ConveyorChestSlotData slotsDatum in SlotsData)
		{
			bool num;
			if (slotIndex <= 0)
			{
				if (slotsDatum.slotPosDirection != direction)
				{
					continue;
				}
				num = slotsDatum.conveyorWgoDataUniqueId == null;
			}
			else
			{
				num = slotsDatum.slotIndex == slotIndex;
			}
			if (num)
			{
				slotsDatum.conveyorWgoDataUniqueId = conveyorWgoData.UniqueId;
				if (slotIndex > 0)
				{
					slotsDatum.slotIndex = slotIndex;
				}
				break;
			}
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

	public override bool HasChildsInDirection(Direction direction)
	{
		foreach (ConveyorChestSlotData slotsDatum in SlotsData)
		{
			if (slotsDatum.slotPosDirection == direction && !SGuid.IsNullOrEmpty(slotsDatum.conveyorWgoDataUniqueId))
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateSlotsData()
	{
		ConveyorConnectorsSetup conveyorConnectorsSetup = base.WgoData.Definition.conveyorConnectorsSetup;
		if (conveyorConnectorsSetup != null && !conveyorConnectorsSetup.IsEmpty)
		{
			int slotsCount = GetSlotsCount(conveyorConnectorsSetup.slotsCount);
			Direction direction = conveyorConnectorsSetup.direction;
			if (slotsData == null || slotsData.Count != slotsCount)
			{
				int num = 0;
				if (slotsData != null)
				{
					num = slotsCount - slotsData.Count;
				}
				if (slotsData != null && num > 0)
				{
					for (int i = 0; i < num; i++)
					{
						slotsData.Add(new ConveyorChestSlotData(direction, slotsData.Count + 1));
					}
				}
				else
				{
					slotsData = new List<ConveyorChestSlotData>();
					for (int j = 0; j < slotsCount; j++)
					{
						slotsData.Add(new ConveyorChestSlotData(direction, j + 1));
					}
				}
			}
			for (int k = 0; k < slotsData.Count; k++)
			{
				slotsData[k].slotIndex = k + 1;
			}
		}
		else if (slotsData == null)
		{
			slotsData = new List<ConveyorChestSlotData>
			{
				new ConveyorChestSlotData(Direction.Left),
				new ConveyorChestSlotData(Direction.Up),
				new ConveyorChestSlotData(Direction.Right),
				new ConveyorChestSlotData(Direction.Down)
			};
		}
	}

	private int GetSlotsCount(int defaultSlotsCount)
	{
		return base.WgoData.id switch
		{
			"garden_bags_storage_1" => 2, 
			"garden_bags_storage_2" => 2, 
			"garden_bags_storage_3" => 3, 
			_ => defaultSlotsCount, 
		};
	}

	private bool TryGetItemIdToGive(ConveyorComponent conveyorComponent, out string itemId)
	{
		itemId = null;
		UpdateSlotsData();
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
