using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
	[SerializeField]
	private List<ScheduledUpdate> scheduledUpdates = new List<ScheduledUpdate>();

	private float timeMultiplier = 1f;

	public bool IsActive { get; set; }

	public float TimeMultiplier => timeMultiplier;

	public void AddScheduledUpdate(ScheduledUpdate scheduledUpdate)
	{
		if (scheduledUpdates.Contains(scheduledUpdate))
		{
			Debug.LogWarning($"Already added scheduled update: {scheduledUpdate}");
		}
		else
		{
			scheduledUpdates.Add(scheduledUpdate);
		}
	}

	private void Update()
	{
		if (!IsActive)
		{
			return;
		}
		float num = timeMultiplier;
		foreach (ScheduledUpdate scheduledUpdate in scheduledUpdates)
		{
			if (scheduledUpdate.updateInterval > 0f)
			{
				scheduledUpdate.accumulatedTime += Time.deltaTime * num;
				while (scheduledUpdate.accumulatedTime >= scheduledUpdate.updateInterval)
				{
					scheduledUpdate.CallUpdate(scheduledUpdate.updateInterval, applyTimeMultiplier: false);
					scheduledUpdate.accumulatedTime -= scheduledUpdate.updateInterval;
				}
			}
			else
			{
				scheduledUpdate.CallUpdate(Time.deltaTime);
			}
		}
	}

	public void SetTimeSpeedMultiplier(float value)
	{
		timeMultiplier = value;
	}
}
