using System;
using System.Collections.Generic;
using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Do Tavern Sellings", 0)]
[Category("Game Actions")]
public class Flow_DoTavernSellings : MyFlowNode
{
	private float _reputation_earned;

	private float _money_earned;

	protected override void RegisterPorts()
	{
		ValueInput<float> in_period = AddValueInput<float>("Period (days)");
		ValueInput<WorldGameObject> in_tavern_cashbox = AddValueInput<WorldGameObject>("tavern_cashbox");
		AddValueOutput("reputation earned", () => _reputation_earned);
		AddValueOutput("money earned", () => _money_earned);
		FlowOutput flow_out = AddFlowOutput("out");
		AddFlowInput("In", delegate(Flow f)
		{
			_reputation_earned = 0f;
			_money_earned = 0f;
			WorldZone zoneByID = WorldZone.GetZoneByID("player_tavern_cellar");
			if (zoneByID == null)
			{
				flow_out.Call(f);
			}
			else
			{
				List<Inventory> multiInventory = zoneByID.GetMultiInventory();
				int[] array = new int[MainGame.me.save.players_tavern_engine.ITEMS_SELLING_IN_TAVERN.Length];
				float totalQuality = WorldZone.GetZoneByID("players_tavern").GetTotalQuality();
				int num = Mathf.RoundToInt(totalQuality * in_period.value);
				int num2 = num;
				float num3 = 0f;
				foreach (Inventory item2 in multiInventory)
				{
					bool flag = false;
					for (int i = 0; i < item2.data.inventory.Count; i++)
					{
						Item item = item2.data.inventory[i];
						if (num == 0)
						{
							flag = true;
							break;
						}
						if (item != null && !item.IsEmpty() && item.value != 0 && MainGame.me.save.players_tavern_engine.ITEMS_SELLING_IN_TAVERN.Contains(item.id))
						{
							int num4 = Array.IndexOf(MainGame.me.save.players_tavern_engine.ITEMS_SELLING_IN_TAVERN, item.id);
							if (num >= item.value)
							{
								num -= item.value;
								array[num4] += item.value;
								num3 += (float)item.value * item.definition.base_price * 1f;
								item2.data.inventory.RemoveAt(i);
								i--;
							}
							else
							{
								item.value -= num;
								array[num4] += num;
								num3 += (float)num * item.definition.base_price * 1f;
								num = 0;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (Mathf.Abs(num3) > 0.01f)
				{
					float average_item_price = num3 / (float)(num2 - num);
					num3 += PlayersTavernEngine.CalculateAlcoholSellingBonus(num3);
					_reputation_earned = num3 * 0.01f * PlayersTavernEngine.CalculateCorrecterCoeff(totalQuality, average_item_price);
					_money_earned = num3;
				}
				if (in_tavern_cashbox.value != null)
				{
					for (int j = 0; j < array.Length; j++)
					{
						string param_name = MainGame.me.save.players_tavern_engine.ITEMS_SELLING_IN_TAVERN[j].Replace(":", "_");
						in_tavern_cashbox.value.AddToParams(param_name, array[j]);
					}
				}
				Debug.Log($"Tavern Sellings done: total money: {num3}; sold items ={num2 - num}; " + $"reputation earned: {_reputation_earned}");
				flow_out.Call(f);
			}
		});
	}
}
