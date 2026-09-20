using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes.Refugees;

[Category("Game Actions/Refugees")]
[Name("Add Place Items To Tents", 0)]
public class Flow_AddPlaceItemsToTent : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WorldGameObject> tent_object_in;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", delegate(Flow flow)
		{
			if (tent_object_in.value != null)
			{
				RefugeesCampEngine.instance.AddItemsOnTentAppear(tent_object_in.value);
			}
			else
			{
				Debug.LogError("Tent WGO is null");
			}
			@out.Call(flow);
		});
		@out = AddFlowOutput("Out");
		tent_object_in = AddValueInput<WorldGameObject>("Tent WGO");
	}
}
