using System;
using UnityEngine;

[Serializable]
public sealed class DurabilitySerializedItemProperty : SerializedItemProperty
{
	[SerializeField]
	private float durability;

	public float Durability
	{
		get
		{
			return durability;
		}
		set
		{
			durability = value;
		}
	}

	public DurabilitySerializedItemProperty(float durability)
	{
		this.durability = durability;
	}

	public override SerializedItemProperty Clone()
	{
		return new DurabilitySerializedItemProperty(durability);
	}
}
