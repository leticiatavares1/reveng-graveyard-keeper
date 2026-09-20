public class WgoCustomDeathSystem : ICustomUpdatable
{
	public void CustomUpdate(float deltaTime)
	{
		for (int num = MainGame.Instance.GameSave.wgoCustomDeathSystemData.wgoCustomDeathData.Count - 1; num >= 0; num--)
		{
			WgoCustomDeathSystemData.WgoCustomDeathData wgoCustomDeathData = MainGame.Instance.GameSave.wgoCustomDeathSystemData.wgoCustomDeathData[num];
			wgoCustomDeathData.remainingTime -= deltaTime;
			if (wgoCustomDeathData.remainingTime <= 0f)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(wgoCustomDeathData.wgoUniqueId);
				if (wgoData == null)
				{
					MainGame.Instance.GameSave.wgoCustomDeathSystemData.wgoCustomDeathData.RemoveAt(num);
				}
				else
				{
					wgoData.TriggerCustomDeathMoment();
					MainGame.Instance.GameSave.wgoCustomDeathSystemData.wgoCustomDeathData.RemoveAt(num);
				}
			}
		}
	}
}
