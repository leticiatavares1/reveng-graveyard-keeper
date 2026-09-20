using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class FishDefinition : BalanceBaseObject
{
	[Serializable]
	public class WeightData
	{
		public string data_name;

		public float weight;

		public static List<WeightData> ParseWeightDatas(string data)
		{
			List<WeightData> list = new List<WeightData>();
			string[] array = data.Replace(" ", "").Replace("\n", "").Replace("&#xd", "")
				.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0)
			{
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Trim().Split('=');
					if (array3.Length != 2)
					{
						Debug.LogError("Wrong weights data!");
						continue;
					}
					float result = 0f;
					if (!float.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						Debug.LogError("Wrong weights data: can not parse \"" + text + "\"");
						continue;
					}
					list.Add(new WeightData
					{
						data_name = array3[0],
						weight = result
					});
				}
			}
			return list;
		}
	}

	[Serializable]
	public class BaitData
	{
		public string bait_name;

		public float weight_k;

		public float wait_time;
	}

	public string item_id;

	public string fish_preset;

	public List<WeightData> base_weight;

	public float[] time_of_day_mods;

	public float[] distance_mods;

	public List<float> rod_mods;

	public BaitData no_bait_mod = new BaitData();

	public List<BaitData> baits_mod = new List<BaitData>();

	public float GetTotalWeight(string reservoir_name, bool is_night, int distance, int rod_lvl, string bait)
	{
		float num = 0f;
		if (string.IsNullOrEmpty(reservoir_name))
		{
			Debug.LogError("Reservoir is null!");
			return 0f;
		}
		WeightData weightData = null;
		foreach (WeightData item in base_weight)
		{
			if (item.data_name == reservoir_name)
			{
				weightData = item;
			}
		}
		if (weightData == null)
		{
			Debug.LogError("Not found reservoir \"" + reservoir_name + "\" in fish \"" + id + "\"");
			return 0f;
		}
		num = weightData.weight;
		num *= time_of_day_mods[(!is_night) ? 1u : 0u];
		if (distance < 1 || distance > 3)
		{
			Debug.LogError("Fishing distance is wrong: \"" + distance + "\"");
			return 0f;
		}
		num *= distance_mods[distance - 1];
		if (rod_lvl < 0 || rod_lvl >= rod_mods.Count)
		{
			Debug.LogError("Fishing rod is wrong: \"" + rod_lvl + "\"");
			return 0f;
		}
		num *= rod_mods[rod_lvl];
		if (string.IsNullOrEmpty(bait))
		{
			return num * no_bait_mod.weight_k;
		}
		BaitData baitData = null;
		foreach (BaitData item2 in baits_mod)
		{
			if (item2.bait_name == bait)
			{
				baitData = item2;
			}
		}
		if (baitData == null)
		{
			baitData = no_bait_mod;
		}
		return num * baitData.weight_k;
	}
}
