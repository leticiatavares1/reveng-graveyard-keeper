using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InspirationData
{
	public string id;

	public int curLevel;

	public int curProgressValue;

	public int completionGoalValue;

	public bool isAllLevelsBought;

	[NonSerialized]
	private List<InspirationDef> purchasedInspirations = new List<InspirationDef>();

	[NonSerialized]
	private List<InspirationDef> completedInspirations = new List<InspirationDef>();

	public bool IsCompleted => curProgressValue >= completionGoalValue;

	public float Progress01 => Mathf.Clamp01((float)curProgressValue / (float)completionGoalValue);

	public int MaxLevel => GameBalance.Me.inspirationLevelsCache[id].levels.Count;

	public List<InspirationDef> PurchasedInspirations => purchasedInspirations;

	public List<InspirationDef> CompletedInspirations => completedInspirations;

	public bool IsAvailableToBuy
	{
		get
		{
			if (IsCompleted && !isAllLevelsBought)
			{
				return MainGame.PlayerData.inventory.Data.HasItemQuantityInInventory("faith", InspirationDef.GetDataForLevel(id, curLevel).completionPrice);
			}
			return false;
		}
	}

	public bool IsHidden => MainGame.Instance.GameSave.knowledgeSystem.IsInspirationHidden(id);

	public void DoLevelUp()
	{
		purchasedInspirations.Insert(0, InspirationDef.GetDataForLevel(id, curLevel));
		InspirationLevelData inspirationLevelData = GameBalance.Me.inspirationLevelsCache[id];
		isAllLevelsBought = curLevel == inspirationLevelData.levels.Count;
		int level = Mathf.Clamp(curLevel + 1, 1, inspirationLevelData.levels.Count);
		InspirationDef dataForLevel = inspirationLevelData.GetDataForLevel(level);
		curLevel = level;
		completionGoalValue = dataForLevel.completionGoalValue;
		MainGame.Instance.GameSave.knowledgeSystem.TryRevealInspirations();
	}

	public void FillBoughtInspirations()
	{
		purchasedInspirations.Clear();
		for (int i = 1; i < curLevel; i++)
		{
			purchasedInspirations.Add(InspirationDef.GetDataForLevel(id, i));
		}
		if (isAllLevelsBought)
		{
			purchasedInspirations.Add(InspirationDef.GetDataForLevel(id, curLevel));
		}
		purchasedInspirations.Reverse();
	}

	public void FillCompletedInspirations()
	{
		if (IsHidden || isAllLevelsBought || !IsCompleted)
		{
			return;
		}
		for (int i = curLevel; i <= MaxLevel; i++)
		{
			InspirationDef dataForLevel = InspirationDef.GetDataForLevel(id, i);
			if (!completedInspirations.Contains(dataForLevel) && curProgressValue >= dataForLevel.completionGoalValue)
			{
				completedInspirations.Add(dataForLevel);
			}
		}
	}
}
