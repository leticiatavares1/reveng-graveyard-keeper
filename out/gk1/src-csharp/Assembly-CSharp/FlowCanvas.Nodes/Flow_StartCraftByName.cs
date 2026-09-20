using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Start Craft WGO")]
[Name("Start Craft By Name", 0)]
[Category("Game Actions")]
public class Flow_StartCraftByName : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_craft_name = AddValueInput<string>("craft name");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Can not ctart craft: WGO is null!");
			}
			else if (string.IsNullOrEmpty(in_craft_name.value))
			{
				Debug.LogError("Can not ctart craft: craft name is empty");
			}
			else
			{
				worldGameObject.TryStartCraft(in_craft_name.value);
				flow_out.Call(f);
			}
		});
	}
}
