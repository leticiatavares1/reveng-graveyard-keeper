using LazyBearTechnology;

public class FightingBuildPointer : WgoBuildPointer
{
	public override bool TryDoBuildAction()
	{
		if (shownAsActive)
		{
			takeResourcesAction?.Invoke();
			WgoData wgoData = new WgoData(buildData.WgoId, target.Data.Position, gameScene.Id);
			if (target.CanBeRotated() && target.MainWgoPart.WgoPartData.rotationIndex != -1)
			{
				wgoData.MainWgoPartData.variationId = target.MainWgoPart.WgoPartData.variationId;
				wgoData.MainWgoPartData.rotationIndex = target.MainWgoPart.WgoPartData.rotationIndex;
			}
			Wgo wgo = gameScene.AddWgoData(wgoData);
			if (buildData.Definition != null)
			{
				foreach (LazyExpression item in buildData.Definition.expressionAfterBuilding)
				{
					item.EvaluateBool(wgo.Data);
				}
			}
			MainGame.Instance.GameSave.militaryBaseData.AddFightBuilding(wgo.Data);
			if (LazySingleton<FightingGameController>.Instance.CurrentFightState != 0)
			{
				wgo.IsActiveCombatant = GameBalance.Me.fighterWgoIdsCache.Contains(wgo.Data.id);
				if (wgo.IsActiveCombatant)
				{
					LazySingleton<FightingGameController>.Instance.RegisterCombatantTarget(wgo, updateInfoIfExists: false);
				}
			}
			return true;
		}
		return false;
	}
}
