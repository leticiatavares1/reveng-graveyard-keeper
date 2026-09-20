using System.Collections.Generic;
using System.Net.Sockets;

namespace NGTools.Network;

public abstract class AbstractTcpListener : NetworkListener
{
	public int backLog = 1;

	protected TcpListener tcpListener;

	private List<Packet> delayedPackets;

	public List<Client> clients { get; private set; }

	protected virtual void Awake()
	{
		clients = new List<Client>();
		delayedPackets = new List<Packet>();
	}

	protected virtual void Update()
	{
		if (tcpListener == null)
		{
			return;
		}
		for (int i = 0; i < clients.Count; i++)
		{
			if (DetectClientDisced(clients[i]))
			{
				clients[i].Close();
				clients.RemoveAt(i);
				i--;
			}
			else
			{
				clients[i].Write();
			}
		}
		if (delayedPackets.Count > 0)
		{
			for (int j = 0; j < delayedPackets.Count; j++)
			{
				BroadcastPacket(delayedPackets[j]);
			}
			delayedPackets.Clear();
		}
		for (int k = 0; k < clients.Count; k++)
		{
			clients[k].ExecReceivedCommands(server.executer);
		}
	}

	private bool DetectClientDisced(Client client)
	{
		if (!client.tcpClient.Connected)
		{
			return true;
		}
		return false;
	}

	public override void StopServer()
	{
		if (tcpListener == null)
		{
			return;
		}
		BroadcastPacket(new ServerHasDisconnectedPacket());
		for (int i = 0; i < clients.Count; i++)
		{
			if (DetectClientDisced(clients[i]))
			{
				clients[i].Close();
				clients.RemoveAt(i);
				i--;
			}
			else
			{
				clients[i].Write();
			}
		}
		tcpListener.Server.Close();
		tcpListener = null;
		InternalNGDebug.LogFile("Stopped AbstractTcpListener.");
	}

	public void BroadcastPacket(Packet packet)
	{
		for (int i = 0; i < clients.Count; i++)
		{
			clients[i].AddPacket(packet);
		}
	}

	public void BroadcastPostPacket(Packet packet)
	{
		for (int i = 0; i < clients.Count; i++)
		{
			delayedPackets.Add(packet);
		}
	}
}
