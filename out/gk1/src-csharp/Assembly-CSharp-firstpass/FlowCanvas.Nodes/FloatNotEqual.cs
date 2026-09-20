using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("≠", 0)]
[Category("Logic Operators/Floats")]
public class FloatNotEqual : PureFunctionNode<bool, float, float>
{
	public override bool Invoke(float a, float b)
	{
		return a != b;
	}
}
