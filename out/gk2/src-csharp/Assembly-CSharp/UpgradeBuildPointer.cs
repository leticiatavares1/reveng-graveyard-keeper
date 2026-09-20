public class UpgradeBuildPointer : WgoBuildPointer
{
	public override bool TryDoBuildAction()
	{
		if (shownAsActive)
		{
			takeResourcesAction?.Invoke();
			Wgo componentInParent = BuildController.Instance.CurrentFullCoverSoftHintArea.GetComponentInParent<Wgo>();
			if (buildData.Definition != null)
			{
				foreach (LazyExpression item in buildData.Definition.expressionAfterBuilding)
				{
					item.EvaluateBool(componentInParent.Data);
				}
			}
			return true;
		}
		return false;
	}
}
