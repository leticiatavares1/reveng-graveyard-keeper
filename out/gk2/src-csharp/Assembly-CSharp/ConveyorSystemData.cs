using System;
using System.Collections.Generic;

[Serializable]
public class ConveyorSystemData
{
	public bool isInitialized;

	public bool isPaused;

	[NonSerialized]
	public List<ConveyorComponent> conveyorComponents = new List<ConveyorComponent>();

	public List<ZombieCraftActivity> zombieCraftActivities = new List<ZombieCraftActivity>();

	public float timer;

	[NonSerialized]
	public List<ConveyorComponent> graphStartElements = new List<ConveyorComponent>();

	[NonSerialized]
	public List<ConveyorComponent> graphEndElements = new List<ConveyorComponent>();

	[NonSerialized]
	public List<ConveyorWorkbenchComponent> workbenchElements = new List<ConveyorWorkbenchComponent>();

	[NonSerialized]
	public List<ConveyorSplitterComponent> splitterElements = new List<ConveyorSplitterComponent>();

	public void PrepareForGame(WorldData worldData)
	{
		RestoreConveyorComponents(worldData);
		graphStartElements = new List<ConveyorComponent>();
		graphEndElements = new List<ConveyorComponent>();
		workbenchElements = new List<ConveyorWorkbenchComponent>();
		splitterElements = new List<ConveyorSplitterComponent>();
	}

	public void RestoreConveyorComponents(WorldData worldData)
	{
		conveyorComponents = new List<ConveyorComponent>();
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
				if (list2[j] is ConveyorWgoData { ConveyorComponent: { } conveyorComponent })
				{
					conveyorComponents.Add(conveyorComponent);
				}
			}
		}
	}
}
