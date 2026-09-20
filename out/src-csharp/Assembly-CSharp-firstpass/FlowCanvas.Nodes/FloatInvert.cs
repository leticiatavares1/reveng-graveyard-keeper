using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Invert", 0)]
[Description("Inverts the input ( value = value * -1 )")]
[Category("Logic Operators/Floats")]
public class FloatInvert : PureFunctionNode<float, float>
{
	public override float Invoke(float value)
	{
		return value * -1f;
	}
}
