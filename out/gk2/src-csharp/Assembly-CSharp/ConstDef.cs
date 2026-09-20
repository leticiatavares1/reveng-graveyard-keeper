using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ConstDef : BalanceBaseObject
{
	public enum ConstType
	{
		@bool,
		@int,
		@float,
		@string
	}

	[SerializeField]
	private ConstType type;

	[SerializeField]
	private bool boolValue;

	[SerializeField]
	private int intValue;

	[SerializeField]
	private float floatValue;

	[SerializeField]
	private string stringValue;

	public bool BoolValue => boolValue;

	public int IntValue => intValue;

	public float FloatValue => floatValue;

	public string StringValue => stringValue;

	public static ConstDef Get(string constName)
	{
		return GameBalance.Me.GetData<ConstDef>(constName);
	}
}
