using System.Collections.Generic;

public class WgoDelayedEventsSystem : ICustomUpdatable
{
	private static WgoDelayedEventSystemData WgoDelayedEventSystemData => MainGame.Instance.GameSave.wgoDelayedEventSystemData;

	public void CustomUpdate(float deltaTime)
	{
		if (MainGame.Instance?.GameSave?.wgoDelayedEventSystemData?.wgoUniqueIds != null)
		{
			List<SGuid> wgoUniqueIds = WgoDelayedEventSystemData.wgoUniqueIds;
			for (int num = wgoUniqueIds.Count - 1; num >= 0; num--)
			{
				MainGame.WorldData.GetWgoData(wgoUniqueIds[num])?.UpdateDelayedEvents(deltaTime);
			}
		}
	}
}
