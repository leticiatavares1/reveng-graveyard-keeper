using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("≠", 0)]
[Category("Logic Operators/Boolean")]
public class BooleanNotEqual : PureFunctionNode<bool, bool, bool>
{
	public override bool Invoke(bool a, bool b)
	{
		return a != b;
	}
}
