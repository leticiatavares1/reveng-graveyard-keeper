public class ConnectedToHostState : OnlineState
{
	public ConnectedToHostState(ConnectionManager connectionManager, NetworkPackageSystem networkPackageSystem)
		: base(connectionManager, networkPackageSystem)
	{
	}

	public override void Enter()
	{
		base.Enter();
		AddDestinationClient(LazyNetwork.NetworkManager.ServerClientId);
	}

	public override void Exit()
	{
		base.Exit();
		RemoveDestinationClient(LazyNetwork.NetworkManager.ServerClientId);
	}

	protected override void ExecuteCommand(ICommand command, ulong senderClientId)
	{
		command.Execute(senderClientId, MainGame.Instance.GameSave);
	}

	protected override void ExecutePackage(CommandPackage package, ulong senderClientId)
	{
		networkPackageSystem.ExecutePackageForClient(package, senderClientId, this);
	}
}
