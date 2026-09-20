using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkPackageSystem
{
	private PackageList<CommandPackage> packages;

	private ulong lastCreatedPackageId;

	private ulong lastReceivedPackageId;

	public int PackagesCountSentThisTick { get; set; }

	public int PackagesCountReceivedThisTick { get; set; }

	public event Action<ulong> OnPackageRetrievalFailed;

	public NetworkPackageSystem(int packageQueueSize)
	{
		packages = new PackageList<CommandPackage>(packageQueueSize);
		lastCreatedPackageId = 0uL;
		lastReceivedPackageId = 0uL;
	}

	public void CreateCommandPackage(List<Command> commands)
	{
		Queue<Command> queue = new Queue<Command>();
		for (int i = 0; i < commands.Count; i++)
		{
			queue.Enqueue(commands[i]);
		}
		while (queue.Count > 0)
		{
			CommandPackage newCommandPackage = GetNewCommandPackage();
			while (queue.Count > 0 && newCommandPackage.TryAddCommand(queue.Peek()))
			{
				queue.Dequeue();
			}
			if (!packages.TryAdd(newCommandPackage))
			{
				if (LazyNetwork.NetworkManager.IsHost)
				{
					throw new Exception("Network package queue is maxed out. Consider increasing queue size in [UNetworkManager] or optimize payloads");
				}
				break;
			}
		}
	}

	public void TrySendRegularPackage()
	{
		if (packages.TryGetForSending(out var item))
		{
			LazyNetwork.NetworkMessageChannelManager.Publish(item, 0uL);
			PackagesCountSentThisTick++;
		}
	}

	public void SendMissingPackages(ulong clientId, ulong lastReceivedPackageId)
	{
		ulong num = lastReceivedPackageId + 1;
		ulong packageId = packages.Last().packageId;
		while (num <= packageId)
		{
			if (!packages.TryGetById(num, out var package))
			{
				this.OnPackageRetrievalFailed?.Invoke(clientId);
				break;
			}
			LazyNetwork.NetworkMessageChannelManager.Publish(package, 0uL);
			PackagesCountSentThisTick++;
			num++;
			Debug.Log($"Missing Package #{package.packageId} was sent to client [{clientId}]");
		}
	}

	public void ExecutePackageForHost(CommandPackage package, ulong senderClientId, StartedHostState currentState)
	{
		for (int i = 0; i < package.serializedCommands.Count; i++)
		{
			ICommand data = CommandFactory.DeserializeCommand(package.serializedCommands[i]);
			currentState.ReceiveNetworkData(data, senderClientId);
		}
		PackagesCountReceivedThisTick++;
	}

	public void ExecutePackageForClient(CommandPackage package, ulong senderClientId, ConnectedToHostState currentState)
	{
		if (!IsPackageQueueConsistent(package.packageId))
		{
			OrderMessage data = new OrderMessage(OrderMessageType.RequestPackages)
			{
				lastPackageIdReceived = (int)lastCreatedPackageId
			};
			LazyNetwork.NetworkMessageChannelManager.Publish(data, 0uL);
			Debug.LogWarning($"Client missed one or more packages from server. Last received = Package #{lastReceivedPackageId}." + "Requesting missing packages from server...");
			return;
		}
		lastReceivedPackageId = package.packageId;
		foreach (byte[] serializedCommand in package.serializedCommands)
		{
			ICommand data2 = CommandFactory.DeserializeCommand(serializedCommand);
			currentState.ReceiveNetworkData(data2, senderClientId);
		}
		PackagesCountReceivedThisTick++;
	}

	public void Debug_ChangeLastReceivedPackageId(ulong value)
	{
		lastReceivedPackageId = value;
	}

	private bool IsPackageQueueConsistent(ulong packageId)
	{
		if ((lastReceivedPackageId >= packageId || packageId - lastReceivedPackageId != 1) && lastReceivedPackageId < packageId)
		{
			return lastReceivedPackageId == 0;
		}
		return true;
	}

	private CommandPackage GetNewCommandPackage()
	{
		return new CommandPackage(++lastCreatedPackageId);
	}
}
