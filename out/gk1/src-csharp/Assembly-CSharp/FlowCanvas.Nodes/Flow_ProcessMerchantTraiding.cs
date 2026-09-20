using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Process Merchant Traiding", 0)]
public class Flow_ProcessMerchantTraiding : MyFlowNode
{
	protected override void RegisterPorts()
	{
		float total_money = 0f;
		int sold_items_count = 0;
		List<string> sold_items = new List<string>();
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("Pallet");
		AddValueOutput("total money", () => total_money);
		AddValueOutput("sold items count", () => sold_items_count);
		AddValueOutput("sold items", () => sold_items);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			total_money = 0f;
			sold_items_count = 0;
			sold_items = new List<string>();
			if (in_wgo.value == null)
			{
				Debug.LogError("Can not Process Merchant Traiding: WGO is null!");
				flow_out.Call(f);
			}
			else if (in_wgo.value.data == null)
			{
				Debug.LogError("Can not Process Merchant Traiding: WGO.data is null!");
				flow_out.Call(f);
			}
			else
			{
				List<Item> list = new List<Item>();
				foreach (Item item in in_wgo.value.data.inventory)
				{
					if (item != null && item.definition != null && (!(item.id != "box_vegetables_silver") || !(item.id != "box_vegetables_gold") || !(item.id != "box_goods")))
					{
						total_money += item.definition.base_price * (float)item.value;
						sold_items_count += item.value;
						if (item.value <= 1)
						{
							sold_items.Add(item.id);
						}
						else
						{
							for (int i = 0; i < item.value; i++)
							{
								sold_items.Add(item.id);
							}
						}
						list.Add(item);
					}
				}
				if (sold_items.Count != sold_items_count)
				{
					Debug.LogError("Something wrong with merchant traiding! Call Bulat!");
				}
				foreach (Item item2 in list)
				{
					in_wgo.value.data.RemoveItem(item2.id, item2.value);
				}
				flow_out.Call(f);
			}
		});
	}
}
