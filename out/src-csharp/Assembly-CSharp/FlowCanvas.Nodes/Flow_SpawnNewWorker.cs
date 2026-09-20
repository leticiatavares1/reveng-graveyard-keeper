using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Spawn New Worker", 0)]
public class Flow_SpawnNewWorker : MyFlowNode
{
	private Item out_item;

	private WorldGameObject out_wgo;

	protected override void RegisterPorts()
	{
		ValueInput<string> in_worker_id = AddValueInput<string>("worker_id");
		ValueInput<GDPoint> in_spawn_point = AddValueInput<GDPoint>("spawn_point");
		ValueInput<Item> in_base_body = AddValueInput<Item>("base_body");
		AddValueOutput("worker_wgo", () => out_wgo);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(in_worker_id.value))
			{
				Debug.LogError("Flow_SpawnNewWorker error: worker_id is null!");
			}
			else
			{
				WorkerDefinition data = GameBalance.me.GetData<WorkerDefinition>(in_worker_id.value);
				if (data == null)
				{
					Debug.LogError("Flow_SpawnNewWorker error: worker_definition is null!");
				}
				else if (in_spawn_point.value == null)
				{
					Debug.LogError("Flow_SpawnNewWorker error: spawn_point is null!");
				}
				else
				{
					Item item = in_base_body.value;
					if (item == null || item.IsEmpty())
					{
						item = MainGame.me.save.GenerateBody(1, 3);
						Debug.LogError("While spawning new zombie worker created new body: base_body was null or empty!");
					}
					if (item == null || item.IsEmpty())
					{
						Debug.LogError("Flow_SpawnNewWorker error: base_body is null or empty!");
					}
					else if (item.definition.type != ItemDefinition.ItemType.Body)
					{
						Debug.LogError("Flow_SpawnNewWorker error: base_body is NOT a body!");
					}
					else
					{
						WorldGameObject zombie_wgo = WorldMap.SpawnWGO(MainGame.me.world_root, data.worker_wgo, in_spawn_point.value.pos);
						MainGame.me.save.workers.CreateNewWorker(zombie_wgo, in_worker_id.value, item);
						out_wgo = zombie_wgo;
						flow_out.Call(f);
					}
				}
			}
		});
	}
}
