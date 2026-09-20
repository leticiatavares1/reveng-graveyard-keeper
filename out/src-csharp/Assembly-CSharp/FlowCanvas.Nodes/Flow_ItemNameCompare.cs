using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Compare Item name with string", 0)]
public class Flow_ItemNameCompare : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> par_param = AddValueInput<string>("param");
		FlowOutput flow_same = AddFlowOutput("==");
		FlowOutput flow_not_the_same = AddFlowOutput("!=");
		AddFlowInput("In", delegate(Flow f)
		{
			Item value = in_item.value;
			if (value == null)
			{
				if (par_param.isDefaultValue)
				{
					flow_same.Call(f);
				}
				else
				{
					flow_not_the_same.Call(f);
				}
			}
			else if (value.id == par_param.value)
			{
				flow_same.Call(f);
			}
			else
			{
				flow_not_the_same.Call(f);
			}
		});
	}
}
