public class PerkSystem : ICustomUpdatable
{
	private PerkSystemData Data => MainGame.Instance.GameSave.perkSystemData;

	public void CustomUpdate(float deltaTime)
	{
		for (int num = Data.activePerks.Count - 1; num >= 0; num--)
		{
			PerkData perkData = Data.activePerks[num];
			perkData.tickTimer += deltaTime;
			if (perkData.Definition.duration > 0f)
			{
				perkData.currentDuration -= deltaTime;
			}
			if (perkData.Definition.tickRate != 0f && perkData.tickTimer >= perkData.Definition.tickRate)
			{
				perkData.tickTimer = 0f;
				if (!perkData.Definition.addGameResPerTick.IsEmpty())
				{
					MainGame.PlayerData.AddRes(perkData.Definition.addGameResPerTick);
				}
				foreach (LazyExpression onPerTickExpression in perkData.Definition.onPerTickExpressions)
				{
					onPerTickExpression.Evaluate();
				}
			}
			if (perkData.Definition.duration > 0f && perkData.currentDuration <= 0f)
			{
				Data.RemovePerk(perkData);
			}
		}
		Data.NotifyUpdated();
	}
}
