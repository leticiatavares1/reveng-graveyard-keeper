using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class UCommandMessageChannel : UNetworkMessageChannel<ICommand>
{
	public override void Publish(ICommand data, ulong customClientId = 0uL)
	{
		try
		{
			byte[] array = CommandFactory.SerializeCommand(data);
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
			ICommand type = CommandFactory.DeserializeCommand(value);
			NotifyOnMessageReceive(type, senderClientId);
			return;
		}
		throw new Exception("Can't read data");
	}
}
