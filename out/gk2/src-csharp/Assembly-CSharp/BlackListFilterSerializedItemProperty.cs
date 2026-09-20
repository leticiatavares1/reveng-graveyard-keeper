using System;
using UnityEngine;

[Serializable]
public sealed class BlackListFilterSerializedItemProperty : SerializedItemProperty
{
	[SerializeField]
	private BlackListItemFilter blackList;

	public BlackListItemFilter BlackList
	{
		get
		{
			return blackList;
		}
		set
		{
			blackList = value;
		}
	}

	public BlackListFilterSerializedItemProperty(BlackListItemFilter blackList)
	{
		this.blackList = blackList;
	}

	public override SerializedItemProperty Clone()
	{
		return new BlackListFilterSerializedItemProperty(blackList);
	}
}
