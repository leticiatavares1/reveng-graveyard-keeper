using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Get Overhead Item", 0)]
[Description("If Character is null, then Player")]
[Icon("Cube", false, "")]
public class Flow_GetOverhead : MyFlowNode
{
	private Item item;

	private string item_id = string.Empty;

	protected override void RegisterPorts()
	{
		AddValueOutput("Item", () => item);
		AddValueOutput("Item ID", () => item_id);
		FlowOutput flow_no_overhead = AddFlowOutput("No overhead");
		FlowOutput flow_has_overhead = AddFlowOutput("Has overhead");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			item = MainGame.me.player.components.character.GetOverheadItem();
			if (item != null)
			{
				item_id = item.id;
			}
			if (item == null)
			{
				flow_no_overhead.Call(f);
			}
			else
			{
				flow_has_overhead.Call(f);
			}
			flow_out.Call(f);
		});
	}
}
