using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Logic Operators/Any")]
[Name("=", 0)]
public class AnyEqual : PureFunctionNode<bool, object, object>
{
	public override bool Invoke(object a, object b)
	{
		return object.Equals(a, b);
	}
}
