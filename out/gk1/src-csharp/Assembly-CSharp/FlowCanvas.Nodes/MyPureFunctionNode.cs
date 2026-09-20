using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[DoNotList]
public abstract class MyPureFunctionNode<TResult> : PureFunctionNodeBase
{
	private FlowNode _node;

	public WorldGameObject wgo => MyFlowNode.GetWGOFromNode(_node);

	public abstract TResult Invoke();

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		node.AddValueOutput("Value", delegate
		{
			_node = node;
			return Invoke();
		});
	}
}
