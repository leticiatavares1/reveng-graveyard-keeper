using System;
using UnityEngine;

[Serializable]
public class SmartDrawerAtom
{
	public enum SmartDrawingCondition
	{
		None = 0,
		OnCrafting = 1,
		ItemsEqual = 2,
		ItemsMoreEqual = 3,
		GameResEqual = 4,
		GameResMore = 5,
		DayTime_Day = 6,
		RainLess = 7,
		RainMore = 8,
		WindLess = 9,
		WindMore = 10,
		FogLess = 11,
		FogMore = 12,
		DayTime_Night = 13,
		ContainItem = 14,
		GameResBetween = 15,
		GameResLess = 16,
		ConcreteItemsBetween = 17,
		ConcreteItemsMore = 18,
		NoLinkedWorker = 19,
		PorterStationStateIsNot = 20,
		HasLinkedWorker = 21,
		TotalItemsCount = 22,
		PlayerHasItem = 23,
		PlayerGameResEqual = 24,
		GDPointEnabled = 26,
		GDPointDisabled = 27,
		HasItemInInventoryByIndex = 28
	}

	public GameObject obj;

	public SmartDrawingCondition condition;

	public float update_period;

	private float _cur_update_time;

	public int int_value;

	public int int_value_2;

	public string string_value = string.Empty;

	public float float_value;

	public float float_value_2;

	public bool CheckNeedRecalc(float delta_time)
	{
		_cur_update_time += delta_time;
		if (_cur_update_time < update_period)
		{
			return false;
		}
		_cur_update_time = 0f;
		return true;
	}
}
