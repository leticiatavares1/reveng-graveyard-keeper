using System;
using System.Collections.Generic;

[Serializable]
public class CraftSystemData
{
	[NonSerialized]
	public List<CraftComponent> activeCrafts = new List<CraftComponent>();

	public List<ZombieCraftActivity> zombieCraftActivities = new List<ZombieCraftActivity>();

	public List<ZombieHPActivity> zombieHPActivities = new List<ZombieHPActivity>();

	public void RestoreActiveCrafts(WorldData worldData)
	{
		activeCrafts = new List<CraftComponent>();
		List<GameSceneData> list = worldData?.gameSceneDataList;
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			List<WgoData> list2 = list[i]?.wgoDataList;
			if (list2 == null)
			{
				continue;
			}
			for (int j = 0; j < list2.Count; j++)
			{
				CraftComponent craftComponent = list2[j]?.CraftComponent;
				if (craftComponent != null && craftComponent.ShouldRegisterInCraftSystem())
				{
					activeCrafts.Add(craftComponent);
				}
			}
		}
	}
}
