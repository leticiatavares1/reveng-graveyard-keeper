using System.Collections.Generic;
using UnityEngine;

public class GardenCustomDrawer : MonoBehaviour
{
	public List<GameObject> garden_stages;

	public List<float> stage_costs;

	public const string GARDEN_GROWING_PARAM_NAME = "growing";

	private int _prev_stage = -1;

	public void Redraw(WorldGameObject wgo)
	{
		float num = wgo.GetParam("growing");
		if (num < 0f)
		{
			num = 0f;
			wgo.SetParam("growing", 0f);
		}
		int currentGrowStage = GetCurrentGrowStage(num);
		if (currentGrowStage != _prev_stage)
		{
			_prev_stage = currentGrowStage;
			for (int i = 0; i < garden_stages.Count; i++)
			{
				garden_stages[i].SetActive(currentGrowStage == i);
			}
		}
	}

	private int GetCurrentGrowStage(float progress)
	{
		int num = Mathf.FloorToInt(progress);
		for (int num2 = garden_stages.Count - 1; num2 >= 0; num2--)
		{
			if (num >= (int)stage_costs[num2])
			{
				return num2;
			}
		}
		return 0;
	}

	public bool IsCorrectDrawer(out string err)
	{
		err = "";
		if (garden_stages == null || garden_stages.Count == 0)
		{
			err += "Wrong garden stages count!\n";
			return false;
		}
		if (stage_costs == null || stage_costs.Count == 0)
		{
			err += "Wrong stages costs count!\n";
			return false;
		}
		if (garden_stages.Count != stage_costs.Count)
		{
			err += "garden_stages count != stage_costs count!\n";
			return false;
		}
		int num = 1;
		foreach (GameObject garden_stage in garden_stages)
		{
			if (garden_stage == null)
			{
				err = err + "Garden Stage #" + num + " is NULL!\n";
			}
			num++;
		}
		if (stage_costs.Count > 1)
		{
			for (int i = 0; i < stage_costs.Count - 1; i++)
			{
				if (stage_costs[i] >= stage_costs[i + 1])
				{
					err = err + "Wrong stage_cost " + (i + 1) + ": " + stage_costs[i] + " >= " + stage_costs[i + 1] + "!\n";
				}
			}
		}
		return string.IsNullOrEmpty(err);
	}
}
