using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Show ending titles", 0)]
[Category("Game Actions")]
public class Flow_ShowEndingTitles : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.saves.StopPlayingGame();
			GUIElements.me.credits.OpenScrolling();
			flow_out.Call(f);
		});
	}
}
