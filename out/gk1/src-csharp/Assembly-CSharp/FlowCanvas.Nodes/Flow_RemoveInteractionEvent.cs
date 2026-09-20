using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("If WGO is null, then self")]
[Icon("MinusDialogue", false, "")]
[Name("Remove Interaction Event", 0)]
[Category("Game Actions")]
public class Flow_RemoveInteractionEvent : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_txt = AddValueInput<string>("Event");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			string value = par_txt.value;
			if (worldGameObject.custom_interaction_events.Contains(value))
			{
				worldGameObject.custom_interaction_events.Remove(value);
			}
			worldGameObject.RedrawBubble();
			flow_out.Call(f);
		});
	}
}
