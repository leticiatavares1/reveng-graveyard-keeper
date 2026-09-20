using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[ExposeAsDefinition]
[Name("Switch", 8)]
[Category("Utility")]
[Description("Returns either one of the two inputs, based on the boolean condition")]
public class SwitchValue<T> : PureFunctionNode<T, bool, T, T>
{
	public override T Invoke(bool condition, T isTrue, T isFalse)
	{
		if (!condition)
		{
			return isFalse;
		}
		return isTrue;
	}
}
