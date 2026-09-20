using UnityEngine;

public class ConnectionManager
{
	private NetworkPackageSystem networkPackageSystem;

	private ConnectionState currentState;

	public OfflineState OfflineState { get; private set; }

	public StartedHostState StartedHostState { get; private set; }

	public ConnectedToHostState ConnectedToHostState { get; private set; }

	public ConnectionState CurrentState => currentState;

	public NetworkPackageSystem NetworkPackageSystem => networkPackageSystem;

	public void Init()
	{
		networkPackageSystem = new NetworkPackageSystem(LazyNetwork.NetworkManager.MaxCommandPackageQueue);
		InitStates();
		SubscribeToNetworkEvents();
	}

	public void Update()
	{
		currentState.Update();
	}

	public void DeInit()
	{
		UnsubscribeFromNetworkEvents();
	}

	private void InitStates()
	{
		OfflineState = new OfflineState(this);
		StartedHostState = new StartedHostState(this, networkPackageSystem);
		ConnectedToHostState = new ConnectedToHostState(this, networkPackageSystem);
		currentState = OfflineState;
	}

	private void SubscribeToNetworkEvents()
	{
		LazyNetwork.NetworkManager.OnServerStarted += OnServerStarted;
		LazyNetwork.NetworkManager.OnServerStopped += OnServerStopped;
		LazyNetwork.NetworkManager.OnClientStarted += OnClientStarted;
		LazyNetwork.NetworkManager.OnClientStopped += OnClientStopped;
	}

	private void UnsubscribeFromNetworkEvents()
	{
		LazyNetwork.NetworkManager.OnServerStarted -= OnServerStarted;
		LazyNetwork.NetworkManager.OnServerStopped -= OnServerStopped;
		LazyNetwork.NetworkManager.OnClientStarted -= OnClientStarted;
		LazyNetwork.NetworkManager.OnClientStopped -= OnClientStopped;
	}

	private void ChangeState(ConnectionState nextState)
	{
		Debug.Log("Changed connection state from " + currentState.GetType().Name + " to " + nextState.GetType().Name + ".");
		currentState?.Exit();
		currentState = nextState;
		currentState.Enter();
	}

	private void OnServerStarted()
	{
		ChangeState(StartedHostState);
	}

	private void OnServerStopped()
	{
		ChangeState(OfflineState);
	}

	private void OnClientStarted()
	{
		ChangeState(ConnectedToHostState);
	}

	private void OnClientStopped()
	{
		ChangeState(OfflineState);
	}
}
