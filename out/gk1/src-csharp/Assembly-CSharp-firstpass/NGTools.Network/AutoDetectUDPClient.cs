using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NGTools.Network;

public sealed class AutoDetectUDPClient
{
	public const float UDPPingInterval = 3f;

	public static byte[] UDPPingMessage = new byte[4] { 78, 71, 83, 83 };

	public static byte[] UDPEndMessage = new byte[2] { 69, 78 };

	private MonoBehaviour behaviour;

	private UdpClient Client;

	private IPEndPoint[] BroadcastEndPoint;

	private float pingInterval;

	private Coroutine pingCoroutine;

	public AutoDetectUDPClient(MonoBehaviour behaviour, int port, int targetPortMin, int targetPortMax, float pingInterval)
	{
		this.behaviour = behaviour;
		this.pingInterval = pingInterval;
		Client = new UdpClient(port);
		Client.EnableBroadcast = true;
		BroadcastEndPoint = new IPEndPoint[targetPortMax - targetPortMin + 1];
		for (int i = 0; i < BroadcastEndPoint.Length; i++)
		{
			BroadcastEndPoint[i] = new IPEndPoint(IPAddress.Broadcast, targetPortMin + i);
		}
		pingCoroutine = this.behaviour.StartCoroutine(AsyncSendPing());
	}

	public void Stop()
	{
		behaviour.StopCoroutine(pingCoroutine);
		for (int i = 0; i < BroadcastEndPoint.Length; i++)
		{
			Client.Send(UDPEndMessage, UDPEndMessage.Length, BroadcastEndPoint[i]);
		}
		Client.Close();
	}

	private IEnumerator AsyncSendPing()
	{
		AsyncCallback callback = SendPresence;
		WaitForSeconds wait = new WaitForSeconds(pingInterval);
		while (true)
		{
			for (int i = 0; i < BroadcastEndPoint.Length; i++)
			{
				Client.BeginSend(UDPPingMessage, UDPPingMessage.Length, BroadcastEndPoint[i], callback, null);
			}
			yield return wait;
		}
	}

	private void SendPresence(IAsyncResult ar)
	{
		Client.EndSend(ar);
	}
}
