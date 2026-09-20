using System;
using System.Collections.Generic;

public class UNetworkMessageChannelManager : BaseNetworkMessageChannelManager
{
	private List<NetworkMessageChannelBase> messageChannelsInternal;

	public override void Init()
	{
		SetupMessageChannels(new List<NetworkMessageChannelBase>
		{
			new UCommandMessageChannel(),
			new UCommandPackageMessageChannel(),
			new UOrderMessageChannel(),
			new UGameSaveMessageChannel()
		});
	}

	public override void DeInit()
	{
	}

	private void SetupMessageChannels(List<NetworkMessageChannelBase> messageChannelsList)
	{
		foreach (NetworkMessageChannelBase messageChannels in messageChannelsList)
		{
			Type genericType = GetGenericType(messageChannels);
			base.messageChannels.Add(genericType, messageChannels);
		}
	}

	private Type GetGenericType(NetworkMessageChannelBase messageChannel)
	{
		return messageChannel.GetType().BaseType.GetGenericArguments()[0];
	}
}
