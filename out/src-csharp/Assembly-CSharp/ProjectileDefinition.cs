using System;
using UnityEngine;

[Serializable]
public class ProjectileDefinition : BalanceBaseObject
{
	public string prefab_name;

	public float speed;

	public float max_dist_lower_limit;

	public float max_dist_upper_limit;

	public float max_time;

	public float damage;

	public int pierce;

	public bool can_damage_mobs;

	public EventDefinition on_start;

	public EventDefinition on_hit_combat;

	public EventDefinition on_hit_non_combat;

	public EventDefinition on_out_of_screen;

	public EventDefinition on_max_dist_reached;

	public float GetMaxDist()
	{
		float num = max_dist_upper_limit - max_dist_lower_limit;
		if (num < 0.01f)
		{
			return max_dist_upper_limit;
		}
		float num2 = UnityEngine.Random.value * num;
		return max_dist_lower_limit + num2;
	}
}
