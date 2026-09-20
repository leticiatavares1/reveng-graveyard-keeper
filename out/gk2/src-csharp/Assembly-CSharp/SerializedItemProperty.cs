using System;

[Serializable]
public abstract class SerializedItemProperty
{
	public virtual SerializedItemProperty Clone()
	{
		return (SerializedItemProperty)Activator.CreateInstance(GetType());
	}
}
