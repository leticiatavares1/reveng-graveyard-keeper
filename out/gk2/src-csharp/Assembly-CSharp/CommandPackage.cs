using System;
using System.Collections.Generic;

[Serializable]
public class CommandPackage
{
	public ulong packageId;

	public List<byte[]> serializedCommands;

	public int PackageSize
	{
		get
		{
			int num = 0;
			for (int i = 0; i < serializedCommands.Count; i++)
			{
				num += serializedCommands[i].Length;
			}
			return num;
		}
	}

	public CommandPackage()
	{
	}

	public CommandPackage(ulong packageId)
	{
		this.packageId = packageId;
		serializedCommands = new List<byte[]>();
	}

	public bool TryAddCommand(ICommand command)
	{
		byte[] array = CommandFactory.SerializeCommand(command);
		int maxPayloadSize = LazyNetwork.NetworkManager.MaxPayloadSize;
		if (array.Length > maxPayloadSize)
		{
			throw new Exception($"Command size is way too large. MaxPayloadSize = {maxPayloadSize}");
		}
		if (PackageSize + array.Length > maxPayloadSize)
		{
			return false;
		}
		serializedCommands.Add(array);
		return true;
	}

	public override string ToString()
	{
		return $"[Package #{packageId}]: commands = {serializedCommands.Count}; size = {PackageSize} bytes";
	}
}
