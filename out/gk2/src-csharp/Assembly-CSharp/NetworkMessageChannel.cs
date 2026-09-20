using System;

public abstract class NetworkMessageChannel<T> : NetworkMessageChannelBase
{
	public event Action<T, ulong> OnMessageReceive;

	public abstract void Publish(T data, ulong customClientId = 0uL);

	protected void NotifyOnMessageReceive(T type, ulong senderClientId)
	{
		this.OnMessageReceive?.Invoke(type, senderClientId);
	}
}
