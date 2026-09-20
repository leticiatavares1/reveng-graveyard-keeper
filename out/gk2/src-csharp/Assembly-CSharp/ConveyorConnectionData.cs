using System;

[Serializable]
public class ConveyorConnectionData
{
	public SGuid connectedUniqueId;

	public Direction connectionDirection;

	public ConveyorConnectionData(SGuid connectedUniqueId, Direction connectionDirection)
	{
		this.connectedUniqueId = connectedUniqueId;
		this.connectionDirection = connectionDirection;
	}
}
