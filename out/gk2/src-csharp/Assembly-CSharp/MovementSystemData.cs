using System;
using System.Collections.Generic;

[Serializable]
public class MovementSystemData
{
	[NonSerialized]
	public List<MovementComponent> movingObjects = new List<MovementComponent>();

	public void RestoreMovingObjects(WorldData worldData)
	{
		movingObjects = new List<MovementComponent>();
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
				MovementComponent movementComponent = list2[j]?.MovementComponent;
				if (movementComponent != null && movementComponent.ShouldRegisterInMovementSystem())
				{
					movingObjects.Add(movementComponent);
				}
			}
		}
	}
}
