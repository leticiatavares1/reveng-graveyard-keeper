using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Color("857fff")]
[Name("Extract Soul", 0)]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Icon("ArrowDown", false, "")]
public class Flow_GenerateSoul : MyFlowNode
{
	private Item out_soul_item_value;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO to drop");
		AddValueInput<Direction>("Direction");
		ValueInput<float> par_dec_durability = AddValueInput<float>("dec durability [0..100]");
		ValueInput<float> min_damage_value = AddValueInput<float>("Min damage value [0..1]");
		ValueInput<float> max_damage_value = AddValueInput<float>("Max damage value [0..1]");
		AddValueOutput("Soul Item", () => out_soul_item_value);
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
				Item bodyFromInventory = worldGameObject.GetBodyFromInventory();
				if (bodyFromInventory != null)
				{
					Item itemOfType = bodyFromInventory.GetItemOfType(ItemDefinition.ItemType.SoulBodyPart);
					if (itemOfType != null)
					{
						RemoveBodyPartFromBody(bodyFromInventory, itemOfType);
						if ((double)par_dec_durability.value > 0.1)
						{
							itemOfType.durability = 1f - par_dec_durability.value / 100f;
						}
						float num = UnityEngine.Random.Range(min_damage_value.value, max_damage_value.value);
						itemOfType.durability -= num;
						CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(worldGameObject.obj_id + ":" + itemOfType.id);
						itemOfType.SetItemID(dataOrNull.output[0].id);
						out_soul_item_value = itemOfType;
					}
					else
					{
						itemOfType = new Item("sin_shard", 1);
						Item item = new Item("sin_shard_body_part", 1);
						RemoveBodyPartFromBody(bodyFromInventory, item);
						out_soul_item_value = itemOfType;
					}
					flow_out.Call(f);
				}
			}
		});
	}

	private static void RemoveBodyPartFromBody(Item body, Item item)
	{
		foreach (Item item2 in body.inventory)
		{
			if (item2.id == item.id)
			{
				body.RemoveItem(item, 1);
				break;
			}
			foreach (Item item3 in item2.inventory)
			{
				if (item3.id == item.id)
				{
					item2.RemoveItem(item, 1);
					return;
				}
			}
		}
	}
}
