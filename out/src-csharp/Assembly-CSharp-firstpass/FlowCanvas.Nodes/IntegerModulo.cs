using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Logic Operators/Integers")]
[Name("%", 0)]
public class IntegerModulo : PureFunctionNode<int, int, int>
{
	public override int Invoke(int value, int mod)
	{
		return value % mod;
	}
}
