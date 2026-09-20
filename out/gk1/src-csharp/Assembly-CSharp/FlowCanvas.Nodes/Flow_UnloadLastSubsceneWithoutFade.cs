using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Icon("CubeArrowStraight", false, "")]
[Description("Removes last additive added subscene without fade and camera moving")]
[Category("Game Actions")]
[Name("Just Unload Last Subscene", 0)]
public class Flow_UnloadLastSubsceneWithoutFade : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			SubsceneLoadManager.UnloadLastScene();
			flow_out.Call(f);
		});
	}
}
