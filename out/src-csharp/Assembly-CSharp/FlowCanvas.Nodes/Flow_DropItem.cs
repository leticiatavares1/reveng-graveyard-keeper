using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Drop Item", 0)]
[Description("If WGO is null, then self")]
[Color("857fff")]
[Icon("ArrowDown", false, "")]
public class Flow_DropItem : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO to drop");
		ValueInput<string> par_res = AddValueInput<string>("Res_id");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<int> par_amount = AddValueInput<int>("Amount");
		ValueInput<Direction> par_direction = AddValueInput<Direction>("Direction");
		FlowOutput flow_out = AddFlowOutput("Out");
		par_amount.serializedValue = 1;
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				Item item;
				if (in_item.value == null || in_item.value.IsEmpty())
				{
					item = new Item(par_res.value, par_amount.value);
				}
				else
				{
					item = in_item.value;
					if (par_amount.value > 0)
					{
						item.value = par_amount.value;
					}
				}
				Debug.Log("Drop item " + item.id + ", n = " + item.value);
				worldGameObject.DropItem(item, par_direction.value, default(Vector3), (par_direction.value == Direction.None) ? 1f : 3f, par_direction.value == Direction.None);
				flow_out.Call(f);
			}
		});
	}
}
