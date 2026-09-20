using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Color("857fff")]
[Icon("ArrowDown", false, "")]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Drop Body", 0)]
public class Flow_DropBody : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO to drop");
		ValueInput<Direction> par_direction = AddValueInput<Direction>("Direction");
		ValueInput<int> par_tier_min = AddValueInput<int>("Tier min");
		ValueInput<int> par_tier_max = AddValueInput<int>("Tier max");
		ValueInput<int> par_tier_soul_min = AddValueInput<int>("Tier min(soul)");
		ValueInput<int> par_tier_soul_max = AddValueInput<int>("Tier max(soul)");
		ValueInput<float> par_dec_durability = AddValueInput<float>("dec durability [0..100]");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				Item item = MainGame.me.save.GenerateBody(par_tier_min.value, par_tier_max.value, par_tier_soul_min.value, par_tier_soul_max.value);
				if ((double)par_dec_durability.value > 0.1)
				{
					item.durability = 1f - par_dec_durability.value / 100f;
				}
				worldGameObject.DropItem(item, par_direction.value, default(Vector3), (par_direction.value == Direction.None) ? 1f : 3f, par_direction.value == Direction.None);
				flow_out.Call(f);
			}
		});
	}
}
