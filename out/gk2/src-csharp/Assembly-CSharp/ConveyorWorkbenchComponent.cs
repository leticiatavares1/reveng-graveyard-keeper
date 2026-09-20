using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[Serializable]
public class ConveyorWorkbenchComponent : ConveyorComponent
{
	public List<SGuid> connectedInWgoDataUniqueId = new List<SGuid>();

	public List<SGuid> connectedOutWgoDataUniqueId = new List<SGuid>();

	[SerializeField]
	private List<ConveyorConnectionData> occupiedConnectorsData = new List<ConveyorConnectionData>();

	[NonSerialized]
	private List<ConveyorWgoData> conveyorInWgoDataList = new List<ConveyorWgoData>();

	[NonSerialized]
	private List<ConveyorWgoData> conveyorOutWgoDataList = new List<ConveyorWgoData>();

	public List<ConveyorWgoData> ConveyorInWgoDataList
	{
		get
		{
			if (conveyorInWgoDataList == null)
			{
				conveyorInWgoDataList = new List<ConveyorWgoData>();
			}
			if (conveyorInWgoDataList.Count == 0 && connectedInWgoDataUniqueId.Count > 0)
			{
				for (int i = 0; i < connectedInWgoDataUniqueId.Count; i++)
				{
					if (!(MainGame.Instance.GameSave.worldData.GetWgoData(connectedInWgoDataUniqueId[i]) is ConveyorWgoData item))
					{
						Debug.LogError($"Conveyor workbench [{base.WgoData.id}]. Can not find Connected In WGO with UniqueId [{connectedInWgoDataUniqueId[i]}]");
					}
					else
					{
						conveyorInWgoDataList.Add(item);
					}
				}
			}
			return conveyorInWgoDataList;
		}
	}

	private List<ConveyorWgoData> ConveyorOutWgoDataList
	{
		get
		{
			if (conveyorOutWgoDataList == null)
			{
				conveyorOutWgoDataList = new List<ConveyorWgoData>();
			}
			if (conveyorOutWgoDataList.Count == 0 && connectedOutWgoDataUniqueId.Count > 0)
			{
				for (int i = 0; i < connectedOutWgoDataUniqueId.Count; i++)
				{
					if (MainGame.Instance.GameSave.worldData.GetWgoData(connectedOutWgoDataUniqueId[i]) is ConveyorWgoData item)
					{
						conveyorOutWgoDataList.Add(item);
					}
				}
			}
			return conveyorOutWgoDataList;
		}
	}

	private List<ConveyorConnectionData> OccupiedConnectorsData
	{
		get
		{
			if (occupiedConnectorsData == null)
			{
				occupiedConnectorsData = new List<ConveyorConnectionData>();
			}
			return occupiedConnectorsData;
		}
	}

	public ConveyorWorkbenchComponent(ConveyorWgoData conveyorWgoData)
		: base(conveyorWgoData)
	{
	}

	public override void Init()
	{
		base.WgoData.CraftComponent.OnStatusChanged += DoJobOutForce;
	}

	public override void DeInit()
	{
		base.WgoData.CraftComponent.OnStatusChanged -= DoJobOutForce;
	}

	public override void DoJob(ConveyorComponent visitor = null)
	{
		base.CurrentVisitState = VisitState.Visited;
	}

	public override void PerformItemTransfer()
	{
	}

