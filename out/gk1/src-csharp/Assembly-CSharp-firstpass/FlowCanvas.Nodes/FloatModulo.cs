using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Logic Operators/Floats")]
[Name("%", 0)]
public class FloatModulo : PureFunctionNode<float, float, float>
{
	public override float Invoke(float value, float mod)
	{
		return value % mod;
	}
}
