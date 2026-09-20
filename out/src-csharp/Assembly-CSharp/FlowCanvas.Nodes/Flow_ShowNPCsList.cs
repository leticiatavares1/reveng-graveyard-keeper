using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Show NPCs List Window", 0)]
[Category("Game Actions")]
public class Flow_ShowNPCsList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.game_gui.OpenAtTab(GameGUI.TabType.NPCs);
			flow_out.Call(f);
		});
	}
}
