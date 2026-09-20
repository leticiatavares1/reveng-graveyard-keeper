using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NGTools.Network;

public class DefaultTcpListener : AbstractTcpListener
{
	public override void StartServer()
	{
		if (tcpListener != null)
		{
			return;
		}
		try
		{
			tcpListener = new TcpListener(IPAddress.Any, port);
			tcpListener.Start(backLog);
			tcpListener.BeginAcceptTcpClient(AcceptClient, null);
			InternalNGDebug.LogFile("Started TCPListener IPAddress.Any:" + port);
		}
		catch (Exception exception)
		{
			InternalNGDebug.LogException(exception);
		}
	}

	private void AcceptClient(IAsyncResult ar)
	{
		if (tcpListener != null)
		{
			Client client = new Client(tcpListener.EndAcceptTcpClient(ar), Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor);
			base.clients.Add(client);
			InternalNGDebug.LogFile("Accepted Client " + client.tcpClient.Client.RemoteEndPoint);
			tcpListener.BeginAcceptTcpClient(AcceptClient, null);
		}
	}
}
