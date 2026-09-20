using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class PrayLogics
{
	[Serializable]
	public struct PrayResult
	{
		public int people;

		public int faith;

		public int faith_bonus;

		public float money;

		public float money_bonus;

		public bool success;

		public string pray_craft_id;

		public string pray_event_id;

		public int success_percent;
	}

	private static List<GameObject> _places_hi_priority = new List<GameObject>();

	private static List<GameObject> _places_low_priority = new List<GameObject>();

	private static List<GameObject> _places_very_low_priority = new List<GameObject>();

	private static GameObject _default_place = null;

	public static PrayResult last_pray_result;

	private static List<Item> _sermon_drops = null;

	public static void InitPrayPlacesList()
	{
		_places_hi_priority = WorldMap.GetGDPointsByGDTag("church_plc");
		_places_low_priority = WorldMap.GetGDPointsByGDTag("church_plc_2");
		_places_very_low_priority = WorldMap.GetGDPointsByGDTag("church_plc_3");
		_default_place = ((_places_very_low_priority.Count > 0) ? _places_very_low_priority.RandomElement() : _places_low_priority.RandomElement());
	}

	public static GameObject GetPrayPlace()
	{
		if (_places_hi_priority.Count > 0)
		{
			return _places_hi_priority.RandomElement(remove_element: true);
		}
		if (_places_low_priority.Count > 0)
		{
			return _places_low_priority.RandomElement(remove_element: true);
		}
		if (_places_very_low_priority.Count > 0)
		{
			return _places_very_low_priority.RandomElement(remove_element: true);
		}
		return _default_place;
	}

	public static void SpreadFaithIncome(List<WorldGameObject> prayers, int faith)
	{
		if (prayers == null)
		{
			Debug.LogError("SpreadFaithIncome error: prayers list is null");
			return;
		}
		for (int i = 0; i < prayers.Count; i++)
		{
			if (prayers[i] == null)
			{
				prayers.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < faith; j++)
		{
			WorldGameObject worldGameObject = prayers.RandomElement();
			if (!(worldGameObject == null))
			{
				worldGameObject.AddToParams("_faith", 1f);
			}
		}
	}

	public static void SpreadMoneyIncome(List<WorldGameObject> prayers, float money, float success_percent = 1f)
	{
		if (prayers == null)
		{
			Debug.LogError("SpreadMoneyIncome: prayers list is null");
			return;
		}
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (WorldGameObject prayer in prayers)
		{
			if ((float)UnityEngine.Random.Range(0, 1) < success_percent)
			{
				list.Add(prayer);
			}
		}
		float num = Mathf.Floor(money * 100f / (float)list.Count) / 100f;
		float num2 = 0f;
		foreach (WorldGameObject item in list)
		{
			num2 += num;
			item.AddToParams("_money", num);
		}
		while (num2 < money)
		{
			num2 += 0.01f;
			list.RandomElement().AddToParams("_money", 0.01f);
		}
	}

	public static PrayResult CalculatePray(string pray_event_id)
	{
		PrayResult result = default(PrayResult);
		PrayEventDefinition data = GameBalance.me.GetData<PrayEventDefinition>(pray_event_id);
		result.people = Mathf.Max(0, Mathf.RoundToInt(data.people.EvaluateFloat()));
		result.faith = Mathf.Max(0, Mathf.RoundToInt(data.faith.EvaluateFloat()));
		result.money = Mathf.Max(0f, data.money.EvaluateFloat());
		result.pray_event_id = pray_event_id;
		CraftDefinition pray_craft = GUIElements.me.pray_craft.pray_craft;
		result.pray_craft_id = pray_craft.id;
		result.success_percent = (pray_craft.needs_quality.EqualsTo(0f) ? 100 : Mathf.RoundToInt(GUIElements.me.pray_craft.GetCurrentZoneQuality() / pray_craft.needs_quality * 100f));
		if (result.success_percent > 100)
		{
			result.success_percent = 100;
		}
		else if (result.success_percent < 0)
		{
			result.success_percent = 0;
		}
		Debug.Log("res.success_percent = " + result.success_percent);
		result.success = result.success_percent == 100 || UnityEngine.Random.Range(0, 100) < result.success_percent;
		_sermon_drops = new List<Item>();
		if (result.success)
		{
			_sermon_drops.AddRange(pray_craft.output);
			Item itemFromList = Item.GetItemFromList(_sermon_drops, "faith");
			if (itemFromList != null)
			{
				_sermon_drops.Remove(itemFromList);
				result.faith_bonus += itemFromList.value;
			}
			Item itemFromList2 = Item.GetItemFromList(_sermon_drops, "money");
			if (itemFromList2 != null)
			{
				_sermon_drops.Remove(itemFromList2);
				result.money_bonus += (float)itemFromList2.value / 100f;
			}
			if (!pray_craft.k_faith.EqualsTo(0f))
			{
				result.faith_bonus += Mathf.RoundToInt((float)result.faith * pray_craft.k_faith);
			}
			if (!pray_craft.k_money.EqualsTo(0f))
			{
				result.money_bonus += Mathf.Round(result.money * pray_craft.k_money * 100f) / 100f;
			}
		}
		else
		{
			result.faith_bonus = 0;
			result.money_bonus = 0f;
		}
		Debug.Log("CalculatePray '" + pray_event_id + "' ppl = " + result.people + ", faith = " + result.faith + ", money = " + result.money + ", success = " + result.success);
		last_pray_result = result;
		return result;
	}

	public static void DropPrayItems()
	{
		GDPoint gd_point = WorldMap.GetGDPointByName("faith_drop_point");
		float num = 0f;
		foreach (Item sermon_drop in _sermon_drops)
		{
			for (int i = 0; i < sermon_drop.value; i++)
			{
				Item drop = new Item(sermon_drop)
				{
					value = 1
				};
				GJTimer.AddTimer(num, delegate
				{
					DropResGameObject dropResGameObject = DropResGameObject.DropAndFly(gd_point.pos, drop, MainGame.me.world_root, gd_point.pos);
					Transform t = dropResGameObject.go_small_drop.transform;
					t.localPosition = new Vector3(0f, 5f);
					DOTween.To(() => t.localPosition.y, delegate(float y)
					{
						t.localPosition = new Vector3(t.localPosition.x, y);
					}, 0f, 1f).SetEase(Ease.OutBounce);
					Color color = dropResGameObject.sprite_res_item.color;
					dropResGameObject.sprite_res_item.color = new Color(dropResGameObject.sprite_res_item.color.r, dropResGameObject.sprite_res_item.color.g, dropResGameObject.sprite_res_item.color.b, 0f);
					dropResGameObject.sprite_res_item.DOColor(color, 0.3f);
				});
				num += 0.2f;
			}
		}
	}
}
