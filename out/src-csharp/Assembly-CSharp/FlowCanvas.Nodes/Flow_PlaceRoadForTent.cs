using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Place Road For Tent", 0)]
[Category("Game Actions/Refugees")]
public class Flow_PlaceRoadForTent : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WorldGameObject> tent_wgo_in_par;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", delegate(Flow flow)
		{
			if (tent_wgo_in_par.value != null)
			{
				RefugeesCampEngine.instance.PlaceRoadAndAuxiliaryForTent(tent_wgo_in_par.value);
			}
			else
			{
				Debug.LogError("Tent WGO is null");
			}
			@out.Call(flow);
		});
		@out = AddFlowOutput("Out");
		tent_wgo_in_par = AddValueInput<WorldGameObject>("Tent WGO");
	}
}
