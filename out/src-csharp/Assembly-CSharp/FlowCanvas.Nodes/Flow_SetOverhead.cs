using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("If Character is null, then Player")]
[Icon("Cube", false, "")]
[Category("Game Actions")]
[Name("Set Overhead Item", 0)]
public class Flow_SetOverhead : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			BaseCharacterComponent player_char = MainGame.me.player_char;
			if ((in_item.value == null || in_item.value.IsEmpty()) && string.IsNullOrEmpty(in_item_id.value))
			{
				player_char.SetOverheadItem(null);
			}
			else
			{
				Item overheadItem = ((in_item.value == null || in_item.value.IsEmpty()) ? new Item(in_item_id.value, 1) : in_item.value);
				player_char.SetOverheadItem(overheadItem);
			}
			flow_out.Call(f);
		});
	}
}
