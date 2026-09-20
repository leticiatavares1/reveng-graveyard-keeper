public class OfflineState : ConnectionState
{
	public OfflineState(ConnectionManager connectionManager)
		: base(connectionManager)
	{
	}

	public override void SendNetworkData<T>(T data)
	{
	}

	public override void ReceiveNetworkData<T>(T data, ulong senderClientId)
	{
	}

	public override void Enter()
	{
	}

	public override void Update()
	{
	}

	public override void Exit()
	{
	}
}
