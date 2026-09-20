using System.Collections.Generic;
using UnityEngine;

public static class ResModificator
{
	public static List<Item> ProcessItemsListBeforeDrop(List<Item> items, WorldGameObject wgo, WorldGameObject character, Item exclude_item = null)
	{
		List<Item> list = new List<Item>();
		List<Item> list2 = new List<Item>();
		List<float> list3 = new List<float>();
		float num = 0f;
		foreach (Item item in items)
		{
			if (item.chance_group == -1)
			{
				if (item.self_chance.EvaluateChance(wgo, character))
				{
					list.Add(new Item(item));
				}
				continue;
			}
			float num2 = item.common_chance.EvaluateFloat(wgo, character);
			if (num2 > 0f)
			{
				list2.Add(new Item(item));
				list3.Add(num2);
				num += num2;
			}
		}
		float num3 = Random.Range(0f, num);
		float num4 = 0f;
		for (int i = 0; i < list3.Count; i++)
		{
			num4 += list3[i];
			if (num4 >= num3)
			{
				list.Add(list2[i]);
				break;
			}
		}
		if (exclude_item != null)
		{
			string text = exclude_item.id.Split(':')[0];
			for (int j = 0; j < list.Count; j++)
			{
				string text2 = list[j].id.Split(':')[0];
				if (text == text2)
				{
					list.RemoveAt(j);
				}
			}
		}
		foreach (Item item2 in list)
		{
			if (item2.min_value != null && !item2.min_value.HasNoExpresion())
			{
				int num5 = Mathf.RoundToInt(item2.min_value.EvaluateFloat(wgo, character));
				int num6 = Mathf.RoundToInt(item2.max_value.EvaluateFloat(wgo, character));
				if (num5 < 0)
				{
					num5 = 0;
				}
				if (num6 >= num5)
				{
					item2.value = Random.Range(num5, num6 + 1);
				}
				else
				{
					item2.value = num5;
				}
			}
		}
		return list;
	}
}
