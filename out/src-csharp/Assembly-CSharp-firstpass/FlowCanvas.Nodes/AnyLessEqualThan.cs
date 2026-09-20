using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("≤", 0)]
[Category("Logic Operators/Any")]
public class AnyLessEqualThan : PureFunctionNode<bool, IComparable, IComparable>
{
	public override bool Invoke(IComparable a, IComparable b)
	{
		if (a.CompareTo(b) != -1)
		{
			return object.Equals(a, b);
		}
		return true;
	}
}
