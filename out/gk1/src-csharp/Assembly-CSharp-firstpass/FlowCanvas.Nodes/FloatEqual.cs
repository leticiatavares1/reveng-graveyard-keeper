using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Logic Operators/Floats")]
[Name("=", 0)]
public class FloatEqual : PureFunctionNode<bool, float, float>
{
	public override bool Invoke(float a, float b)
	{
		return a == b;
	}
}
