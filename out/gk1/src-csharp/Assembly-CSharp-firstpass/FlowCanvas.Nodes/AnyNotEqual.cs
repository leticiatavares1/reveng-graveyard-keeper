using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("≠", 0)]
[Category("Logic Operators/Any")]
public class AnyNotEqual : PureFunctionNode<bool, object, object>
{
	public override bool Invoke(object a, object b)
	{
		return !object.Equals(a, b);
	}
}