	public override bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction = Direction.None)
	{
		if (connectedOutWgoDataUniqueId.Contains(conveyorWgoData.UniqueId) || connectedInWgoDataUniqueId.Contains(conveyorWgoData.UniqueId))
		{
			Debug.LogWarning($"Trying to connect {conveyorWgoData.UniqueId} that is already connected");
			return false;
		}
		ConveyorComponent conveyorComponent = conveyorWgoData.ConveyorComponent;
		if (!(conveyorComponent is ConveyorCellComponent) && !(conveyorComponent is ConveyorSplitterComponent))
		{
			return false;
		}
		Debug.Log($"Connected {conveyorWgoData.id} with guid {conveyorWgoData.UniqueId} as {connectionType} to {base.WgoData.id} with guid {wgoDataUniqueId}");
		switch (connectionType)
		{
		case ConveyorConnectionType.In:
			connectedInWgoDataUniqueId.Add(conveyorWgoData.UniqueId);
			if (!ConveyorInWgoDataList.Contains(conveyorWgoData))
			{
				ConveyorInWgoDataList.Add(conveyorWgoData);
			}
			break;
		case ConveyorConnectionType.Out:
			connectedOutWgoDataUniqueId.Add(conveyorWgoData.UniqueId);
			if (!ConveyorOutWgoDataList.Contains(conveyorWgoData))
			{
				ConveyorOutWgoDataList.Add(conveyorWgoData);
			}
			break;
		}
		OccupiedConnectorsData.Add(new ConveyorConnectionData(conveyorWgoData.UniqueId, direction));
		occupiedConnectorsDirections.Add(direction);
		OnConnectedEvent();
		return true;
	}

	public override bool Disconnect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType)
	{
		if (!connectedOutWgoDataUniqueId.Contains(conveyorWgoData.UniqueId) && !connectedInWgoDataUniqueId.Contains(conveyorWgoData.UniqueId))
		{
			Debug.Log($"Trying to disconnect {conveyorWgoData.UniqueId} that is not connected");
			return false;
		}
		Debug.Log($"Disconnected {conveyorWgoData.id} with guid {conveyorWgoData.UniqueId} as {connectionType} from {base.WgoData.id} with guid {wgoDataUniqueId}");
		switch (connectionType)
		{
		case ConveyorConnectionType.In:
			connectedInWgoDataUniqueId.Remove(conveyorWgoData.UniqueId);
			ConveyorInWgoDataList.Remove(conveyorWgoData);
			break;
		case ConveyorConnectionType.Out:
			connectedOutWgoDataUniqueId.Remove(conveyorWgoData.UniqueId);
			ConveyorOutWgoDataList.Remove(conveyorWgoData);
			break;
		}
		if (TryGetConnectorDirection(conveyorWgoData.UniqueId, out var direction))
		{
			OccupiedConnectorsData.RemoveAll((ConveyorConnectionData x) => x.connectedUniqueId == conveyorWgoData.UniqueId);
			occupiedConnectorsDirections.Remove(direction);
		}
		OnDisconnectedEvent();
		return true;
	}

	public override bool DisconnectChilds()
	{
		foreach (ConveyorWgoData conveyorInWgoData in ConveyorInWgoDataList)
		{
			if (conveyorInWgoData != null)
			{
				Debug.Log($"Disconnected {conveyorInWgoData.id} with guid {conveyorInWgoData.UniqueId} as {ConveyorConnectionType.In} from {base.WgoData.id} with guid {wgoDataUniqueId}");
				conveyorInWgoData.ConveyorComponent.Disconnect(base.WgoData, ConveyorConnectionType.In);
				conveyorInWgoData.ConveyorComponent.RemoveParentData(base.WgoData);
			}
		}
		foreach (ConveyorWgoData conveyorOutWgoData in ConveyorOutWgoDataList)
		{
			if (conveyorOutWgoData != null)
			{
				Debug.Log($"Disconnected {conveyorOutWgoData.id} with guid {conveyorOutWgoData.UniqueId} as {ConveyorConnectionType.Out} from {base.WgoData.id} with guid {wgoDataUniqueId}");
				conveyorOutWgoData.ConveyorComponent.RemoveParentData(base.WgoData);
			}
		}
		ConveyorOutWgoDataList.Clear();
		connectedOutWgoDataUniqueId.Clear();
		ConveyorInWgoDataList.Clear();
		connectedInWgoDataUniqueId.Clear();
		return true;
	}

	public override void GetEndElement(ref List<ConveyorComponent> endElements)
	{
		base.CurrentVisitState = VisitState.Visiting;
		if (ConveyorOutWgoDataList.Count == 0)
		{
			endElements.Add(this);
		}
		foreach (ConveyorWgoData conveyorOutWgoData in ConveyorOutWgoDataList)
		{
			ConveyorComponent conveyorComponent = conveyorOutWgoData.ConveyorComponent;
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
		foreach (ConveyorWgoData conveyorInWgoData in ConveyorInWgoDataList)
		{
			conveyorInWgoData.ConveyorComponent.HandleCycleDependency(cyrcleComponent);
		}
	}

	private void GetItemFromConveyor(ConveyorWgoData conveyorCell)
	{
		if (base.WgoData.CraftComponent.Status == CraftComponentStatus.WaitingForOutputDrop || !conveyorCell.ConveyorComponent.CanGiveItem(this) || !base.WgoData.CraftComponent.HasCraftsInQueue || base.WgoData.CraftComponent.IsStarted)
		{
			return;
		}
		List<NeedItemData> requirements = base.WgoData.CraftComponent.CraftElementsQueue[0].Requirements;
		base.WgoData.CraftComponent.UpdateQueueElementsCraftStatus();
		if (base.WgoData.CraftComponent.CraftElementsQueue[0].CraftStatus != CraftStatus.NotEnoughResources)
		{
			return;
		}
		foreach (NeedItemData item in requirements)
		{
			if (!base.WgoData.CraftableObjectCraftInventory.Data.HasItemQuantityInInventory(item.id, item.GetCount(base.WgoData)) && conveyorCell.Inventory.Data.HasItemQuantityInInventory(item.id, 1))
			{
				conveyorCell.ConveyorComponent.OutItem = new ConveyorMovableItemData(item.id, conveyorCell.MainWgoPartData.rotationIndex, isCommon: false);
				base.WgoData.CraftableObjectCraftInventory.AddItemsToInventory(conveyorCell.Inventory.RemoveItemById(item.id, 1));
				wasPerformedItemTransfer = true;
			}
		}
	}

	private void PutItemToConveyor(ConveyorWgoData conveyorCell)
	{
		if (base.WgoData.CraftComponent.Status == CraftComponentStatus.WaitingForOutputDrop)
		{
			if (conveyorCell.Inventory.Data.Inventory.Count == 0)
			{
				TryGetConnectorDirection(conveyorCell.UniqueId, out var direction);
				conveyorCell.ConveyorComponent.InItem = new ConveyorMovableItemData(base.WgoData.CraftableObjectCraftInventory.Data.Inventory[0].id, direction, isCommon: false);
				conveyorCell.Inventory.AddItemsToInventory(base.WgoData.CraftableObjectCraftInventory.RemoveItemById(base.WgoData.CraftableObjectCraftInventory.Data.Inventory[0].id, 1));
				wasPerformedItemTransfer = true;
			}
			if (base.WgoData.Definition.isAutoCrafter && base.WgoData.CraftableObjectCraftInventory.Data.Inventory.Count == 0)
			{
				base.WgoData.CraftComponent.Status = CraftComponentStatus.ReadyToStartCraft;
			}
		}
	}

	public void DoJobIn()
	{
		if (base.WgoData.CraftComponent.IsDestroyingCraftActive)
		{
			return;
		}
		foreach (ConveyorWgoData conveyorInWgoData in ConveyorInWgoDataList)
		{
			GetItemFromConveyor(conveyorInWgoData);
		}
	}

	public void DoJobOut()
	{
		if (base.WgoData.CraftComponent.IsDestroyingCraftActive)
		{
			return;
		}
		foreach (ConveyorWgoData conveyorOutWgoData in ConveyorOutWgoDataList)
		{
			PutItemToConveyor(conveyorOutWgoData);
		}
		if (wasPerformedItemTransfer)
		{
			if (base.WgoData.CraftableObjectCraftInventory.Data.Inventory.Count == 0)
			{
				TryFinishNormalCraft();
				TryStartOrFinishAutoCraft();
			}
		}
		else if (base.WgoData.CraftableObjectCraftInventory.Data.Inventory.Count != 0 || (base.WgoData.Definition.isAutoCrafter && base.WgoData.CraftComponent.Status == CraftComponentStatus.WaitingForOutputDrop))
		{
			base.WgoData.CraftComponent.CurrentCraftElement.CraftStatus = CraftStatus.NotEnoughSpaceInWgo;
			base.WgoData.UpdateAttachedWgoViewWidgets();
		}
		else
		{
			TryStartOrFinishAutoCraft();
		}
	}

	public void DoJobOutForce(CraftComponentStatus craftComponentStatus)
	{
		if (craftComponentStatus == CraftComponentStatus.WaitingForOutputDrop)
		{
			DoJobOut();
		}
	}

	public override bool CanGiveItem(ConveyorComponent conveyorComponent)
	{
		return false;
	}

	private bool TryGetConnectorDirection(SGuid connectedUniqueId, out Direction direction)
	{
		ConveyorConnectionData conveyorConnectionData = OccupiedConnectorsData.Find((ConveyorConnectionData x) => x.connectedUniqueId == connectedUniqueId);
		direction = conveyorConnectionData?.connectionDirection ?? Direction.Down;
		return conveyorConnectionData != null;
	}

	private void TryFinishNormalCraft()
	{
		if (!base.WgoData.Definition.isAutoCrafter)
		{
			base.WgoData.CraftComponent.TryFinishCurCraft();
		}
	}

	private void TryStartOrFinishAutoCraft()
	{
		if (!base.WgoData.Definition.isAutoCrafter)
		{
			return;
		}
		if (!base.WgoData.CraftComponent.HasCraftsInQueue)
		{
			if (GameBalance.Me.craftsInCache.TryGetValue(base.WgoData.id, out var value))
			{
				base.WgoData.CraftComponent.AddToQueue(new CraftElement(value[0].id, 1, new List<NeedItemData>(value[0].needItems), new CraftParamsData(value[0].id, base.WgoData)));
			}
		}
		else if (base.WgoData.CraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
		{
			IWorker worker = base.WgoData.Worker;
			bool flag = false;
			if (worker == null)
			{
				flag = true;
				base.WgoData.TrySetWorker(MainGame.PlayerController);
			}
			base.WgoData.CraftComponent.ContinueAutoCraft();
			if (flag)
			{
				base.WgoData.ClearWorker();
			}
			if (GameBalance.Me.craftsInCache.TryGetValue(base.WgoData.id, out var value2))
			{
				base.WgoData.CraftComponent.AddToQueue(new CraftElement(value2[0].id, 1, new List<NeedItemData>(value2[0].needItems), new CraftParamsData(value2[0].id, base.WgoData)));
			}
			base.WgoData.CraftComponent.Status = CraftComponentStatus.WaitingForOutputDrop;
		}
	}
}
