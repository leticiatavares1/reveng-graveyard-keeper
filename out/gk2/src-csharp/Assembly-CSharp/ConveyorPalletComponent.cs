using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConveyorPalletComponent : ConveyorComponent
{
	[SerializeField]
	private SGuid connectedWgoDataUniqueId;

	private ConveyorWgoData connectedWgoData;

	public ConveyorWgoData ConnectedWgoData
	{
		get
		{
			if (connectedWgoData != null)
			{
				return connectedWgoData;
			}
			if (connectedWgoDataUniqueId == null)
			{
				return null;
			}
			connectedWgoData = MainGame.Instance.GameSave.worldData.GetWgoData(connectedWgoDataUniqueId) as ConveyorWgoData;
			return connectedWgoData;
		}
	}

	public ConveyorPalletComponent(ConveyorWgoData conveyorWgoData)
		: base(conveyorWgoData)
	{
	}

	public override void DoJob(ConveyorComponent visitor = null)
	{
		base.CurrentVisitState = VisitState.Visited;
	}

	public override bool Connect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType, Direction direction = Direction.None)
	{
		if (connectedWgoDataUniqueId != null && connectedWgoDataUniqueId == conveyorWgoData.UniqueId)
		{
			Debug.LogWarning($"Trying to connect  {conveyorWgoData.UniqueId} that is already connected");
			return false;
		}
		if (!(conveyorWgoData.ConveyorComponent is ConveyorCellComponent))
		{
			return false;
		}
		Debug.Log($"Connected {conveyorWgoData.id} with guid {conveyorWgoData.UniqueId} to {base.WgoData.id} with guid {wgoDataUniqueId}");
		connectedWgoDataUniqueId = conveyorWgoData.UniqueId;
		conveyorWgoData.ConveyorComponent.AddParentData(base.WgoData, direction);
		return true;
	}

	public override bool Disconnect(ConveyorWgoData conveyorWgoData, ConveyorConnectionType connectionType)
	{
		if (connectedWgoDataUniqueId == null)
		{
			Debug.LogWarning($"Trying to disconnect  {conveyorWgoData.UniqueId}. But nothing is connected");
			return false;
		}
		if (connectedWgoDataUniqueId != null && connectedWgoDataUniqueId != conveyorWgoData.UniqueId)
		{
			Debug.LogWarning($"Trying to disconnect  {conveyorWgoData.UniqueId} that is not connected");
			return false;
		}
		Debug.Log($"Disconnected {conveyorWgoData.id} with guid {conveyorWgoData.UniqueId} from {base.WgoData.id} with guid {wgoDataUniqueId}");
		connectedWgoDataUniqueId = null;
		connectedWgoData = null;
		conveyorWgoData.ConveyorComponent.RemoveParentData(base.WgoData);
		return true;
	}

	public override bool DisconnectChilds()
	{
		if (connectedWgoDataUniqueId != null)
		{
			if (ConnectedWgoData != null)
			{
				Debug.Log($"Disconnected {ConnectedWgoData.id} with guid {connectedWgoDataUniqueId} from {base.WgoData.id} with guid {wgoDataUniqueId}");
				ConnectedWgoData.ConveyorComponent.RemoveParentData(base.WgoData);
			}
			connectedWgoDataUniqueId = null;
			connectedWgoData = null;
			return true;
		}
		return false;
	}

	public override bool Contains(ConveyorComponent conveyorComponent)
	{
		if (ConnectedWgoData == conveyorComponent.WgoData)
		{
			return true;
		}
		return false;
	}

	public override void GetEndElement(ref List<ConveyorComponent> endElements)
	{
		base.CurrentVisitState = VisitState.Visiting;
		if (ConnectedWgoData == null)
		{
			endElements.Add(this);
		}
		else
		{
			ConveyorComponent conveyorComponent = ConnectedWgoData.ConveyorComponent;
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
		return base.WgoData.Inventory.Data.Inventory.Count > 0;
	}
}
