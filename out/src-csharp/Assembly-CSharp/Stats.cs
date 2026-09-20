using UnityEngine;

public static class Stats
{
	private static bool _inited;

	public static void Init()
	{
		if (!_inited)
		{
			_inited = true;
		}
	}

	public static void DesignEvent(string event_name)
	{
	}

	public static void DesignEvent(string event_name, float value)
	{
	}

	public static void PlayerAddMoney(float amount, string trader_name)
	{
		if (amount <= 0f)
		{
			Debug.LogWarning("PlayerAddMoney: Stats error: can't send a zero or negative number, trader_name = " + trader_name + ", amount = " + amount);
		}
	}

	public static void PlayerDecMoney(float amount, string trader_name)
	{
		if (amount <= 0f)
		{
			Debug.LogWarning("PlayerDecMoney: Stats error: can't send a zero or negative number, trader_name = " + trader_name + ", amount = " + amount);
		}
	}

	public static void ResourceEvent(bool is_source, string resource_name, float amount, string item_type)
	{
		if (amount <= 0f)
		{
			Debug.LogWarning("ResourceEvent: Stats error: can't send a zero or negative number, resource_name = " + resource_name + ", amount = " + amount);
		}
	}
}
