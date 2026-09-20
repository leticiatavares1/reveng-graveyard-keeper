using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class AnimationEventData
{
	public string id;

	[Range(0f, 100f)]
	public float time;

	[Range(0f, 100f)]
	public float deltaTime;

	[HideInInspector]
	public float planningTime;

	[HideInInspector]
	public float nextTimeToPlay;

	public List<AnimationTriggerData> triggers;

	public string PickTrigger()
	{
		if (triggers.Count == 0)
		{
			Debug.LogError("Triggers for event type " + id + " not defined.");
			return string.Empty;
		}
		int num = CalcWeightTotal();
		int num2 = UnityEngine.Random.Range(0, num + 1);
		int num3 = 0;
		foreach (AnimationTriggerData trigger in triggers)
		{
			if (num3 + trigger.weight >= num2)
			{
				return trigger.triggerName;
			}
			num3 += trigger.weight;
		}
		throw new Exception();
	}

	public int CalcWeightTotal()
	{
		int num = 0;
		foreach (AnimationTriggerData trigger in triggers)
		{
			num += trigger.weight;
		}
		return num;
	}

	public void PlanStartPlayingTime()
	{
		planningTime = Time.time;
		nextTimeToPlay = time + UnityEngine.Random.Range(0f - deltaTime, deltaTime);
	}
}
