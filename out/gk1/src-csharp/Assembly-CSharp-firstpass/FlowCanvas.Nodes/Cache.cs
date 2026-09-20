using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Utility")]
[ExposeAsDefinition]
[Description("Caches the value only when the node is called.")]
[Name("Cache", 9)]
public class Cache<T> : CallableFunctionNode<T, T>
{
	public override T Invoke(T value)
	{
		return value;
	}
}
