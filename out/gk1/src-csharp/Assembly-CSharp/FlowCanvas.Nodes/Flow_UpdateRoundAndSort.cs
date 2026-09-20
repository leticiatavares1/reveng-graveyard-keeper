using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Update RoundAndSort", 0)]
[Category("Game Actions")]
[Description("Update RoundAndSort component for wgo. If wgo is null then self")]
public class Flow_UpdateRoundAndSort : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		WorldGameObject _wgo = null;
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("WGO", () => _wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = (_wgo = WGOParamOrSelf(par_wgo));
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_UpdateRoundAndSort: WGO is null");
				flow_out.Call(f);
			}
			else
			{
				RoundAndSortComponent component = worldGameObject.GetComponent<RoundAndSortComponent>();
				if (component != null)
				{
					component.DoUpdateStuff();
				}
				flow_out.Call(f);
			}
		});
	}
}
