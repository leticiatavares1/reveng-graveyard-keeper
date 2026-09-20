using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Store Object", 0)]
public class Flow_StoreObject : MyFlowNode
{
	private object _obj;

	protected override void RegisterPorts()
	{
		ValueInput<object> in_object = AddValueInput<object>("Object");
		AddValueOutput("out Object", () => _obj);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			_obj = in_object.value;
			flow_out.Call(f);
		});
	}
}
