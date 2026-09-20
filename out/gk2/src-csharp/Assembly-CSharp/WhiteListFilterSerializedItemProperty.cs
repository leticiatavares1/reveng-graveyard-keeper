using System;
using UnityEngine;

[Serializable]
public sealed class WhiteListFilterSerializedItemProperty : SerializedItemProperty
{
	[SerializeField]
	private WhiteListItemFilter whiteList;

	public WhiteListItemFilter WhiteList
	{
		get
		{
			return whiteList;
		}
		set
		{
			whiteList = value;
		}
	}

	public WhiteListFilterSerializedItemProperty(WhiteListItemFilter whiteList)
	{
		this.whiteList = whiteList;
	}

	public override SerializedItemProperty Clone()
	{
		return new WhiteListFilterSerializedItemProperty(whiteList);
	}
}
