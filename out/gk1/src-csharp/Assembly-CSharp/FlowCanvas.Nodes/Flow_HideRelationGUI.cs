using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Hide relation GUI", 0)]
[Description("Used for cutscenes, where relation GUI is unnecessary")]
[Category("Game Actions")]
public class Flow_HideRelationGUI : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.relation.Hide();
			flow_out.Call(f);
		});
	}
}
