using NGTools.Network;
using UnityEngine;

namespace NGTools.NGRemoteScene;

[PacketLinkTo(2, false)]
internal sealed class ClientHasDisconnectedPacket : Packet
{
	private ClientHasDisconnectedPacket(ByteBuffer buffer)
		: base(buffer)
	{
	}

	public ClientHasDisconnectedPacket()
	{
	}

	public override void OnGUI(IUnityData unityData)
	{
		GUILayout.Label("Disconnecting client.");
	}
}
