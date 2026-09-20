using System;
using System.Collections.Generic;

public interface INetworkManager
{
	bool IsHost { get; }

	bool IsClient { get; }

	ulong MyId { get; }

	bool IsCoopGame { get; }

	int MaxCommandPackageQueue { get; }

	int MaxPayloadSize { get; }

	ulong ServerClientId { get; }

	List<ulong> OtherClients { get; }

	event Action OnServerStarted;

	event Action OnServerStopped;

	event Action OnClientStarted;

	event Action OnClientStopped;

	event Action<ulong> OnClientConnected;

	event Action<ulong> OnClientDisconnected;

	void Init();

	void DeInit();

	bool StartHostGame(string ip, ushort port);

	bool ConnectToHost(string ip, ushort port);
}
