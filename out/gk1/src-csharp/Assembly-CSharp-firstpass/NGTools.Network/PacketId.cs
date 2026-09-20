using System.Collections.Generic;
using System.Reflection;

namespace NGTools.Network;

internal class PacketId
{
	public const int ServerHasDisconnect = 1;

	public const int ClientHasDisconnect = 2;

	public const int ClientSendPing = 3;

	public const int ServerAnswerPing = 4;

	public const int Server_ErrorNotification = 5;

	private static Dictionary<int, string> packetNames;

	public static string GetPacketName(int id)
	{
		if (packetNames == null)
		{
			packetNames = new Dictionary<int, string>();
			FieldInfo[] fields = typeof(PacketId).GetFields();
			for (int i = 0; i < fields.Length; i++)
			{
				packetNames.Add((int)fields[i].GetRawConstantValue(), fields[i].Name);
			}
		}
		return packetNames[id];
	}
}
