using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Next Variation", 0)]
[Category("Game Actions")]
public class Flow_NextVariation : MyFlowNode
{
	public bool is_next;

	public override string name
	{
		get
		{
			if (!is_next)
			{
				return "Previous Variation";
			}
			return "Next Variation";
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_SetVariationByIndex error: WGO is null!");
				flow_out.Call(f);
			}
			else
			{
				if (is_next)
				{
					worldGameObject.NextVariationRadiobutton();
				}
				else
				{
					worldGameObject.PrevVariationRadiobutton();
				}
				flow_out.Call(f);
			}
		});
	}
}
