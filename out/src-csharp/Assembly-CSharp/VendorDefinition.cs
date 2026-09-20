using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VendorDefinition : BalanceBaseObject
{
	[Serializable]
	public struct CountModificator
	{
		public string item_name;

		public int tier;

		public int base_count;
	}

	[Serializable]
	public struct ItemModificator
	{
		public string item_name;

		public int tier;
	}

	[SerializeField]
	private List<string> product_types = new List<string>();

	public int start_tire;

	public float start_money;

	public float daily_money_income;

	public float[] levelup_costs;

	public List<CountModificator> count_modificators = new List<CountModificator>();

	public List<ItemModificator> not_buying = new List<ItemModificator>();

	public List<ItemModificator> not_selling = new List<ItemModificator>();

	public List<ExpressionRes> additional_types = new List<ExpressionRes>();

	public List<string> GetProductTypes()
	{
		List<string> list = new List<string>();
		list.AddRange(product_types);
		for (int i = 0; i < additional_types.Count; i++)
		{
			if (additional_types[i].expression.EvaluateBoolean(MainGame.me.player, MainGame.me.player))
			{
				list.Add(additional_types[i].name);
			}
		}
		return list;
	}

	private void AddCountModificator(CountModificator count_modificator)
	{
		if (count_modificators == null)
		{
			count_modificators = new List<CountModificator>();
		}
		if (count_modificators.Count == 0)
		{
			count_modificators.Add(count_modificator);
			return;
		}
		for (int i = 0; i < count_modificators.Count; i++)
		{
			if (count_modificators[i].tier < count_modificator.tier)
			{
				if (i == count_modificators.Count - 1)
				{
					count_modificators.Add(count_modificator);
					break;
				}
				continue;
			}
			if (count_modificators[i].tier == count_modificator.tier)
			{
				count_modificators.Insert(i, count_modificator);
				break;
			}
			if (count_modificators[i].tier > count_modificator.tier)
			{
				count_modificators.Insert((i > 0) ? (i - 1) : 0, count_modificator);
				break;
			}
		}
	}

	public void SortCountModificators()
	{
		if (count_modificators == null || count_modificators.Count <= 1)
		{
			return;
		}
		count_modificators.Sort(delegate(CountModificator left, CountModificator right)
		{
			if (left.tier < right.tier)
			{
				return -1;
			}
			return (left.tier > right.tier) ? 1 : 0;
		});
	}
}
