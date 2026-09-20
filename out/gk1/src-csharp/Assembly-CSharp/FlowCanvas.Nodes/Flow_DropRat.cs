using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Drop Rat", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Color("857fff")]
[Icon("ArrowDown", false, "")]
public class Flow_DropRat : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO to drop");
		ValueInput<Direction> par_direction = AddValueInput<Direction>("Direction");
		ValueInput<string> par_rat_name = AddValueInput<string>("rat name");
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
				string text = par_rat_name.value;
				if (string.IsNullOrEmpty(text))
				{
					List<ItemDefinition> list = new List<ItemDefinition>();
					foreach (ItemDefinition items_datum in GameBalance.me.items_data)
					{
						if (items_datum.type == ItemDefinition.ItemType.Rat)
						{
							list.Add(items_datum);
						}
					}
					text = list[UnityEngine.Random.Range(0, list.Count)].id;
				}
				Item item = new Item(text)
				{
					inventory_size = 999
				};
				item.AddItem(new Item("rat_status:normal"));
				worldGameObject.DropItem(item, par_direction.value, default(Vector3), (par_direction.value == Direction.None) ? 1f : 3f, par_direction.value == Direction.None);
				flow_out.Call(f);
			}
		});
	}
}
