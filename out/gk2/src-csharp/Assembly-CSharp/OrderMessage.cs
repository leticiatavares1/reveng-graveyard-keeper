using System;

[Serializable]
public class OrderMessage
{
	public OrderMessageType messageType;

	public int lastPackageIdReceived;

	public OrderMessage()
	{
	}

	public OrderMessage(OrderMessageType messageType)
	{
		this.messageType = messageType;
	}

	public void Execute(ulong senderClientId)
	{
		switch (messageType)
		{
		case OrderMessageType.RequestPackages:
			LazyNetwork.ConnectionManager.NetworkPackageSystem.SendMissingPackages(senderClientId, (ulong)lastPackageIdReceived);
			break;
		case OrderMessageType.ForceStartGame:
			LobbyHelper.Client_StartGame();
			break;
		case OrderMessageType.ConfirmGameSave:
			LobbyHelper.Host_UpdateClientSyncStatus(senderClientId);
			break;
		}
	}
}
