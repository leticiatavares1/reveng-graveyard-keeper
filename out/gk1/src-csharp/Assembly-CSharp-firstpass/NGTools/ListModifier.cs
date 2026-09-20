using System;
using System.Collections;

namespace NGTools;

public class ListModifier : ICollectionModifier
{
	public IList list;

	public int Size => list.Count;

	public Type Type => Utility.GetArraySubType(list.GetType());

	public ListModifier(IList list)
	{
		this.list = list;
	}

	public object Get(int index)
	{
		return list[index];
	}

	public void Set(int index, object value)
	{
		list[index] = value;
	}
}
