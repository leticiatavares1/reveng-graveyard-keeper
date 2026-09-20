using System;
using System.Collections.Generic;

[Serializable]
public class ScheduledUpdate
{
	public float updateInterval;

	public List<ICustomUpdatable> customUpdatables = new List<ICustomUpdatable>();

	public float accumulatedTime;

	public ScheduledUpdate(ICustomUpdatable updatable, float updateInterval)
	{
		this.updateInterval = updateInterval;
		customUpdatables.Add(updatable);
	}

	public ScheduledUpdate(List<ICustomUpdatable> updatables, float updateInterval)
	{
		this.updateInterval = updateInterval;
		customUpdatables.AddRange(updatables);
	}

	public void CallUpdate(float deltaTime, bool applyTimeMultiplier = true)
	{
		if (applyTimeMultiplier)
		{
			deltaTime *= MainGame.UpdateManager.TimeMultiplier;
		}
		foreach (ICustomUpdatable customUpdatable in customUpdatables)
		{
			customUpdatable.CustomUpdate(deltaTime);
		}
	}
}
