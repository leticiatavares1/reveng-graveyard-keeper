using System;
using System.Collections.Generic;

public abstract class BaseNetworkMessageChannelManager
{
	protected Dictionary<Type, NetworkMessageChannelBase> messageChannels = new Dictionary<Type, NetworkMessageChannelBase>();

	public abstract void Init();

	public abstract void DeInit();

	public void RegisterMessageChannels()
	{
		foreach (NetworkMessageChannelBase value in messageChannels.Values)
		{
			value.Register();
		}
	}

	public void UnregisterMessageChannels()
	{
		foreach (NetworkMessageChannelBase value in messageChannels.Values)
		{
			value.Unregister();
		}
	}

	public void Publish<T>(T data, ulong customClientId = 0uL)
	{
		if (messageChannels.TryGetValue(typeof(T), out var value) && value is NetworkMessageChannel<T> networkMessageChannel)
		{
			networkMessageChannel.Publish(data, customClientId);
		}
	}

	public void AddListener<T>(Action<T, ulong> callback)
	{
		if (messageChannels.TryGetValue(typeof(T), out var value) && value is NetworkMessageChannel<T> networkMessageChannel)
		{
			networkMessageChannel.OnMessageReceive += callback;
		}
	}

	public void RemoveListener<T>(Action<T, ulong> callback)
	{
		if (messageChannels.TryGetValue(typeof(T), out var value) && value is NetworkMessageChannel<T> networkMessageChannel)
		{
			networkMessageChannel.OnMessageReceive -= callback;
		}
	}

	public NetworkMessageChannelBase GetNetworkMessageChannel<T>()
	{
		if (messageChannels.TryGetValue(typeof(T), out var value) && value is NetworkMessageChannel<T> result)
		{
			return result;
		}
		return null;
	}
}
