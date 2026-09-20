using Unity.Netcode;

public abstract class UNetworkMessageChannel<T> : NetworkMessageChannel<T>
{
	protected NetworkDelivery networkDelivery = NetworkDelivery.ReliableSequenced;

	protected string name;

	public UNetworkMessageChannel()
	{
		name = GetType().Name;
	}

	protected void SendMessage(FastBufferWriter writer, ulong customClientId)
	{
		if (LazyNetwork.NetworkManager.IsHost)
		{
			if (customClientId == 0L)
			{
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, LazyNetwork.ConnectionManager.CurrentState.DestinationClients, writer, networkDelivery);
			}
			else
			{
				NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, customClientId, writer, networkDelivery);
			}
		}
		else
		{
			NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, LazyNetwork.ConnectionManager.CurrentState.DestinationClients[0], writer, networkDelivery);
		}
	}

	public override void Register()
	{
		NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(name, OnReceiveInternal);
	}

	public override void Unregister()
	{
		NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(name);
	}

	public void SetNetworkDelivery(NetworkDelivery networkDelivery)
	{
		this.networkDelivery = networkDelivery;
	}

	private void OnReceiveInternal(ulong senderClientId, FastBufferReader messagePayload)
	{
		OnReceive(senderClientId, messagePayload);
	}
}
