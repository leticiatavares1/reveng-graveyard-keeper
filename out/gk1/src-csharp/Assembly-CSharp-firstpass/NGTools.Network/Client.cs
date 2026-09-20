using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;

namespace NGTools.Network;

public class Client
{
	public enum BatchMode
	{
		Off,
		On
	}

	public struct ExecutedPacket
	{
		public readonly string time;

		public readonly Packet packet;

		public ExecutedPacket(Packet packet)
		{
			time = DateTime.Now.ToString("HH:mm:ss.fff");
			this.packet = packet;
		}
	}

	public class Batch
	{
		public string name;

		public readonly Packet[] batchedPackets;

		public Batch(string name, Packet[] batch)
		{
			this.name = name;
			batchedPackets = batch;
		}
	}

	public const int SendBufferCapacity = 1024;

	public const int ReadBufferSize = 16384;

	public const int TempBufferSize = 4048;

	public readonly TcpClient tcpClient;

	public BatchMode batchMode;

	public readonly List<Packet> batchedPackets;

	public string[] batchNames;

	public readonly bool saveSentPackets;

	public readonly List<string> receivedPacketsHistoric;

	public readonly List<ExecutedPacket> sentPacketsHistoric;

	private readonly List<Batch> batchesHistoric;

	private readonly List<Packet> pendingPackets;

	private readonly List<Packet> receivedPackets;

	private readonly ByteBuffer packetBuffer;

	private readonly ByteBuffer receiveBuffer;

	private readonly ByteBuffer fullPacketBuffer;

	private readonly NetworkStream reader;

	private readonly NetworkStream writer;

	private readonly ByteBuffer sendBuffer;

	private readonly ByteBuffer tinySendBuffer;

	private readonly byte[] tempBuffer;

	private readonly object[] packetArgument;

	private int packetId = -1;

	private uint length;

	private readonly Dictionary<int, Type> packetTypes;

	public long bytesSent { get; private set; }

	public long bytesReceived { get; private set; }

	public int PendingPacketsCount => pendingPackets.Count;

