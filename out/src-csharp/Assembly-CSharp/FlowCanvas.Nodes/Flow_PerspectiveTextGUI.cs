using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Perspective Text", 0)]
public class Flow_PerspectiveTextGUI : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_on_finished = AddFlowOutput("On finished");
		AddFlowInput("In", delegate(Flow f)
		{
			PerspectiveTextGUI componentInChildren = MainGame.me.ui_root.GetComponentInChildren<PerspectiveTextGUI>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.Init();
				componentInChildren.OpenSlidingText(delegate
				{
					flow_on_finished.Call(f);
				});
			}
			else
			{
				flow_on_finished.Call(f);
			}
			flow_out.Call(f);
		});
	}
}
