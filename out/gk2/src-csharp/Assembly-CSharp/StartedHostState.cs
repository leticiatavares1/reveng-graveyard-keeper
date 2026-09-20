public class StartedHostState : OnlineState
{
	public StartedHostState(ConnectionManager connectionManager, NetworkPackageSystem networkPackageSystem)
		: base(connectionManager, networkPackageSystem)
	{
	}

	public override void Enter()
	{
		base.Enter();
		LazyNetwork.NetworkManager.OnClientConnected += OnClientConnected;
		LazyNetwork.NetworkManager.OnClientDisconnected += OnClientDisconnected;
	}

	public override void Exit()
	{
		base.Exit();
		LazyNetwork.NetworkManager.OnClientConnected -= OnClientConnected;
		LazyNetwork.NetworkManager.OnClientDisconnected -= OnClientDisconnected;
	}

	public override void SendNetworkData<T>(T data)
	{
		if (data is Command command)
		{
			command.Execute(LazyNetwork.NetworkManager.MyId, MainGame.Instance.GameSave);
		}
		base.SendNetworkData(data);
	}

	protected override void ExecuteCommand(ICommand command, ulong senderClientId)
	{
		command.Execute(senderClientId, MainGame.Instance.GameSave);
		SendNetworkData(this);
	}

	protected override void ExecutePackage(CommandPackage package, ulong senderClientId)
	{
		networkPackageSystem.ExecutePackageForHost(package, senderClientId, this);
	}

	private void OnClientConnected(ulong id)
	{
		AddDestinationClient(id);
	}

	private void OnClientDisconnected(ulong id)
	{
		RemoveDestinationClient(id);
	}
}
