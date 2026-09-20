using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Open craft GUI")]
[Name("Can Insert Item Into WGO", 0)]
public class Flow_CanInsertItemIntoWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			if (base.wgo.CanInsertItem(in_item.value))
			{
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
		});
	}
}