	public Client(TcpClient tcpClient, bool save = true)
	{
		this.tcpClient = tcpClient;
		batchMode = BatchMode.Off;
		reader = this.tcpClient.GetStream();
		writer = reader;
		saveSentPackets = save;
		if (saveSentPackets)
		{
			sentPacketsHistoric = new List<ExecutedPacket>(512);
		}
		receivedPacketsHistoric = new List<string>(512);
		batchedPackets = new List<Packet>(64);
		batchesHistoric = new List<Batch>(4);
		pendingPackets = new List<Packet>(4);
		receivedPackets = new List<Packet>(4);
		batchNames = new string[0];
		packetBuffer = new ByteBuffer(16384);
		receiveBuffer = new ByteBuffer(16384);
		fullPacketBuffer = new ByteBuffer(16384);
		sendBuffer = new ByteBuffer(1024);
		tinySendBuffer = new ByteBuffer(1024);
		tempBuffer = new byte[4048];
		packetArgument = new object[1] { packetBuffer };
		packetId = -1;
		packetTypes = new Dictionary<int, Type>();
		foreach (Type item in Utility.EachSubClassesOf(typeof(Packet)))
		{
			object[] customAttributes = item.GetCustomAttributes(typeof(PacketLinkToAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				if (packetTypes.ContainsKey((customAttributes[0] as PacketLinkToAttribute).packetId))
				{
					InternalNGDebug.LogError("Packet \"" + item.FullName + "\" shares the same ID as \"" + packetTypes[(customAttributes[0] as PacketLinkToAttribute).packetId]?.ToString() + "\".");
				}
				else
				{
					packetTypes.Add((customAttributes[0] as PacketLinkToAttribute).packetId, item);
				}
			}
		}
		if (reader.CanRead)
		{
			reader.BeginRead(tempBuffer, 0, tempBuffer.Length, ReadCallBack, this);
		}
		else
		{
			Debug.LogError("Client has a non readable NetworkStream.");
		}
	}

	public void Close()
	{
		reader.Close();
		tcpClient.Close();
	}

	private void ReadCallBack(IAsyncResult ar)
	{
		int num = reader.EndRead(ar);
		lock (receiveBuffer)
		{
			receiveBuffer.Append(tempBuffer, 0, num);
		}
		lock (fullPacketBuffer)
		{
			reader.BeginRead(tempBuffer, 0, tempBuffer.Length, ReadCallBack, this);
			if (!reader.DataAvailable)
			{
				ExecuteBuffer();
			}
		}
	}

	private void ExecuteBuffer()
	{
		lock (receiveBuffer)
		{
			fullPacketBuffer.Append(receiveBuffer);
			bytesReceived += receiveBuffer.Length;
			receiveBuffer.Clear();
		}
		uint num = (uint)fullPacketBuffer.Position;
		while ((uint)(fullPacketBuffer.Length - (int)num) >= 8u)
		{
			if (packetId == -1)
			{
				packetId = fullPacketBuffer.ReadInt32();
				length = fullPacketBuffer.ReadUInt32();
				num = (uint)fullPacketBuffer.Position;
			}
			if (length <= fullPacketBuffer.Length - fullPacketBuffer.Position)
			{
				InternalNGDebug.LogFile("X " + PacketId.GetPacketName(packetId) + " (" + packetId + ") " + length + " B @ " + fullPacketBuffer.Position + "/" + fullPacketBuffer.Length);
				if (packetTypes.TryGetValue(packetId, out var value))
				{
					try
					{
						fullPacketBuffer.CopyBuffer(packetBuffer, (int)num, (int)length);
						lock (receivedPackets)
						{
							Packet packet = Activator.CreateInstance(value, BindingFlags.Instance | BindingFlags.NonPublic, null, packetArgument, null) as Packet;
							receivedPackets.Add(packet);
							receivedPacketsHistoric.Add(DateTime.Now.ToString("HH:mm:ss.fff") + " " + packet);
						}
					}
					catch (Exception exception)
					{
						InternalNGDebug.LogFileException("Packet parsing failed: Type: " + value, exception);
						fullPacketBuffer.Clear();
					}
				}
				else
				{
					InternalNGDebug.LogFile("Unknown command " + PacketId.GetPacketName(packetId) + " (" + packetId + ") of " + length + " chars.");
				}
				num += length;
				fullPacketBuffer.Position = (int)num;
				packetId = -1;
				if (fullPacketBuffer.Length == fullPacketBuffer.Position)
				{
					fullPacketBuffer.Clear();
				}
				continue;
			}
			InternalNGDebug.LogFile("X... " + PacketId.GetPacketName(packetId) + " (" + packetId + ") " + length + " B @ " + fullPacketBuffer.Position + "/" + fullPacketBuffer.Length);
			break;
		}
	}

	public void Write()
	{
		if (pendingPackets.Count == 0)
		{
			return;
		}
		InternalNGDebug.LogFile("W " + pendingPackets.Count + " packet(s).");
		for (int i = 0; i < pendingPackets.Count; i++)
		{
			tinySendBuffer.Clear();
			pendingPackets[i].Out(tinySendBuffer);
			InternalNGDebug.LogFile("W " + PacketId.GetPacketName(pendingPackets[i].packetId) + " (" + pendingPackets[i].packetId + ") " + (uint)tinySendBuffer.Length + " B.");
			sendBuffer.Append(pendingPackets[i].packetId);
			sendBuffer.Append((uint)tinySendBuffer.Length);
			sendBuffer.Append(tinySendBuffer);
		}
		byte[] array = sendBuffer.Flush();
		writer.BeginWrite(array, 0, array.Length, WriteCallBack, this);
		bytesSent += array.Length;
		if (saveSentPackets)
		{
			for (int j = 0; j < pendingPackets.Count; j++)
			{
				sentPacketsHistoric.Add(new ExecutedPacket(pendingPackets[j]));
			}
		}
		pendingPackets.Clear();
	}

	public void WriteCallBack(IAsyncResult ar)
	{
		reader.EndWrite(ar);
	}

	public void ExecReceivedCommands(PacketExecuter executer)
	{
		lock (receivedPackets)
		{
			if (receivedPackets.Count == 0)
			{
				return;
			}
			for (int i = 0; i < receivedPackets.Count; i++)
			{
				try
				{
					executer.ExecutePacket(this, receivedPackets[i]);
				}
				catch (Exception exception)
				{
					InternalNGDebug.LogException(exception);
				}
			}
			receivedPackets.Clear();
		}
	}

	public void AddPacket(Packet packet)
	{
		if (batchMode == BatchMode.On && packet.isBatchable)
		{
			for (int i = 0; i < batchedPackets.Count; i++)
			{
				if (packet.AggregateInto(batchedPackets[i]))
				{
					return;
				}
			}
			batchedPackets.Add(packet);
			return;
		}
		for (int j = 0; j < pendingPackets.Count; j++)
		{
			if (packet.AggregateInto(pendingPackets[j]))
			{
				return;
			}
		}
		pendingPackets.Add(packet);
	}

	public void SaveBatch(string name)
	{
		if (batchedPackets.Count > 0)
		{
			batchesHistoric.Add(new Batch(name, batchedPackets.ToArray()));
			batchNames = new string[batchesHistoric.Count];
			for (int i = 0; i < batchesHistoric.Count; i++)
			{
				batchNames[i] = batchesHistoric[i].name + " (" + batchesHistoric[i].batchedPackets.Length + ")";
			}
		}
	}

	public void ExecuteBatch()
	{
		if (batchedPackets.Count > 0)
		{
			for (int i = 0; i < batchedPackets.Count; i++)
			{
				pendingPackets.Add(batchedPackets[i]);
			}
			batchedPackets.Clear();
		}
	}

	public void LoadBatch(int i)
	{
		if (0 <= i && i < batchesHistoric.Count)
		{
			batchedPackets.Clear();
			for (int j = 0; j < batchesHistoric[i].batchedPackets.Length; j++)
			{
				batchedPackets.Add(batchesHistoric[i].batchedPackets[j]);
			}
		}
	}

	public override string ToString()
	{
		return "FPL=" + fullPacketBuffer.Position + "/" + fullPacketBuffer.Length + " BRecv=" + bytesReceived + " PP=" + pendingPackets.Count;
	}
}
