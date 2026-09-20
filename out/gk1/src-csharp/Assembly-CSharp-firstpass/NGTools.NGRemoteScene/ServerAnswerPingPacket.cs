using NGTools.Network;

namespace NGTools.NGRemoteScene;

[PacketLinkTo(4, false)]
internal sealed class ServerAnswerPingPacket : Packet
{
	private ServerAnswerPingPacket(ByteBuffer buffer)
		: base(buffer)
	{
	}

	public ServerAnswerPingPacket()
	{
	}
}
