using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes.Refugees;

[Name("Spawn Next Refugee", 0)]
[Category("Game Actions/Refugees")]
public class Flow_SpawnNextRefugee : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GameObject> point_in;

	private ValueInput<string> home_gd_point;

	private ValueOutput<WorldGameObject> wgo_out;

	private WorldGameObject wgo_out_value;

	public override string name
	{
		get
		{
			if (string.IsNullOrEmpty(home_gd_point.value))
			{
				return base.name + "\n<color=#FF2020>NULL OR EMPTY HOME GD TAG</color>";
			}
			return base.name;
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", SpawnRefugee);
		@out = AddFlowOutput("Out");
		point_in = AddValueInput<GameObject>("Spawn Point");
		home_gd_point = AddValueInput<string>("Home GD Point Tag");
		wgo_out = AddValueOutput("WGO", () => wgo_out_value);
	}

	private void SpawnRefugee(Flow flow)
	{
		if (point_in.value != null)
		{
			wgo_out_value = RefugeesCampEngine.instance.SpawnNextRefugeeAtTransform(point_in.value.transform, home_gd_point.value);
		}
		else
		{
			Debug.LogError("Point value is null");
		}
		@out.Call(flow);
	}
}
