using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Open Dungeon Window", 0)]
[Category("Game Actions")]
public class Flow_OpenDungeonWindow : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.dungeon_window.Open();
			flow_out.Call(f);
		});
	}
}
