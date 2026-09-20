using System;

namespace NGTools;

public class ArrayModifier : ICollectionModifier
{
	public Array array;

	public int Size => array.Length;

	public Type Type => Utility.GetArraySubType(array.GetType());

	public ArrayModifier(Array array)
	{
		this.array = array;
	}

	public object Get(int index)
	{
		return array.GetValue(index);
	}

	public void Set(int index, object value)
	{
		array.SetValue(value, index);
	}
}
