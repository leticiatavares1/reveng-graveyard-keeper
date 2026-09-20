using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class TalentSystemData
{
	public delegate void DelTalentExpChanged(string talentId, int curExp);

	public delegate void DelTalentLevelPurchased(string talentId, string talentLevelId);

	public delegate void DelTalentLevelUnlocked(string talentLevelId, List<string> talentLevelIds);

	public List<TalentData> talentData = new List<TalentData>();

	public event DelTalentExpChanged OnTalentExpChanged;

	public event Action<string> OnInspirationProgressChanged;

	public event Action<string> OnInspirationCompleted;

	public event Action<string> OnInspirationPurchased;

	public event DelTalentLevelPurchased OnTalentLevelPurchased;

	public TalentSystemData()
	{
	}

	public TalentSystemData(List<TalentDef> talentDefData)
	{
		foreach (TalentDef talentDefDatum in talentDefData)
		{
			TalentData item = new TalentData(talentDefDatum.id);
			talentData.Add(item);
		}
	}

	public void PrepareForGame()
	{
		SyncPlayerTalentLevelUpsFromBalance();
		TalentSystemCache.Instance.ClearCache();
		TalentSystemCache.Instance.CreateCache();
		foreach (TalentData talentDatum in talentData)
		{
			talentDatum.PrepareForGame();
		}
	}

	public void UnPrepareFromGame()
	{
		TalentSystemCache.Instance.ClearCache();
	}

	public void AddToInspiration(string inspirationId, int counterToAdd)
	{
		if (!GameBalance.Me.inspirationLevelsCache.TryGetValue(inspirationId, out var value))
		{
			return;
		}
		TalentData talentBranch = GetTalentBranch(value.talentId);
		if (TalentSystemCache.Instance.inspirations.TryGetValue(inspirationId, out var value2))
		{
			value2.curProgressValue += counterToAdd;
			int num = talentBranch.inspirationsProgression.FindIndex((InspirationProgressData x) => x.id == inspirationId);
			if (num == -1)
			{
				talentBranch.inspirationsProgression.Add(new InspirationProgressData(inspirationId, value2.curProgressValue));
			}
			else
			{
				talentBranch.inspirationsProgression[num].currentValue = value2.curProgressValue;
			}
			if (!value2.IsHidden)
			{
				CompleteInspiration(value2);
				this.OnInspirationProgressChanged?.Invoke(value2.id);
			}
		}
	}

	public void CompleteInspiration(InspirationData inspirationData)
	{
		if (inspirationData.isAllLevelsBought || !inspirationData.IsCompleted)
		{
			return;
		}
		MainGame.Instance.GameSave.knowledgeSystem.TryUnlockInspirationTab();
		for (int i = inspirationData.curLevel; i <= inspirationData.MaxLevel; i++)
		{
			InspirationDef dataForLevel = InspirationDef.GetDataForLevel(inspirationData.id, i);
			if (!inspirationData.CompletedInspirations.Contains(dataForLevel) && inspirationData.curProgressValue >= dataForLevel.completionGoalValue)
			{
				inspirationData.CompletedInspirations.Add(dataForLevel);
				this.OnInspirationCompleted?.Invoke(dataForLevel.id);
			}
		}
	}

	public bool HasTwoZeroFaithInspirationsToBuyInSameBranch()
	{
		foreach (TalentData talentDatum in talentData)
		{
			if (HasTwoZeroFaithInspirationsToBuy(talentDatum))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetTalentIdWithTwoZeroFaithInspirationsToBuy(out string talentId)
	{
		foreach (TalentData talentDatum in talentData)
		{
			if (HasTwoZeroFaithInspirationsToBuy(talentDatum))
			{
				talentId = talentDatum.id;
				return true;
			}
		}
		talentId = null;
		return false;
	}

	public bool HasTwoZeroFaithInspirationsToBuy(TalentData talent)
	{
		return CountZeroFaithInspirationsToBuy(talent) >= 2;
	}

	private static int CountZeroFaithInspirationsToBuy(TalentData talent)
	{
		if (talent.activeInspirations == null)
		{
			return 0;
		}
		int num = 0;
		foreach (InspirationData activeInspiration in talent.activeInspirations)
		{
			if (!activeInspiration.IsHidden && activeInspiration.IsCompleted && !activeInspiration.isAllLevelsBought)
			{
				InspirationDef dataForLevel = InspirationDef.GetDataForLevel(activeInspiration.id, activeInspiration.curLevel);
				if (dataForLevel != null && dataForLevel.completionPrice == 0)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void PurchaseInspiration(string inspirationId)
	{
		if (!GameBalance.Me.inspirationLevelsCache.TryGetValue(inspirationId, out var value))
		{
			return;
		}
		TalentData talentBranch = GetTalentBranch(value.talentId);
		if (TalentSystemCache.Instance.inspirations.TryGetValue(inspirationId, out var value2))
		{
			int completionExp = InspirationDef.GetDataForLevel(inspirationId, value2.curLevel).completionExp;
			AddExp(value.talentId, completionExp);
			int num = talentBranch.inspirationsProgression.FindIndex((InspirationProgressData x) => x.id == inspirationId);
			if (num == -1)
			{
				talentBranch.inspirationsProgression.Add(new InspirationProgressData(inspirationId, value2.curProgressValue, value2.completionGoalValue));
			}
			else
			{
				talentBranch.inspirationsProgression[num].completionGoalValue += value2.completionGoalValue;
			}
			value2.DoLevelUp();
			LazyAudio.PlayAndForget("get_inspiration");
			if (talentBranch.CanUpgradeLevel)
			{
				talentBranch.talentExpPoints++;
				talentBranch.UnblockTalentLevelUpActionIndicator();
				talentBranch.curExp -= talentBranch.talentExpLevelBalanceData.GetExpForLevel(talentBranch.curTalentLevel);
				talentBranch.curTalentLevel++;
			}
			AchievementsSystem.Instance.TriggerCountable("inspiration_unlocked");
			this.OnInspirationPurchased?.Invoke(inspirationId);
		}
		else
		{
			Debug.LogError("Inspiration [" + inspirationId + "] wasn't found for talent [" + value.talentId + "]");
		}
	}

	public TalentData GetTalentBranch(string talentId)
	{
		return talentData.Find((TalentData x) => x.id == talentId);
	}

	public bool AddTalentValue(string talentId, int value)
	{
		TalentData talentBranch = GetTalentBranch(talentId);
		if (talentBranch == null)
		{
			Debug.LogError("Talent [" + talentId + "] wasn't found");
			return false;
		}
		talentBranch.curTalentValue += value;
		return true;
	}

	public bool CanPurchaseLevel(string talentLevelId, out TalentData talentData)
	{
		talentData = null;
		TalentLevelUpDef data = GameBalance.Me.GetData<TalentLevelUpDef>(talentLevelId);
		if (data == null)
		{
			return false;
		}
		talentData = GetTalentBranch(data.talentId);
		if (talentData.talentExpPoints >= data.talentExpPointsPrice)
		{
			return data.ParentsUnlocked;
		}
		return false;
	}

	public void PurchaseLevel(string talentLevelId, bool free = false)
	{
		if (!CanPurchaseLevel(talentLevelId, out var talentData))
		{
			return;
		}
		talentData.studiedLevelUps.Add(talentLevelId);
		TalentLevelUpDef data = GameBalance.Me.GetData<TalentLevelUpDef>(talentLevelId);
		if (!string.IsNullOrEmpty(data.linkedPerk))
		{
			MainGame.Instance.GameSave.perkSystemData.AddPerk(data.linkedPerk);
		}
		if (!free)
		{
			talentData.talentExpPoints -= data.talentExpPointsPrice;
		}
		if (data.talentValueAdd > 0)
		{
			talentData.curTalentValue += data.talentValueAdd;
		}
		foreach (LazyExpression item in data.expressionsOnBuy)
		{
			item.Evaluate();
		}
		this.OnTalentLevelPurchased?.Invoke(talentData.id, talentLevelId);
	}

	private void AddExp(string talentBranchId, int exp)
	{
		TalentData talentBranch = GetTalentBranch(talentBranchId);
		if (talentBranch != null)
		{
			talentBranch.curExp += exp;
			this.OnTalentExpChanged?.Invoke(talentBranchId, talentBranch.curExp);
		}
	}

	private void SyncPlayerTalentLevelUpsFromBalance()
	{
		foreach (TalentData talentDatum in talentData)
		{
			RemoveMissingTalentLevelUps(talentDatum.studiedLevelUps);
		}
		PerkSystemData perkSystemData = MainGame.Instance.GameSave.perkSystemData;
		foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
		{
			if (talentLevelUpDef.isZombiePerk)
			{
				continue;
			}
			TalentData talentBranch = GetTalentBranch(talentLevelUpDef.talentId);
			if (talentBranch == null)
			{
				continue;
			}
			if (talentLevelUpDef.availableAtStart && !talentBranch.studiedLevelUps.Contains(talentLevelUpDef.id))
			{
				talentBranch.studiedLevelUps.Add(talentLevelUpDef.id);
				if (talentLevelUpDef.talentValueAdd > 0)
				{
					talentBranch.curTalentValue += talentLevelUpDef.talentValueAdd;
				}
			}
			if (talentBranch.studiedLevelUps.Contains(talentLevelUpDef.id) && !string.IsNullOrEmpty(talentLevelUpDef.linkedPerk) && !perkSystemData.HasPerk(talentLevelUpDef.linkedPerk))
			{
				perkSystemData.AddPerk(talentLevelUpDef.linkedPerk);
			}
		}
	}

	private static void RemoveMissingTalentLevelUps(List<string> ids)
	{
		for (int num = ids.Count - 1; num >= 0; num--)
		{
			if (GameBalance.Me.GetData<TalentLevelUpDef>(ids[num]) == null)
			{
				ids.RemoveAt(num);
			}
		}
	}
}
