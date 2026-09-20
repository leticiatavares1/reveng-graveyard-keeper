using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Logic Operators/Floats")]
[Name("÷", 0)]
public class FloatDivide : PureFunctionNode<float, float, float>
{
	public override float Invoke(float a, float b)
	{
		return a / b;
	}
}
