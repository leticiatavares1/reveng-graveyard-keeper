using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
	public static CustomNetworkManager me;

	public static bool is_running;

	public static bool is_server;

	private static bool _server_started;

	private static GlobalNetworkObject _gno;

	public override void OnStartServer()
	{
		base.OnStartServer();
	}

	private void OnAddPlayer(NetworkMessage netmsg)
	{
		Debug.Log("Add player");
	}

	public void Awake()
	{
		Debug.Log("CustomNetworkManager awake");
		me = this;
		if (_gno == null)
		{
			GameObject obj = new GameObject("GlobalNetworkObject");
			obj.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
			_gno = obj.AddComponent<GlobalNetworkObject>();
		}
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short player_controller_id, NetworkReader extra_message_reader)
	{
		Debug.Log("OnServerAddPlayer id = " + player_controller_id);
		base.OnServerAddPlayer(conn, player_controller_id, extra_message_reader);
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short player_controller_id)
	{
		Debug.Log("OnServerAddPlayer (2) id = " + player_controller_id + ", host = " + conn.hostId + ", addr = " + conn.address);
		PlayerComponent playerComponent = PlayerComponent.SpawnPlayer(IsLocalPlayer(conn));
		NetworkServer.AddPlayerForConnection(conn, playerComponent.gameObject, player_controller_id);
		MainGame.me.save.QuickSave();
		_gno.SendMapFromServerToClients(MainGame.me.save.map);
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log("OnClientConnect, is_server = " + is_server);
		if (!is_server)
		{
			MainGame.me.world.FindAndRemovePlayerPrefab();
		}
		Debug.Log("ClientScene.AddPlayer");
		ClientScene.AddPlayer(conn, 0);
		ReadOnlyCollection<NetworkConnection> connections = NetworkServer.connections;
		_ = ClientScene.localPlayers;
		_ = NetworkServer.handlers;
		foreach (NetworkConnection item in connections)
		{
			_ = item;
		}
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		is_server = true;
		bool flag = IsLocalPlayer(conn);
		Debug.Log("OnServerConnect, conn.hostId = " + conn.hostId + ", _server_started = " + _server_started);
		base.OnServerConnect(conn);
		if (!_server_started)
		{
			if (_gno == null)
			{
				Debug.LogError("GNO is null");
				return;
			}
			_gno.Init(flag);
		}
		if (flag && !_server_started)
		{
			_server_started = true;
		}
	}

	private static bool IsLocalPlayer(NetworkConnection conn)
	{
		return conn.hostId == -1;
	}
}
