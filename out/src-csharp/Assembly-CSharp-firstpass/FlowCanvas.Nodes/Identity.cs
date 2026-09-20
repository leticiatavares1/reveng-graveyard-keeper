using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Use this for organization. It returns exactly what is provided in the input.")]
[Name("Identity", 10)]
[ExposeAsDefinition]
[Category("Utility")]
public class Identity<T> : PureFunctionNode<T, T>
{
	public override string name => null;

	public override T Invoke(T value)
	{
		return value;
	}
}
