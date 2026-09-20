using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class UGameSaveMessageChannel : UNetworkMessageChannel<GameSave>
{
	public UGameSaveMessageChannel()
	{
		networkDelivery = NetworkDelivery.ReliableFragmentedSequenced;
	}

	public override void Publish(GameSave data, ulong customClientId = 0uL)
	{
		try
		{
			byte[] array = BinaryDataSerializer.SerializeData(data);
			using FastBufferWriter writer = new FastBufferWriter(array.Length, Allocator.Temp);
			if (!writer.TryBeginWrite(array.Length))
			{
				throw new Exception("Can't write data");
			}
			writer.WriteBytes(array);
			SendMessage(writer, customClientId);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			throw;
		}
	}

	public override void OnReceive(ulong senderClientId, IDisposable messagePayload)
	{
		FastBufferReader fastBufferReader = (FastBufferReader)(object)messagePayload;
		int num = fastBufferReader.Length - fastBufferReader.Position;
		byte[] value = new byte[num];
		if (fastBufferReader.TryBeginRead(num))
		{
			fastBufferReader.ReadBytes(ref value, value.Length);
			LobbyHelper.Client_InitGameSave(BinaryDataSerializer.DeserializeData<GameSave>(value));
			return;
		}
		throw new Exception("Can't read data");
	}
}
