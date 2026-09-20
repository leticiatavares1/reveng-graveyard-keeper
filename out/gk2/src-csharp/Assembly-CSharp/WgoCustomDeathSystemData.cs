using System;
using System.Collections.Generic;

[Serializable]
public class WgoCustomDeathSystemData
{
	[Serializable]
	public class WgoCustomDeathData
	{
		public SGuid wgoUniqueId;

		public float remainingTime;
	}

	public List<WgoCustomDeathData> wgoCustomDeathData = new List<WgoCustomDeathData>();

	public void AddCustomDeath(WgoData wgoData)
	{
		wgoCustomDeathData.Add(new WgoCustomDeathData
		{
			wgoUniqueId = wgoData.UniqueId,
			remainingTime = wgoData.Definition.customDeathTime
		});
	}
}
