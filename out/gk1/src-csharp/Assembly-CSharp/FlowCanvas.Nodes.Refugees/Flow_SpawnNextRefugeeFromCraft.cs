using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes.Refugees;

[Name("Spawn Next Refugee From Craft", 0)]
[Category("Game Actions/Refugees")]
public class Flow_SpawnNextRefugeeFromCraft : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GameObject> point_in;

	private ValueOutput<WorldGameObject> wgo_out;

	private WorldGameObject wgo_out_value;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", SpawnRefugee);
		@out = AddFlowOutput("Out");
		point_in = AddValueInput<GameObject>("Point");
		wgo_out = AddValueOutput("WGO", () => wgo_out_value);
	}

	private void SpawnRefugee(Flow flow)
	{
		if (point_in.value != null)
		{
			wgo_out_value = RefugeesCampEngine.instance.SpawnNextRefugeeAtTransformFromCraft(point_in.value.transform);
		}
		else
		{
			Debug.LogError("Point value is null");
		}
		@out.Call(flow);
	}
}
