using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class VendorOrderData : ObjectLinkedToDefinition<VendorOrderDef>
{
	[SerializeField]
	private VendorOrderState state;

	[SerializeField]
	private int tier;

	[SerializeField]
	private SGuid guid;

	[SerializeField]
	private int count;

	[SerializeField]
	private bool isFinishedOnce;

	public SGuid Guid => guid;

	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			count = value;
		}
	}

	public bool IsFinishedOnce => isFinishedOnce;

	public VendorOrderState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
			if (state == VendorOrderState.Finished)
			{
				isFinishedOnce = true;
			}
		}
	}

	public int Tier
	{
		get
		{
			return tier;
		}
		set
		{
			tier = value;
		}
	}

	public VendorOrderData(string id)
		: base(id)
	{
		guid = new SGuid();
	}
}
