using System.Collections.Generic;

public abstract class OnlineState : ConnectionState
{
	protected NetworkPackageSystem networkPackageSystem;

	protected List<Command> commands = new List<Command>();

	protected List<UniqueCommandHolder> notRepeatableCommands = new List<UniqueCommandHolder>();

	public OnlineState(ConnectionManager connectionManager, NetworkPackageSystem networkPackageSystem)
		: base(connectionManager)
	{
		this.networkPackageSystem = networkPackageSystem;
	}

	protected abstract void ExecuteCommand(ICommand command, ulong senderClientId);

	protected abstract void ExecutePackage(CommandPackage package, ulong senderClientId);

	public override void Enter()
	{
		LazyNetwork.NetworkMessageChannelManager.RegisterMessageChannels();
		LazyNetwork.NetworkMessageChannelManager.AddListener<ICommand>(ExecuteCommand);
		LazyNetwork.NetworkMessageChannelManager.AddListener<CommandPackage>(ExecutePackage);
	}

	public override void Exit()
	{
		LazyNetwork.NetworkMessageChannelManager.RemoveListener<ICommand>(ExecuteCommand);
		LazyNetwork.NetworkMessageChannelManager.RemoveListener<CommandPackage>(ExecutePackage);
		LazyNetwork.NetworkMessageChannelManager.UnregisterMessageChannels();
	}

	public override void SendNetworkData<T>(T data)
	{
		if (data is Command command)
		{
			AddCommand(command);
		}
		else if (data is UniqueCommandHolder commandHolder)
		{
			AddNotRepeatableCommand(commandHolder);
		}
	}

	public override void ReceiveNetworkData<T>(T data, ulong senderClientId)
	{
		if (data is ICommand command)
		{
			ExecuteCommand(command, senderClientId);
		}
		else if (data is CommandPackage package)
		{
			ExecutePackage(package, senderClientId);
		}
	}

	public override void Update()
	{
		foreach (UniqueCommandHolder notRepeatableCommand in notRepeatableCommands)
		{
			LazyNetwork.NetworkMessageChannelManager.Publish((ICommand)notRepeatableCommand.GetCommand(), 0uL);
			notRepeatableCommand.isQueued = false;
		}
		notRepeatableCommands.Clear();
		if (commands.Count > 0)
		{
			networkPackageSystem.CreateCommandPackage(commands);
			commands.Clear();
			networkPackageSystem.TrySendRegularPackage();
		}
	}

	private void AddCommand(Command command)
	{
		if (command != null && (!LazyNetwork.NetworkManager.IsHost || LazyNetwork.NetworkManager.OtherClients.Count != 0))
		{
			commands.Add(command);
		}
	}

	private void AddNotRepeatableCommand(UniqueCommandHolder commandHolder)
	{
		if (commandHolder != null && (!LazyNetwork.NetworkManager.IsHost || LazyNetwork.NetworkManager.OtherClients.Count != 0) && !commandHolder.isQueued)
		{
			notRepeatableCommands.Add(commandHolder);
			commandHolder.isQueued = true;
		}
	}
}
