using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Activate Spawners", 0)]
[Description("Activate Spawners by custom tag")]
[Icon("CubePlus", false, "")]
public class Flow_ActivateSpawners : MyFlowNode
{
	private const float _ACTIVATION_PERIOD = 0.1f;

	protected override void RegisterPorts()
	{
		ValueInput<string> par_custom_tag = AddValueInput<string>("Custom tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<MobSpawner> mobSpawnersByCustomTag = WorldMap.GetMobSpawnersByCustomTag(par_custom_tag.value);
			float num = 0f;
			foreach (MobSpawner item in mobSpawnersByCustomTag)
			{
				MobSpawner spawner = item;
				GJTimer.AddTimer(num, delegate
				{
					if (spawner.spawned_mobs != null && spawner.spawned_mobs.Count > 0)
					{
						foreach (WorldGameObject item2 in new List<WorldGameObject>(spawner.spawned_mobs))
						{
							item2.DestroyMe();
						}
					}
					spawner.spawned_mobs = new List<WorldGameObject>();
					spawner.ActivateSpawner();
				});
				num += 0.1f;
			}
			flow_out.Call(f);
		});
	}
}
