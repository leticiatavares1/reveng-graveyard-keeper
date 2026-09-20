using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Open Time Machine GUI", 0)]
public class Flow_OpenTimeMachineGUI : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (!MainGame.me.player_char.control_enabled)
			{
				GS.SetPlayerEnable(player_enabled: true, affect_cinematic: false);
			}
			GUIElements.me.time_machine_gui.Open();
			flow_out.Call(f);
		});
	}
}
