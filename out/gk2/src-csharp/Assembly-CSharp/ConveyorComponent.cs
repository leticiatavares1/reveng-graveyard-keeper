using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConveyorComponent
{
	public bool wasPerformedItemTransfer;

	public SGuid wgoDataUniqueId;

	public List<Direction> occupiedConnectorsDirections = new List<Direction>();

	private ConveyorMovableItemData outItem;

	private ConveyorMovableItemData inItem;

	[SerializeField]
	protected List<SGuid> parentsUniqueIds = new List<SGuid>();

	[SerializeField]
	protected List<ConveyorConnectionData> parentsConnectionsData = new List<ConveyorConnectionData>();

	private Dictionary<SGuid, ConveyorWgoData> parentsData = new Dictionary<SGuid, ConveyorWgoData>();

	private ConveyorWgoData wgoData;

	public VisitState CurrentVisitState { get; set; }

	public ConveyorWgoData WgoData
	{
		get
		{
			if (wgoData != null)
			{
				return wgoData;
			}
			if (wgoDataUniqueId == null)
			{
				return null;
			}
			wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoDataUniqueId) as ConveyorWgoData;
			return wgoData;
		}
		set
		{
			wgoData = value;
			wgoDataUniqueId = wgoData.UniqueId;
		}
	}

	public Dictionary<SGuid, ConveyorWgoData> ParentsData
	{
		get
		{
			if (parentsData == null)
			{
				parentsData = new Dictionary<SGuid, ConveyorWgoData>();
			}
			if (parentsUniqueIds.Count > 0 && parentsData.Count == 0)
			{
				foreach (SGuid parentsUniqueId in parentsUniqueIds)
				{
					parentsData.TryAdd(parentsUniqueId, MainGame.Instance.GameSave.worldData.GetWgoData(parentsUniqueId) as ConveyorWgoData);
				}
			}
			return parentsData;
		}
	}

	public List<ConveyorConnectionData> ParentsConnectionsData
	{
		get
		{
			if (parentsConnectionsData == null)
			{
				parentsConnectionsData = new List<ConveyorConnectionData>();
			}
			return parentsConnectionsData;
		}
	}

	public ConveyorMovableItemData OutItem
	{
		get
		{
			return outItem;
		}
		set
		{
			outItem = value;
		}
	}

	public ConveyorMovableItemData InItem
	{
		get
		{
			return inItem;
		}
		set
		{
			inItem = value;
		}
	}

	public event Action OnConnected;

	public event Action OnDisconnected;

	public ConveyorComponent(ConveyorWgoData parentWgoData)
	{
		wgoDataUniqueId = parentWgoData.UniqueId;
		wgoData = parentWgoData;
	}

	public virtual void Init()
	{
	}

	public virtual void DeInit()
	{
	}

	public virtual void GetEndElement(ref List<ConveyorComponent> endElements)
	{
	}

	public virtual void PerformItemTransfer()
	{
	}

	public virtual void DoJob(ConveyorComponent visitor = null)
	{
	}

	public virtual bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction = Direction.None)
	{
		Debug.Log("Conveyer Wgo data connected");
		return true;
	}

	public virtual bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction, int slotIndex)
	{
		return Connect(conveyorWgoData, connectionType, direction);
	}

	public virtual bool Disconnect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType)
	{
		Debug.Log("Conveyer Wgo data disconnected");
		return true;
	}

	public virtual bool DisconnectChilds()
	{
		return true;
	}

	public virtual bool Contains(ConveyorComponent conveyorComponent)
	{
		return false;
	}

	public virtual void UpdateWgoPartState()
	{
	}

	public virtual void AddParentData(ConveyorWgoData conveyorWgoData, Direction direction)
	{
		if (!ParentsData.TryGetValue(conveyorWgoData.UniqueId, out var _))
		{
			ParentsData.Add(conveyorWgoData.UniqueId, conveyorWgoData);
			parentsUniqueIds.Add(conveyorWgoData.UniqueId);
			ParentsConnectionsData.Add(new ConveyorConnectionData(conveyorWgoData.UniqueId, direction));
		}
	}

	public virtual void RemoveParentData(ConveyorWgoData conveyorWgoData)
	{
		if (ParentsData.TryGetValue(conveyorWgoData.UniqueId, out var _))
		{
			ParentsData.Remove(conveyorWgoData.UniqueId);
			parentsUniqueIds.Remove(conveyorWgoData.UniqueId);
			ParentsConnectionsData.RemoveAll((ConveyorConnectionData x) => x.connectedUniqueId == conveyorWgoData.UniqueId);
		}
	}

	public void RemoveParentLink(SGuid parentUniqueId)
	{
		if (!SGuid.IsNullOrEmpty(parentUniqueId))
		{
			parentsData?.Remove(parentUniqueId);
			parentsUniqueIds?.Remove(parentUniqueId);
			parentsConnectionsData?.RemoveAll((ConveyorConnectionData x) => x.connectedUniqueId == parentUniqueId);
		}
	}

	public bool TryGetParentConnectionDirection(SGuid parentUniqueId, out Direction direction)
	{
		ConveyorConnectionData conveyorConnectionData = ParentsConnectionsData.Find((ConveyorConnectionData x) => x.connectedUniqueId == parentUniqueId);
		direction = conveyorConnectionData?.connectionDirection ?? Direction.Down;
		return conveyorConnectionData != null;
	}

	public virtual void HandleCycleDependency(ConveyorComponent inputComponent)
	{
	}

	public virtual bool CanGiveItem(ConveyorComponent conveyorComponent)
	{
		return false;
	}

	public virtual bool CanBeVisitedBy(ConveyorComponent conveyorComponent)
	{
		return CurrentVisitState == VisitState.NotVisited;
	}

	public virtual List<Item> GiveItem(ConveyorComponent reciever)
	{
		return WgoData.Inventory.RemoveItemById(WgoData.Inventory.Data.Inventory[0].id, 1);
	}

	public virtual void ClearInAndOutItemDatas()
	{
		outItem = null;
		inItem = null;
	}

	public virtual bool HasChildsInDirection(Direction direction)
	{
		return false;
	}

	public virtual bool HasParentsInDirection(Direction direction)
	{
		return false;
	}

	public void OnConnectedEvent()
	{
		this.OnConnected?.Invoke();
	}

	public void OnDisconnectedEvent()
	{
		this.OnDisconnected?.Invoke();
	}
}
