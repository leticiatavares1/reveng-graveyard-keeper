using LazyBearTechnology;
using UnityEngine;

public class LazyNetwork : LazySingleton<LazyNetwork>
{
	private static bool isInitialized;

	private BaseNetworkMessageChannelManager networkMessageChannelManager;

	private INetworkManager networkManager;

	private ConnectionManager connectionManager;

	public static INetworkManager NetworkManager => LazySingleton<LazyNetwork>.Instance.networkManager;

	public static BaseNetworkMessageChannelManager NetworkMessageChannelManager => LazySingleton<LazyNetwork>.Instance.networkMessageChannelManager;

	public static ConnectionManager ConnectionManager => LazySingleton<LazyNetwork>.Instance.connectionManager;

	public static bool IsInitialized
	{
		get
		{
			if (!Application.isPlaying)
			{
				return false;
			}
			return isInitialized;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Object.DontDestroyOnLoad(this);
	}

	public void Init(INetworkManager networkManager, BaseNetworkMessageChannelManager networkMessageChannelManager)
	{
		if (!isInitialized)
		{
			connectionManager = new ConnectionManager();
			this.networkManager = networkManager;
			this.networkMessageChannelManager = networkMessageChannelManager;
			InitComponents();
			isInitialized = true;
		}
	}

	private void InitComponents()
	{
		NetworkManager.Init();
		NetworkMessageChannelManager.Init();
		ConnectionManager.Init();
	}

	private void Update()
	{
		if (isInitialized)
		{
			ConnectionManager.Update();
		}
	}

	private void OnDestroy()
	{
		if (isInitialized)
		{
			ConnectionManager.DeInit();
			NetworkMessageChannelManager.DeInit();
			NetworkManager.DeInit();
		}
	}
}
