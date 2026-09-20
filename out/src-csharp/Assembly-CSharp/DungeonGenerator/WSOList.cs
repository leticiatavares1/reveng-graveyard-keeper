using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator;

[CreateAssetMenu(fileName = "WSOList", menuName = "WSOList", order = 1)]
public class WSOList : ScriptableObject
{
	public List<WorldSimpleObject> wso_list = new List<WorldSimpleObject>();

	public List<float> weights = new List<float>();

	public bool IsCorrectWSOList()
	{
		if (wso_list == null)
		{
			return false;
		}
		if (weights == null)
		{
			return false;
		}
		return wso_list.Count == weights.Count;
	}

	public void AddWSO(WorldSimpleObject t_wso, float t_weight)
	{
		if (!(t_wso == null) && !(t_weight < 0f))
		{
			wso_list.Add(t_wso);
			weights.Add(t_weight);
		}
	}

	public void RemoveWSO(WorldSimpleObject t_wso)
	{
		if (!(t_wso == null) && IsCorrectWSOList() && wso_list.Contains(t_wso))
		{
			int index = wso_list.IndexOf(t_wso);
			wso_list.RemoveAt(index);
			weights.RemoveAt(index);
		}
	}

	public WorldSimpleObject GetRandomWSO()
	{
		if (!IsCorrectWSOList())
		{
			return null;
		}
		if (wso_list.Count == 0)
		{
			return null;
		}
		if (wso_list.Count == 1)
		{
			return wso_list[0];
		}
		float num = Dungeon.RandomRange(0f, SumOfWeights());
		float num2 = 0f;
		int i;
		for (i = 0; i < weights.Count; i++)
		{
			num2 += weights[i];
			if (num2 > num)
			{
				break;
			}
		}
		if (i >= weights.Count)
		{
			i = weights.Count - 1;
		}
		return wso_list[i];
	}

	public float SumOfWeights()
	{
		float num = 0f;
		foreach (float weight in weights)
		{
			num += weight;
		}
		return num;
	}
}
