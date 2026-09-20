using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Open Map", 0)]
public class Flow_OpenMap : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.game_gui.OpenAtTab(GameGUI.TabType.Map);
			flow_out.Call(f);
		});
	}
}
