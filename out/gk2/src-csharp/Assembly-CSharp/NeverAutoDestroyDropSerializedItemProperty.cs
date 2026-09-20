using System;

[Serializable]
public sealed class NeverAutoDestroyDropSerializedItemProperty : SerializedItemProperty
{
	public override SerializedItemProperty Clone()
	{
		return new NeverAutoDestroyDropSerializedItemProperty();
	}
}
