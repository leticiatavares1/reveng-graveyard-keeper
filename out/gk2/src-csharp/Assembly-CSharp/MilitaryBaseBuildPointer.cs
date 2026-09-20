public class MilitaryBaseBuildPointer : WgoBuildPointer
{
	public override bool TryDoBuildAction()
	{
		if (shownAsActive)
		{
			takeResourcesAction?.Invoke();
			Wgo wgo = gameScene.AddWgoData(buildData.WgoId, target.Data.Position);
			if (target.CanBeRotated() && target.MainWgoPart.WgoPartData.rotationIndex != -1)
			{
				wgo.MainWgoPart.ApplyWgoPartState(target.MainWgoPart.WgoPartData.variationId, target.MainWgoPart.WgoPartData.rotationIndex);
			}
			if (buildData.Definition != null)
			{
				foreach (LazyExpression item in buildData.Definition.expressionAfterBuilding)
				{
					item.EvaluateBool(wgo.Data);
				}
			}
			MainGame.Instance.GameSave.militaryBaseData.AddBaseBuilding(wgo.Data);
			return true;
		}
		return false;
	}
}
