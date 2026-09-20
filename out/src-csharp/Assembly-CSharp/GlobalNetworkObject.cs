using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GlobalNetworkObject : NetworkTransmitter
{
	private bool _is_server;

	private static int kRpcRpcPrepareToReceiveGameMap;

	private static int kCmdCmdOnClientReceivedMap;

	public void Init(bool is_server)
	{
		Debug.Log("GlobalNetworkObject init, is_server = " + is_server);
		_is_server = is_server;
	}

	public void SendMapFromServerToClients(SerializableGameMap map)
	{
		CallRpcPrepareToReceiveGameMap();
		map.RestoreScene();
	}

	[ClientRpc(channel = 0)]
	private void RpcPrepareToReceiveGameMap()
	{
		Debug.Log("RpcPrepareToReceiveGameMap, is_server = " + _is_server);
		if (_is_server)
		{
			base.OnDataFragmentReceived -= ClientOnMapProgress;
			base.OnDataCompletelyReceived -= ClientOnMapReceived;
		}
		else
		{
			base.OnDataFragmentReceived += ClientOnMapProgress;
			base.OnDataCompletelyReceived += ClientOnMapReceived;
			MainGame.me.save.map.ClearSceneMap();
		}
	}

	[Command]
	private void CmdOnClientReceivedMap()
	{
		if (_is_server)
		{
			Debug.Log("CmdOnClientReceivedMap");
		}
	}

	[Client]
	private void ClientOnMapProgress(int transmission_id, byte[] data)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void GlobalNetworkObject::ClientOnMapProgress(System.Int32,System.Byte[])' called on server");
		}
		else if (!_is_server)
		{
			Debug.Log("ClientOnMapProgress, data len = " + data.Length);
		}
	}

	[Client]
	private void ClientOnMapReceived(int transmission_id, byte[] data)
	{
		if (!NetworkClient.active)
		{
			Debug.LogWarning("[Client] function 'System.Void GlobalNetworkObject::ClientOnMapReceived(System.Int32,System.Byte[])' called on server");
		}
		else if (!_is_server)
		{
			Debug.Log("ClientOnMapReceived");
			string @string = Encoding.UTF8.GetString(data);
			MainGame.me.save.map.FromJSON(@string);
			MainGame.me.save.map.RestoreScene();
			CallCmdOnClientReceivedMap();
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdOnClientReceivedMap(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOnClientReceivedMap called on client.");
		}
		else
		{
			((GlobalNetworkObject)obj).CmdOnClientReceivedMap();
		}
	}

	public void CallCmdOnClientReceivedMap()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdOnClientReceivedMap called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdOnClientReceivedMap();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdOnClientReceivedMap);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdOnClientReceivedMap");
	}

	protected static void InvokeRpcRpcPrepareToReceiveGameMap(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPrepareToReceiveGameMap called on server.");
		}
		else
		{
			((GlobalNetworkObject)obj).RpcPrepareToReceiveGameMap();
		}
	}

	public void CallRpcPrepareToReceiveGameMap()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPrepareToReceiveGameMap called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPrepareToReceiveGameMap);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcPrepareToReceiveGameMap");
	}

	static GlobalNetworkObject()
	{
		kCmdCmdOnClientReceivedMap = -10160345;
		NetworkBehaviour.RegisterCommandDelegate(typeof(GlobalNetworkObject), kCmdCmdOnClientReceivedMap, InvokeCmdCmdOnClientReceivedMap);
		kRpcRpcPrepareToReceiveGameMap = 1049007849;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GlobalNetworkObject), kRpcRpcPrepareToReceiveGameMap, InvokeRpcRpcPrepareToReceiveGameMap);
		NetworkCRC.RegisterBehaviour("GlobalNetworkObject", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
