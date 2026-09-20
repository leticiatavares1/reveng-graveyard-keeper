using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Spawn Zombie Worker From Stock", 0)]
[Category("Game Actions")]
public class Flow_SpawnZombieWorkerFromStock : MyFlowNode
{
	private WorldGameObject out_o;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_workbench_wgo = AddValueInput<WorldGameObject>("Workbench");
		ValueInput<Item> par_worker_item = AddValueInput<Item>("worker item");
		AddValueOutput("WGO", () => out_o);
		FlowOutput flow_success = AddFlowOutput("Succeed");
		FlowOutput flow_fail = AddFlowOutput("Failed");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject o = null;
			bool is_success;
			string text = WorldMap.SpawnZombieWorkerFromStock(par_workbench_wgo.value, par_worker_item.value, out o, out is_success);
			if (!string.IsNullOrEmpty(text))
			{
				Debug.LogError(text);
			}
			out_o = o;
			if (is_success)
			{
				flow_success.Call(f);
			}
			else
			{
				flow_fail.Call(f);
			}
		});
	}
}
