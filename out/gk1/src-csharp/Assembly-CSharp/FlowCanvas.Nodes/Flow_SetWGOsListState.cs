using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("CubeArrowStraight", false, "")]
[Description("Set WGOs List State")]
[Category("Game Actions")]
[Name("Set WGOs List State", 0)]
public class Flow_SetWGOsListState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<bool> in_state = AddValueInput<bool>("State");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<WorldGameObject> value = par_wgo.value;
			if (value == null)
			{
				Debug.LogError("FollowWGO error: WGOs List is null");
			}
			else
			{
				foreach (WorldGameObject item in value)
				{
					if (!(item == null))
					{
						item.gameObject.SetActive(in_state.value);
						item.Redraw();
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
