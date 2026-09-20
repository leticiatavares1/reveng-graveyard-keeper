using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Is WGO in Range", 0)]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
public class Flow_IsWGOInRange : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo1 = AddValueInput<WorldGameObject>("WGO #1");
		ValueInput<WorldGameObject> par_wgo2 = AddValueInput<WorldGameObject>("WGO #2");
		ValueInput<float> par_range = AddValueInput<float>("Range");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_yes = AddFlowOutput("In range");
		FlowOutput flow_no = AddFlowOutput("Out of range");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo1);
			if (!(worldGameObject == null))
			{
				if (par_wgo2.value == null)
				{
					Debug.LogError("Flow_IsWGOInRange: WGO #2 is not set");
				}
				else
				{
					if (worldGameObject.IsInRange(par_wgo2.value, par_range.value))
					{
						flow_yes.Call(f);
					}
					else
					{
						flow_no.Call(f);
					}
					flow_out.Call(f);
				}
			}
		});
	}
}
