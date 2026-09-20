using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour
{
	[SerializeField]
	private UnityTransport networkTransport;

	private bool isManagerInitedCorrectly;

	public static GameNetworkManager Singleton { get; private set; }

	private void Awake()
	{
		if (Singleton != null)
		{
			Debug.LogError("More than one instance of GameNetworkManager is found.");
			return;
		}
		Singleton = this;
		Object.DontDestroyOnLoad(Singleton);
	}

	private void Start()
	{
		if (Singleton != this)
		{
			Debug.LogError("It seems it's a duplicate. This shouldn't happen.");
			return;
		}
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
		if (ConnectionNotificationManager.Singleton != null)
		{
			ConnectionNotificationManager.Singleton.OnClientConnectionNotification += NotifyConnectionStatus;
		}
		isManagerInitedCorrectly = true;
	}

	private void NotifyConnectionStatus(ulong clientId, ConnectionNotificationManager.ConnectionStatus status)
	{
		Debug.Log($"Client {clientId} has {status}");
	}

	public bool StartHostGame(string ip, ushort port)
	{
		if (!isManagerInitedCorrectly)
		{
			return false;
		}
		networkTransport.SetConnectionData(ip, port);
		return NetworkManager.Singleton.StartHost();
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
		if (!NetworkManager.Singleton.StartClient())
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
