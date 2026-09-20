using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Is Tech Unlocked", 0)]
[Category("Game Actions")]
public class Flow_IsTechUnlocked : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> tech_id = AddValueInput<string>("tech id");
		FlowOutput flow_no = AddFlowOutput("Not unlocked");
		FlowOutput flow_yes = AddFlowOutput("Unlocked");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(tech_id.value))
			{
				flow_no.Call(f);
			}
			else if (MainGame.me.save.unlocked_techs.Contains(tech_id.value))
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
