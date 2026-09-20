using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class CustomInteraction
{
	public LazyExpression condition;

	public List<LazyExpression> execution;

	public GameKey gameKey = GameKey.Interaction;

	public string hint;

	public bool IsInteractable(WgoData data)
	{
		Item candidate;
		return TryGetInteractable(data, out candidate);
	}

	public bool TryGetInteractable(WgoData data, out Item candidate)
	{
		candidate = null;
		if (!condition.HasExpression)
		{
			return HasExecution(data);
		}
		PlayerData playerData = MainGame.PlayerData;
		if (playerData != null && playerData.HasMultipleOverheadItems)
		{
			IReadOnlyList<Item> overheadItems = playerData.OverheadItems;
			for (int num = overheadItems.Count - 1; num >= 0; num--)
			{
				Item item = overheadItems[num];
				if (item != null && !item.IsEmpty)
				{
					Item overheadCandidate = LazyExpressionEvaluationScope.OverheadCandidate;
					LazyExpressionEvaluationScope.OverheadCandidate = item;
					bool num2 = condition.EvaluateBool(data);
					LazyExpressionEvaluationScope.OverheadCandidate = overheadCandidate;
					if (num2)
					{
						candidate = item;
						return true;
					}
				}
			}
			return false;
		}
		bool num3 = condition.EvaluateBool(data);
		if (num3 && playerData != null && playerData.HasOverheadItem)
		{
			candidate = playerData.overheadItem;
		}
		return num3;
	}

	public bool HasExecution(WgoData data)
	{
		if (execution.Count <= 1)
		{
			if (execution.Count == 1)
			{
				return execution[0].HasExpression;
			}
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return condition.ToString();
	}
}
