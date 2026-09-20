using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Unlock Tech", 0)]
public class Flow_UnlockTech : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> tech_id = AddValueInput<string>("tech id");
		ValueInput<bool> silent = AddValueInput<bool>("silent?");
		ValueInput<bool> show_tech = AddValueInput<bool>("show tech");
		AddFlowInput("In", delegate(Flow f)
		{
			if (silent.value)
			{
				MainGame.me.save.UnlockTech(tech_id.value);
				flow_out.Call(f);
			}
			else
			{
				GUIElements.me.tech_dialog.Open(GameBalance.me.GetData<TechDefinition>(tech_id.value), delegate
				{
					flow_out.Call(f);
				}, forced_unlock: true, reveal_tech: false, show_tech.value);
			}
		});
	}
}
