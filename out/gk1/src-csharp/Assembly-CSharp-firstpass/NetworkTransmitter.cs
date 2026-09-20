using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkTransmitter : NetworkBehaviour
{
	private class TransmissionData
	{
		public int cur_data_index;

		public byte[] data;

		public TransmissionData(byte[] _data)
		{
			cur_data_index = 0;
			data = _data;
		}
	}

	[CompilerGenerated]
	private sealed class _003CSendBytesToClientsRoutine_003Ed__19 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public int transmission_id;

		public byte[] data;

		public NetworkTransmitter _003C_003E4__this;

		private TransmissionData _003Cdata_to_transmit_003E5__2;

		private int _003Cbuffer_size_003E5__3;

		private byte[] _003Cbuffer_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSendBytesToClientsRoutine_003Ed__19(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			NetworkTransmitter networkTransmitter = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				UnityEngine.Debug.Log(LOG_PREFIX + "SendBytesToClients transmission_id=" + transmission_id + " | datasize=" + data.Length);
				networkTransmitter.CallRpcPrepareToReceiveBytes(transmission_id, data.Length);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				networkTransmitter.serverTransmissionIds.Add(transmission_id);
				_003Cdata_to_transmit_003E5__2 = new TransmissionData(data);
				_003Cbuffer_size_003E5__3 = _default_buffer_size;
				break;
			case 2:
				_003C_003E1__state = -1;
				if (networkTransmitter.OnDataFragmentSent != null)
				{
					networkTransmitter.OnDataFragmentSent(transmission_id, _003Cbuffer_003E5__4);
				}
				_003Cbuffer_003E5__4 = null;
				break;
			}
			if (_003Cdata_to_transmit_003E5__2.cur_data_index < _003Cdata_to_transmit_003E5__2.data.Length - 1)
			{
				int num2 = _003Cdata_to_transmit_003E5__2.data.Length - _003Cdata_to_transmit_003E5__2.cur_data_index;
				if (num2 < _003Cbuffer_size_003E5__3)
				{
					_003Cbuffer_size_003E5__3 = num2;
				}
				_003Cbuffer_003E5__4 = new byte[_003Cbuffer_size_003E5__3];
				Array.Copy(_003Cdata_to_transmit_003E5__2.data, _003Cdata_to_transmit_003E5__2.cur_data_index, _003Cbuffer_003E5__4, 0, _003Cbuffer_size_003E5__3);
				networkTransmitter.CallRpcReceiveBytes(transmission_id, _003Cbuffer_003E5__4);
				_003Cdata_to_transmit_003E5__2.cur_data_index += _003Cbuffer_size_003E5__3;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
			networkTransmitter.serverTransmissionIds.Remove(transmission_id);
			if (networkTransmitter.OnDataComepletelySent != null)
			{
				networkTransmitter.OnDataComepletelySent(transmission_id, _003Cdata_to_transmit_003E5__2.data);
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private static readonly string LOG_PREFIX;

	public const int RELIABLE_SEQUENCED_CHANNEL = 0;

	private static int _default_buffer_size;

	private List<int> serverTransmissionIds = new List<int>();

	private Dictionary<int, TransmissionData> _client_transmission_data = new Dictionary<int, TransmissionData>();

	private static int kRpcRpcPrepareToReceiveBytes;

	private static int kRpcRpcReceiveBytes;

	public event UnityAction<int, byte[]> OnDataComepletelySent;

	public event UnityAction<int, byte[]> OnDataFragmentSent;

	public event UnityAction<int, byte[]> OnDataFragmentReceived;

	public event UnityAction<int, byte[]> OnDataCompletelyReceived;

	[Server]
	public void SendBytesToClients(int transmission_id, byte[] data)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void NetworkTransmitter::SendBytesToClients(System.Int32,System.Byte[])' called on client");
		}
		else
		{
			StartCoroutine(SendBytesToClientsRoutine(transmission_id, data));
		}
	}

	[Server]
	[IteratorStateMachine(typeof(_003CSendBytesToClientsRoutine_003Ed__19))]
	public IEnumerator SendBytesToClientsRoutine(int transmission_id, byte[] data)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Collections.IEnumerator NetworkTransmitter::SendBytesToClientsRoutine(System.Int32,System.Byte[])' called on client");
			return null;
		}
		return new _003CSendBytesToClientsRoutine_003Ed__19(0)
		{
			_003C_003E4__this = this,
			transmission_id = transmission_id,
			data = data
		};
	}

	[ClientRpc(channel = 0)]
	private void RpcPrepareToReceiveBytes(int transmission_id, int expected_size)
	{
		if (!_client_transmission_data.ContainsKey(transmission_id))
		{
			TransmissionData value = new TransmissionData(new byte[expected_size]);
			_client_transmission_data.Add(transmission_id, value);
		}
	}

	[ClientRpc(channel = 0)]
	private void RpcReceiveBytes(int transmission_id, byte[] rec_buffer)
	{
		if (!_client_transmission_data.ContainsKey(transmission_id))
		{
			return;
		}
		TransmissionData transmissionData = _client_transmission_data[transmission_id];
		Array.Copy(rec_buffer, 0, transmissionData.data, transmissionData.cur_data_index, rec_buffer.Length);
		transmissionData.cur_data_index += rec_buffer.Length;
		if (this.OnDataFragmentReceived != null)
		{
			this.OnDataFragmentReceived(transmission_id, rec_buffer);
		}
		if (transmissionData.cur_data_index >= transmissionData.data.Length - 1)
		{
			UnityEngine.Debug.Log(LOG_PREFIX + "Completely Received Data at transmission_id=" + transmission_id);
			_client_transmission_data.Remove(transmission_id);
			if (this.OnDataCompletelyReceived != null)
			{
				this.OnDataCompletelyReceived(transmission_id, transmissionData.data);
			}
		}
	}

	static NetworkTransmitter()
	{
		LOG_PREFIX = "[" + typeof(NetworkTransmitter).Name + "]: ";
		_default_buffer_size = 1024;
		kRpcRpcPrepareToReceiveBytes = 1749299505;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NetworkTransmitter), kRpcRpcPrepareToReceiveBytes, InvokeRpcRpcPrepareToReceiveBytes);
		kRpcRpcReceiveBytes = 1006209153;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NetworkTransmitter), kRpcRpcReceiveBytes, InvokeRpcRpcReceiveBytes);
		NetworkCRC.RegisterBehaviour("NetworkTransmitter", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcPrepareToReceiveBytes(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcPrepareToReceiveBytes called on server.");
		}
		else
		{
			((NetworkTransmitter)obj).RpcPrepareToReceiveBytes((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcReceiveBytes(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcReceiveBytes called on server.");
		}
		else
		{
			((NetworkTransmitter)obj).RpcReceiveBytes((int)reader.ReadPackedUInt32(), reader.ReadBytesAndSize());
		}
	}

	public void CallRpcPrepareToReceiveBytes(int transmission_id, int expected_size)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("RPC Function RpcPrepareToReceiveBytes called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPrepareToReceiveBytes);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)transmission_id);
		networkWriter.WritePackedUInt32((uint)expected_size);
		SendRPCInternal(networkWriter, 0, "RpcPrepareToReceiveBytes");
	}

	public void CallRpcReceiveBytes(int transmission_id, byte[] rec_buffer)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("RPC Function RpcReceiveBytes called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcReceiveBytes);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)transmission_id);
		networkWriter.WriteBytesFull(rec_buffer);
		SendRPCInternal(networkWriter, 0, "RpcReceiveBytes");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
