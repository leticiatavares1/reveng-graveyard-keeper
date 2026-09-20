using System.Collections.Generic;
using UnityEngine;

public abstract class ConnectionState
{
	protected ConnectionManager connectionManager;

	protected List<ulong> destinationClients = new List<ulong>();

	public List<ulong> DestinationClients => destinationClients;

	public ConnectionState(ConnectionManager connectionManager)
	{
		this.connectionManager = connectionManager;
	}

	public abstract void SendNetworkData<T>(T data);

	public abstract void ReceiveNetworkData<T>(T data, ulong senderClientId);

	public abstract void Enter();

	public abstract void Update();

	public abstract void Exit();

	protected void AddDestinationClient(ulong clientId)
	{
		if (destinationClients.Contains(clientId))
		{
			Debug.LogWarning($"Client [{clientId}] was already added");
			return;
		}
		destinationClients.Add(clientId);
		Debug.Log($"Destination client {clientId} was added");
	}

	protected void RemoveDestinationClient(ulong clientId)
	{
		destinationClients.Remove(clientId);
		Debug.Log($"Destination client {clientId} was removed");
	}
}
