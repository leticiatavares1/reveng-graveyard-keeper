using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Logic Operators/Any")]
[Name("<", 0)]
public class AnyLessThan : PureFunctionNode<bool, IComparable, IComparable>
{
	public override bool Invoke(IComparable a, IComparable b)
	{
		return a.CompareTo(b) == -1;
	}
}
