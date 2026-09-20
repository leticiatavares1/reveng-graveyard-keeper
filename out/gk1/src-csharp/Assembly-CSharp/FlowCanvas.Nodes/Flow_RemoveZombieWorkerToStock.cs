using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Remove Zombie Worker To Stock", 0)]
[Category("Game Actions")]
public class Flow_RemoveZombieWorkerToStock : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_gd_point_tag = AddValueInput<string>("GD Point Tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				string text = WorldMap.RemoveZombieWorkerToStock(worldGameObject, in_gd_point_tag.value);
				if (!string.IsNullOrEmpty(text))
				{
					Debug.LogError(text);
				}
				flow_out.Call(f);
			}
		});
	}
}
