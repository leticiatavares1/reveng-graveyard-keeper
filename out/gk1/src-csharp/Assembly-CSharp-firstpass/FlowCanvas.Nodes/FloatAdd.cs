using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("+", 0)]
[Category("Logic Operators/Floats")]
public class FloatAdd : PureFunctionNode<float, float, float>
{
	public override float Invoke(float a, float b)
	{
		return a + b;
	}
}
