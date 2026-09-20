using System;
using System.Net;
using System.Net.Sockets;

namespace NGTools.Network;

public class UDPListener : NetworkListener
{
	public UdpClient client;

	private IPEndPoint endPoint;

	private IPEndPoint clientEndPoint;

	private ByteBuffer packetBuffer = new ByteBuffer(6220800);

	private ByteBuffer sendBuffer = new ByteBuffer(6220800);

	public override void StartServer()
	{
		endPoint = new IPEndPoint(IPAddress.Any, port);
		clientEndPoint = new IPEndPoint(IPAddress.Any, port);
		client = new UdpClient(endPoint);
		client.EnableBroadcast = true;
		client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
		client.BeginReceive(ReceivedPacket, null);
		InternalNGDebug.LogFile("Started UDPListener.");
	}

	public override void StopServer()
	{
		client.Close();
		client = null;
		InternalNGDebug.LogFile("Stopped UDPListener.");
	}

	public void Send(Packet packet)
	{
		sendBuffer.Clear();
		packetBuffer.Clear();
		packet.Out(packetBuffer);
		sendBuffer.Append(packet.packetId);
		sendBuffer.Append((uint)packetBuffer.Length);
		sendBuffer.Append(packetBuffer);
		byte[] array = sendBuffer.Flush();
		client.BeginSend(array, array.Length, clientEndPoint, SendPacket, null);
	}

	private void SendPacket(IAsyncResult ar)
	{
		client.EndSend(ar);
	}

	private void ReceivedPacket(IAsyncResult ar)
	{
		client.EndReceive(ar, ref clientEndPoint);
		client.BeginReceive(ReceivedPacket, null);
	}
}
