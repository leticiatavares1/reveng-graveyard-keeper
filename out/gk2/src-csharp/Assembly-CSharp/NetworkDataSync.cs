using Unity.Netcode;
using UnityEngine;

public class NetworkDataSync : NetworkBehaviour
{
	public static NetworkDataSync Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
		NetworkManager.Singleton.AddNetworkPrefab(base.gameObject);
	}

	[ServerRpc(RequireOwnership = false)]
	public void RecvTestDataServerRpc(ulong clientId, int intVal)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(2800608343u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, clientId);
			BytePacker.WriteValueBitPacked(bufferWriter, intVal);
			__endSendServerRpc(ref bufferWriter, 2800608343u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (NetworkManager.Singleton.LocalClient.ClientId != clientId)
			{
				Debug.Log(string.Format("clientId: [{0}]; {1}, {2}: [{3}]", clientId, "RecvTestDataServerRpc", "intVal", intVal));
			}
		}
	}

	[ClientRpc]
	public void RecvTestDataClientRpc(int intVal)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1606100119u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, intVal);
			__endSendClientRpc(ref bufferWriter, 1606100119u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsOwner)
			{
				Debug.Log(string.Format("{0}, {1}: [{2}]", "RecvTestDataClientRpc", "intVal", intVal));
			}
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2800608343u, __rpc_handler_2800608343, "RecvTestDataServerRpc", RpcInvokePermission.Everyone);
		__registerRpc(1606100119u, __rpc_handler_1606100119, "RecvTestDataClientRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2800608343(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value);
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NetworkDataSync)target).RecvTestDataServerRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1606100119(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NetworkDataSync)target).RecvTestDataClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "NetworkDataSync";
	}
}
