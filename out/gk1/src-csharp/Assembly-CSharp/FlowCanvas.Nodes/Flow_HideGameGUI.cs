using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Hide Game GUI", 0)]
public class Flow_HideGameGUI : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.game_gui.Hide();
			flow_out.Call(f);
		});
	}
}
