using System;
using Unity.Netcode;
using UnityEngine;

public class ConnectionNotificationManager : MonoBehaviour
{
	public enum ConnectionStatus
	{
		Connected,
		Disconnected
	}

	public static ConnectionNotificationManager Singleton { get; internal set; }

	public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;

	private void Awake()
	{
		if (Singleton != null)
		{
			throw new Exception("Detected more than one instance of ConnectionNotificationManager! Do you have more than one component attached to a GameObject");
		}
		Singleton = this;
	}

	private void Start()
	{
		if (!(Singleton != this))
		{
			if (NetworkManager.Singleton == null)
			{
				throw new Exception("There is no NetworkManager for the ConnectionNotificationManager to do stuff with! Please add a NetworkManager to the scene.");
			}
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
		}
	}

	private void OnDestroy()
	{
		if (NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
		}
	}

	private void OnClientConnectedCallback(ulong clientId)
	{
		this.OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Connected);
	}

	private void OnClientDisconnectCallback(ulong clientId)
	{
		this.OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Disconnected);
	}
}
