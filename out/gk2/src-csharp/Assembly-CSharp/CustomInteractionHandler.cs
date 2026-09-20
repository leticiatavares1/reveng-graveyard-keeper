public class CustomInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Definition.customInteraction.IsInteractable(assignedWgo.Data))
		{
			Interact(assignedWgo.Data.Definition.customInteraction);
			return true;
		}
		Interact(assignedWgo.Data.Definition.customInteraction2);
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (HasInteraction(assignedWgo.Data.Definition.customInteraction) || HasInteraction(assignedWgo.Data.Definition.customInteraction2))
		{
			return true;
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		string hint = string.Empty;
		if (TryGetInteractionHint(assignedWgo.Data.Definition.customInteraction, out hint))
		{
			if (hint.StartsWith("[") && hint.EndsWith("]"))
			{
				return new InteractionInfos(GetInteractionInfoByUsingTool());
			}
			return new InteractionInfos(new InteractionInfo((!string.IsNullOrEmpty(hint)) ? LocalizeHintWithActionIcon(hint, assignedWgo.Data.Definition.customInteraction.gameKey) : string.Empty));
		}
		if (TryGetInteractionHint(assignedWgo.Data.Definition.customInteraction2, out hint))
		{
			if (hint.StartsWith("[") && hint.EndsWith("]"))
			{
				return new InteractionInfos(GetInteractionInfoByUsingTool());
			}
			return new InteractionInfos(new InteractionInfo((!string.IsNullOrEmpty(hint)) ? LocalizeHintWithActionIcon(hint, assignedWgo.Data.Definition.customInteraction2.gameKey) : string.Empty));
		}
		return new InteractionInfos(new InteractionInfo(hint));
	}

	private bool HasInteraction(CustomInteraction customInteraction)
	{
		if (customInteraction.IsInteractable(assignedWgo.Data))
		{
			return true;
		}
		return false;
	}

	private void Interact(CustomInteraction customInteraction)
	{
		customInteraction.TryGetInteractable(assignedWgo.Data, out var candidate);
		Item overheadCandidate = LazyExpressionEvaluationScope.OverheadCandidate;
		LazyExpressionEvaluationScope.OverheadCandidate = candidate;
		try
		{
			foreach (LazyExpression item in customInteraction.execution)
			{
				item.EvaluateBool(assignedWgo.Data);
			}
		}
		finally
		{
			LazyExpressionEvaluationScope.OverheadCandidate = overheadCandidate;
		}
		if (assignedWgo.Data.Definition.customInteraction != null && assignedWgo.Data.Definition.customInteraction2 != null)
		{
			assignedWgo.DrawWidgets();
		}
	}

	private bool TryGetInteractionHint(CustomInteraction customInteraction, out string hint)
	{
		hint = string.Empty;
		if (customInteraction.IsInteractable(assignedWgo.Data))
		{
			hint = customInteraction.hint;
			return true;
		}
		return false;
	}
}
