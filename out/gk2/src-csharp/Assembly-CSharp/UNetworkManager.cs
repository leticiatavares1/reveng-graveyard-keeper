using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class UNetworkManager : MonoBehaviour, INetworkManager
{
	[SerializeField]
	private UnityTransport networkTransport;

	[SerializeField]
	private int maxCommandPackageQueue = 15;

	private bool isManagerInitedCorrectly;

	private ulong serverClientId;

	private List<ulong> otherClients = new List<ulong>();

	private bool isCoopGame;

	public bool IsHost
	{
		get
		{
			if (IsCoopGame)
			{
				return NetworkManager.Singleton.IsHost;
			}
			return true;
		}
	}

	public bool IsClient
	{
		get
		{
			if (IsCoopGame)
			{
				return NetworkManager.Singleton.IsClient;
			}
			return false;
		}
	}

	public ulong MyId => NetworkManager.Singleton.LocalClientId;

	public bool IsCoopGame => isCoopGame;

	public int MaxCommandPackageQueue => maxCommandPackageQueue;

	public int MaxPayloadSize => networkTransport.MaxPayloadSize;

	public ulong ServerClientId => serverClientId;

	public List<ulong> OtherClients => otherClients;

	public event Action OnServerStarted;

	public event Action OnServerStopped;

	public event Action OnClientStarted;

	public event Action OnClientStopped;

	public event Action<ulong> OnClientConnected;

	public event Action<ulong> OnClientDisconnected;

	public void Init()
	{
		if (NetworkManager.Singleton == null)
		{
			Debug.LogError("NetworkManager was not initialized.");
			return;
		}
		if (networkTransport == null)
		{
			Debug.LogError("Transport protocol was not set.");
			return;
		}
		ConnectionNotificationManager.Singleton.OnClientConnectionNotification += NotifyConnectionStatus;
		isManagerInitedCorrectly = true;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted_Internal;
		NetworkManager.Singleton.OnServerStopped += OnServerStopped_Internal;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientStarted_Internal;
		NetworkManager.Singleton.OnClientStopped += OnClientStoppedInternal;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedInternal;
	}

	public void DeInit()
	{
		if (!(NetworkManager.Singleton == null))
		{
			NetworkManager.Singleton.OnServerStarted -= OnServerStarted_Internal;
			NetworkManager.Singleton.OnServerStopped -= OnServerStopped_Internal;
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientStarted_Internal;
			NetworkManager.Singleton.OnClientStopped -= OnClientStoppedInternal;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedInternal;
		}
	}

	private void NotifyConnectionStatus(ulong clientId, ConnectionNotificationManager.ConnectionStatus status)
	{
		if (clientId != NetworkManager.Singleton.LocalClient.ClientId)
		{
			if (NetworkManager.Singleton.IsHost)
			{
				switch (status)
				{
				case ConnectionNotificationManager.ConnectionStatus.Connected:
					otherClients.Add(clientId);
					break;
				case ConnectionNotificationManager.ConnectionStatus.Disconnected:
					otherClients.Remove(clientId);
					break;
				}
			}
			else
			{
				serverClientId = clientId;
			}
		}
		Debug.Log($"Client {clientId} has {status}");
	}

	private void OnServerStarted_Internal()
	{
		this.OnServerStarted?.Invoke();
	}

	private void OnServerStopped_Internal(bool value)
	{
		this.OnServerStopped?.Invoke();
	}

	private void OnClientStarted_Internal(ulong id)
	{
		if (!IsHost)
		{
			this.OnClientStarted?.Invoke();
		}
		else if (id != LazyNetwork.NetworkManager.MyId)
		{
			this.OnClientConnected?.Invoke(id);
		}
	}

	private void OnClientDisconnectedInternal(ulong id)
	{
		if (IsHost && id != LazyNetwork.NetworkManager.MyId)
		{
			this.OnClientDisconnected?.Invoke(id);
		}
	}

	private void OnClientStoppedInternal(bool isHost)
	{
		if (!isHost)
		{
			this.OnClientStopped?.Invoke();
		}
	}

	public bool StartHostGame(string ip, ushort port)
	{
		if (!isManagerInitedCorrectly)
		{
			return false;
		}
		networkTransport.SetConnectionData(ip, port);
		isCoopGame = NetworkManager.Singleton.StartHost();
		return isCoopGame;
	}

	public void DisconnectClient()
	{
	}

	public void StartClient()
	{
		NetworkManager.Singleton.StartClient();
	}

	public bool ConnectToHost(string ip, ushort port)
	{
		networkTransport.SetConnectionData(ip, port);
		isCoopGame = NetworkManager.Singleton.StartClient();
		if (!isCoopGame)
		{
			Debug.LogError("NetworkManager StartClient failed");
			return false;
		}
		return true;
	}

	public void DisconnectFromHost()
	{
		NetworkManager.Singleton.Shutdown();
	}

	public void Editor_ConnectToHost(string ip = "127.0.0.1", ushort port = 8889)
	{
		ConnectToHost(ip, port);
	}

	public void Editor_DisconnectFromHost()
	{
		DisconnectFromHost();
	}

	public void Editor_SendDataToServer(int value)
	{
		NetworkDataSync.Instance.RecvTestDataServerRpc(NetworkManager.Singleton.LocalClient.ClientId, value);
	}

	public void Editor_SendDataToClient(int value)
	{
		NetworkDataSync.Instance.RecvTestDataClientRpc(value);
	}
}
