using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("PlusDialogue", false, "")]
[Name("Add Interaction Event", 0)]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
public class Flow_AddInteractionEvent : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_txt = AddValueInput<string>("Event");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_AddInteractionEvent: WGO is null");
			}
			else
			{
				worldGameObject.AddInteractionEvent(par_txt.value);
				flow_out.Call(f);
			}
		});
	}
}
