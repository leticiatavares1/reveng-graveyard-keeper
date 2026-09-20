using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGTools.Network;

public class PacketExecuter
{
	private Dictionary<int, Action<Client, Packet>> packets;

	protected PacketExecuter()
	{
		packets = new Dictionary<int, Action<Client, Packet>>();
	}

	public void ExecutePacket(Client sender, Packet packet)
	{
		if (packets.TryGetValue(packet.packetId, out var value))
		{
			value(sender, packet);
		}
	}

	public void HandlePacket(int packetId, Action<Client, Packet> callback)
	{
		if (packets.ContainsKey(packetId))
		{
			Debug.LogError("Packet with id \"" + packetId + "\" is already handled by " + GetType().Name + ".");
		}
		packets.Add(packetId, callback);
	}

	public void UnhandlePacket(int packetId)
	{
		if (!packets.ContainsKey(packetId))
		{
			Debug.LogError("Packet with id \"" + packetId + "\" is being removed but is not even handled.");
		}
		packets.Remove(packetId);
	}
}
