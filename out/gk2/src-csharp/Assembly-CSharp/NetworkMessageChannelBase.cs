using System;

public abstract class NetworkMessageChannelBase
{
	public abstract void OnReceive(ulong senderClientId, IDisposable messagePayload);

	public virtual void Register()
	{
	}

	public virtual void Unregister()
	{
	}
}
