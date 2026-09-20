using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get WGO's Obj ID", 0)]
[Category("Game Actions")]
public class Flow_GetWGOObjId : MyFlowNode
{
	private string _out_str = string.Empty;

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("value", () => _out_str);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_GetWGOObjId: WGO is null");
				flow_out.Call(f);
			}
			else
			{
				_out_str = worldGameObject.obj_id;
				flow_out.Call(f);
			}
		});
	}
}
