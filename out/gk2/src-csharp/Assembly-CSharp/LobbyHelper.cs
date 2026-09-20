using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using LinqTools;
using Unity.Netcode;

public static class LobbyHelper
{
	public static Action OnAllClientsSynced;

	public static Action<ulong> OnClientAdded;

	public static Action<ulong> OnClientSynced;

	private static Dictionary<ulong, bool> connectedClients = new Dictionary<ulong, bool>();

	public static string GetLocalIpAddress()
	{
		return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault((IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
	}

	public static void Host_Init()
	{
		GameSave gameSave = new GameSave();
		GameSave.SetupNewGameSave(gameSave);
		NetworkPlayer networkPlayer = new NetworkPlayer((int)NetworkManager.Singleton.LocalClient.ClientId, gameSave.playerData);
		MainGame.Instance.PrepareGameForNetwork(gameSave, networkPlayer, isHost: true);
		NetworkManager.Singleton.OnClientConnectedCallback += Host_OnClientConnected;
	}

	public static void Host_UpdateClientSyncStatus(ulong clientId)
	{
		connectedClients[clientId] = true;
		OnClientSynced?.Invoke(clientId);
		if (Host_AreAllClientsSynced())
		{
			OnAllClientsSynced?.Invoke();
		}
	}

	public static void Host_StartGame()
	{
		MainGame.Instance.LoadGameScene().Forget();
		foreach (ulong key in connectedClients.Keys)
		{
			OrderMessage data = new OrderMessage(OrderMessageType.ForceStartGame);
			LazyNetwork.NetworkMessageChannelManager.Publish(data, key);
		}
	}

	public static void Host_SyncGameSaves()
	{
		List<ulong> list = new List<ulong>();
		foreach (ulong key in connectedClients.Keys)
		{
			list.Add(key);
			LazyNetwork.NetworkMessageChannelManager.Publish(MainGame.Instance.GameSave, key);
		}
		list.ForEach(delegate(ulong index)
		{
			connectedClients[index] = false;
		});
	}

	private static void Host_OnClientConnected(ulong clientId)
	{
		NetworkPlayer player = MainGame.Instance.GameSave.CreateClient((int)clientId);
		MainGame.Instance.SpawnPlayer(player);
		connectedClients.Add(clientId, value: false);
		OnClientAdded?.Invoke(clientId);
	}

	private static bool Host_AreAllClientsSynced()
	{
		foreach (KeyValuePair<ulong, bool> connectedClient in connectedClients)
		{
			if (!connectedClient.Value)
			{
				return false;
			}
		}
		return true;
	}

	public static void Client_InitGameSave(GameSave gameSave)
	{
		MainGame.Instance.SetGameSave(gameSave);
		OrderMessage data = new OrderMessage(OrderMessageType.ConfirmGameSave);
		LazyNetwork.NetworkMessageChannelManager.Publish(data, 0uL);
	}

	public static void Client_StartGame()
	{
		LazyUI.GetWindow<UILobbyWindow>().Close();
		int clientId = (int)NetworkManager.Singleton.LocalClient.ClientId;
		if (MainGame.Instance.GameSave.GetClient(clientId, out var clientPlayer))
		{
			MainGame.Instance.PrepareGameForNetwork(MainGame.Instance.GameSave, clientPlayer, isHost: false);
			MainGame.Instance.LoadGameScene().Forget();
		}
	}
}
