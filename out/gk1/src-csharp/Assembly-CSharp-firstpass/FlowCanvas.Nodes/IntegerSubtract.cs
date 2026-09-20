using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("-", 0)]
[Category("Logic Operators/Integers")]
public class IntegerSubtract : PureFunctionNode<int, int, int>
{
	public override int Invoke(int a, int b)
	{
		return a - b;
	}
}
