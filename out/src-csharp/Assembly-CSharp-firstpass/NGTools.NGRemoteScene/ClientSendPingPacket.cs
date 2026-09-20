using NGTools.Network;
using UnityEngine;

namespace NGTools.NGRemoteScene;

[PacketLinkTo(3, false)]
internal sealed class ClientSendPingPacket : Packet
{
	private ClientSendPingPacket(ByteBuffer buffer)
		: base(buffer)
	{
	}

	public ClientSendPingPacket()
	{
	}

	public override void OnGUI(IUnityData unityData)
	{
		GUILayout.Label("Client send ping.");
	}
}
