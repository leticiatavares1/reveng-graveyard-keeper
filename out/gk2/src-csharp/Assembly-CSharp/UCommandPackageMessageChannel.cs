using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class UCommandPackageMessageChannel : UNetworkMessageChannel<CommandPackage>
{
	public UCommandPackageMessageChannel()
	{
		networkDelivery = NetworkDelivery.ReliableFragmentedSequenced;
	}

	public override void Publish(CommandPackage data, ulong customClientId = 0uL)
	{
		try
		{
			byte[] array = null;
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream, data);
				array = memoryStream.ToArray();
			}
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
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			CommandPackage type;
			using (MemoryStream serializationStream = new MemoryStream(value))
			{
				type = (CommandPackage)binaryFormatter.Deserialize(serializationStream);
			}
			NotifyOnMessageReceive(type, senderClientId);
			return;
		}
		throw new Exception("Can't read data");
	}
}
