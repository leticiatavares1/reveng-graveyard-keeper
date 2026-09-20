using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class TalentData : ObjectLinkedToDefinition<TalentDef>
{
	public int curExp;

	public int curTalentLevel;

	public int talentExpPoints;

	public int curTalentValue;

	public List<string> studiedLevelUps = new List<string>();

	public List<InspirationProgressData> inspirationsProgression = new List<InspirationProgressData>();

	public bool isTalentLevelUpActionIndicatorBlocked;

	[NonSerialized]
	public List<InspirationData> activeInspirations;

	[NonSerialized]
	public TalentExpLevelBalanceData talentExpLevelBalanceData;

	public bool CanUpgradeLevel => curExp >= talentExpLevelBalanceData.GetExpForLevel(curTalentLevel);

	public bool CanPurchaseAnyInspiration
	{
		get
		{
			for (int i = 0; i < activeInspirations.Count; i++)
			{
				if (activeInspirations[i].IsAvailableToBuy)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool HasAvailableTalentLevelUpActionIndicator
	{
		get
		{
			if (!isTalentLevelUpActionIndicatorBlocked)
			{
				return HasAvailableTalentLevelUpToPurchase();
			}
			return false;
		}
	}

	public TalentData()
	{
	}

	public TalentData(string talentId)
		: base(talentId)
	{
		curExp = 0;
		curTalentLevel = 1;
		curTalentValue = 0;
		foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
		{
			if (!talentLevelUpDef.isZombiePerk && talentLevelUpDef.talentId == talentId && talentLevelUpDef.availableAtStart)
			{
				studiedLevelUps.Add(talentLevelUpDef.id);
				if (!string.IsNullOrEmpty(talentLevelUpDef.linkedPerk))
				{
					MainGame.Instance.GameSave.perkSystemData.AddPerk(talentLevelUpDef.linkedPerk);
				}
				if (talentLevelUpDef.talentValueAdd > 0)
				{
					curTalentValue += talentLevelUpDef.talentValueAdd;
				}
			}
		}
	}

	public TalentLevelUpDef.State GetLevelUpState(TalentLevelUpDef def)
	{
		if (studiedLevelUps.Contains(def.id))
		{
			return TalentLevelUpDef.State.Unlocked;
		}
		if (MainGame.Instance.GameSave.knowledgeSystem.hiddenTalentLevelUps.Contains(def.id))
		{
			return TalentLevelUpDef.State.Hidden;
		}
		if (MainGame.Instance.GameSave.knowledgeSystem.unknownTalentLevelUps.Contains(def.id))
		{
			return TalentLevelUpDef.State.Unknown;
		}
		if (def.ParentsUnlocked && talentExpPoints >= def.talentExpPointsPrice)
		{
			return TalentLevelUpDef.State.Available;
		}
		return TalentLevelUpDef.State.Visible;
	}

	public bool HasAvailableTalentLevelUpToPurchase()
	{
		for (int i = 0; i < GameBalance.Me.talentLevelUpDefs.Count; i++)
		{
			TalentLevelUpDef talentLevelUpDef = GameBalance.Me.talentLevelUpDefs[i];
			if (!talentLevelUpDef.isZombiePerk && !(talentLevelUpDef.talentId != id) && GetLevelUpState(talentLevelUpDef) == TalentLevelUpDef.State.Available)
			{
				return true;
			}
		}
		return false;
	}

	public void BlockTalentLevelUpActionIndicator()
	{
		if (HasAvailableTalentLevelUpToPurchase())
		{
			isTalentLevelUpActionIndicatorBlocked = true;
		}
	}

	public void UnblockTalentLevelUpActionIndicator()
	{
		isTalentLevelUpActionIndicatorBlocked = false;
	}

	public void PrepareForGame()
	{
		activeInspirations = new List<InspirationData>();
		talentExpLevelBalanceData = GameBalance.Me.talentExpLevelsCache[id];
		TalentSystemCache instance = TalentSystemCache.Instance;
		if (instance.inspirationsByTalentId.TryGetValue(id, out var value))
		{
			foreach (InspirationLevelData item in value)
			{
				activeInspirations.Add(instance.inspirations[item.id]);
			}
		}
		if (inspirationsProgression.Count == 0)
		{
			return;
		}
		foreach (InspirationData activeInspiration in activeInspirations)
		{
			int num = inspirationsProgression.FindIndex((InspirationProgressData x) => x.id == activeInspiration.id);
			if (num == -1)
			{
				continue;
			}
			int completionGoalValue = inspirationsProgression[num].completionGoalValue;
			int num2 = 1;
			InspirationDef dataForLevel = InspirationDef.GetDataForLevel(inspirationsProgression[num].id, num2);
			for (completionGoalValue -= dataForLevel.completionGoalValue; completionGoalValue >= 0; completionGoalValue -= dataForLevel.completionGoalValue)
			{
				num2++;
				dataForLevel = InspirationDef.GetDataForLevel(inspirationsProgression[num].id, num2);
				if (dataForLevel == null)
				{
					num2--;
					dataForLevel = InspirationDef.GetDataForLevel(inspirationsProgression[num].id, num2);
					break;
				}
			}
			activeInspiration.curLevel = num2;
			activeInspiration.curProgressValue = inspirationsProgression[num].currentValue;
			activeInspiration.completionGoalValue = dataForLevel.completionGoalValue;
			if (num2 == activeInspiration.MaxLevel && completionGoalValue >= 0)
			{
				activeInspiration.isAllLevelsBought = true;
			}
			activeInspiration.FillBoughtInspirations();
			activeInspiration.FillCompletedInspirations();
		}
	}
}
