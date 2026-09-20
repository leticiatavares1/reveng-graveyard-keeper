using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Is Craft Unlocked", 0)]
[Category("Game Actions")]
public class Flow_IsCraftUnlocked : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> craft_id = AddValueInput<string>("craft id");
		FlowOutput flow_no = AddFlowOutput("Not unlocked");
		FlowOutput flow_yes = AddFlowOutput("Unlocked");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(craft_id.value))
			{
				flow_no.Call(f);
			}
			else if (MainGame.me.save.unlocked_crafts.Contains(craft_id.value))
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
