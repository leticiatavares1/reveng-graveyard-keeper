namespace NGTools.Network;

[PacketLinkTo(1, false)]
internal sealed class ServerHasDisconnectedPacket : Packet
{
	private ServerHasDisconnectedPacket(ByteBuffer buffer)
		: base(buffer)
	{
	}

	public ServerHasDisconnectedPacket()
	{
	}
}
