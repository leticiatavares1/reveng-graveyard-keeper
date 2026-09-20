using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class KnowledgeSystem
{
	public static Action<string, bool> OnTechUnlocked;

	public static Action<string> OnTechRevealed;

	public static Action<string> OnTalentLevelUpRevealed;

	public static Action<string> OnTutorialViewed;

	public List<string> unlockedPhrases = new List<string>();

	public List<string> blackListPhrases = new List<string>();

	public List<string> unlockedTechs = new List<string>();

	public List<string> delayedDemoTechUnlocks = new List<string>();

	public List<string> hiddenTechs = new List<string>();

	public List<string> revealedTechs;

	public List<string> unlockedCrafts = new List<string>();

	public List<string> unlockedTownBuildings = new List<string>();

	public List<string> lockedTownBuildings = new List<string>();

	public List<string> oneTimeCompletedCrafts = new List<string>();

	public List<string> blackListCrafts = new List<string>();

	public List<string> unlockedBuildings = new List<string>();

	public List<string> lockedBuildings = new List<string>();

	public List<CharacterWindowData.CharPage> lockedCharacterWindowTabs = new List<CharacterWindowData.CharPage>();

	public List<TechTreeTab> lockedTechTabs = new List<TechTreeTab>();

	public List<string> unlockedTutorials = new List<string>();

	public List<string> viewedTutorials = new List<string>();

	public List<string> hiddenAlchemyFormulas = new List<string>();

	public List<string> unlockedAlchemyFormulas = new List<string>();

	public List<string> knownMixCrafts = new List<string>();

	public List<string> hiddenTalentLevelUps = new List<string>();

	public List<string> revealedTalentLevelUps;

	public List<string> unknownTalentLevelUps = new List<string>();

	public List<string> unlockedTalentIds = new List<string>();

	public List<string> freeZombieNames = new List<string>();

	public List<ItemType> unlockedOrgans = new List<ItemType>();

	public List<string> hiddenInspirations = new List<string>();

	public List<string> knownMapZones = new List<string>();

	public List<string> visitedWorldZones = new List<string>();

	public List<string> activeMapFightIcons = new List<string>();

	public List<string> unlockedVendorsForOrders = new List<string>();

	public List<string> unlockedCustomHudDaySprites = new List<string>();

	public void PrepareForGame()
	{
		if (unlockedCustomHudDaySprites == null)
		{
			unlockedCustomHudDaySprites = new List<string>();
		}
		SyncNewTechsFromBalance();
		RebuildUnlockedTechRewards();
		MainGame.Instance.GameSave.questSystemData.OnQuestCompleted -= HandleQuestCompleted;
		MainGame.Instance.GameSave.questSystemData.OnQuestCompleted += HandleQuestCompleted;
		SyncTalentLevelUpsVisibilityFromBalance();
		TryUnlockInspirationTab();
	}

	private void SyncNewTechsFromBalance()
	{
		List<TechDef> list = GameBalance.Me?.techDefs;
		if (list == null)
		{
			return;
		}
		if (revealedTechs == null)
		{
			revealedTechs = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				TechDef techDef = list[i];
				if (techDef != null && !string.IsNullOrEmpty(techDef.id) && techDef.hiddenAtStart && !IsTechUnlocked(techDef.id) && !IsTechHidden(techDef.id))
				{
					revealedTechs.Add(techDef.id);
				}
			}
		}
		else
		{
			RemoveMissingTechs(revealedTechs);
		}
		for (int j = 0; j < list.Count; j++)
		{
			TechDef techDef2 = list[j];
			if (techDef2 != null && !string.IsNullOrEmpty(techDef2.id))
			{
				ApplyNewTechIntroState(techDef2);
			}
		}
		RevealTechsFromCompletedQuests();
		CatchUpRevealedTechsFromTree();
	}

	public void RevealTechsFromCompletedQuests()
	{
		QuestSystemData questSystemData = MainGame.Instance.GameSave.questSystemData;
		List<QuestDef> list = GameBalance.Me?.questDefs;
		if (questSystemData == null || list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			QuestDef questDef = list[i];
			if (questDef != null && !string.IsNullOrEmpty(questDef.id) && questSystemData.IsQuestInStatus(questDef.id, QuestStatus.Completed))
			{
				ReplayRevealTechExpressions(questDef.id, questDef.execExpressionsStart);
				ReplayRevealTechExpressions(questDef.id, questDef.execExpressionsFinish);
			}
		}
	}

	private void ReplayRevealTechExpressions(string questId, List<LazyExpression> expressions)
	{
		if (expressions == null)
		{
			return;
		}
		for (int i = 0; i < expressions.Count; i++)
		{
			if (TryGetRevealTechId(expressions[i], out var techId) && IsTechHidden(techId))
			{
				RevealTech(techId);
				Debug.Log("Knowledge: reveal tech [" + techId + "] from completed quest [" + questId + "]");
			}
		}
	}

	private static bool TryGetRevealTechId(LazyExpression expression, out string techId)
	{
		techId = null;
		string text = expression?.GetRawExpressionString();
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (!text.StartsWith("RevealTech(\"", StringComparison.Ordinal) || !text.EndsWith("\")", StringComparison.Ordinal))
		{
			return false;
		}
		int num = text.Length - "RevealTech(\"".Length - 2;
		if (num <= 0)
		{
			return false;
		}
		techId = text.Substring("RevealTech(\"".Length, num);
		return true;
	}

	public void CatchUpRevealedTechsFromTree()
	{
		FillRevealedTechsFromTreeState();
		RevealHiddenTechsIfParentVisibleInTree();
	}

	public void FillRevealedTechsFromTreeState()
	{
		List<TechDef> list = GameBalance.Me?.techDefs;
		if (list == null)
		{
			return;
		}
		if (revealedTechs == null)
		{
			revealedTechs = new List<string>();
		}
		for (int i = 0; i < list.Count; i++)
		{
			TechDef techDef = list[i];
			if (techDef != null && !string.IsNullOrEmpty(techDef.id) && techDef.hiddenAtStart && !IsTechHidden(techDef.id))
			{
				MarkTechRevealed(techDef.id);
			}
		}
	}

	public void RevealHiddenTechsIfParentVisibleInTree()
	{
		List<TechDef> list = GameBalance.Me?.techDefs;
		if (list == null)
		{
			return;
		}
		bool flag;
		do
		{
			flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				TechDef techDef = list[i];
				if (techDef != null && !string.IsNullOrEmpty(techDef.id) && IsTechHidden(techDef.id) && HasVisibleHiddenAtStartParent(techDef))
				{
					RevealTech(techDef.id);
					flag = true;
				}
			}
		}
		while (flag);
	}

	private bool HasVisibleHiddenAtStartParent(TechDef techDef)
	{
		List<string> parents = techDef.parents;
		if (parents == null || parents.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < parents.Count; i++)
		{
			string text = parents[i];
			if (!string.IsNullOrEmpty(text) && !IsTechHidden(text))
			{
				TechDef dataOrNull = GameBalance.Me.GetDataOrNull<TechDef>(text);
				if (dataOrNull != null && dataOrNull.hiddenAtStart)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ApplyNewTechIntroState(TechDef techDef)
	{
		bool flag = IsTechRevealed(techDef.id);
		if (techDef.hiddenAtStart && !IsTechUnlocked(techDef.id) && !IsTechHidden(techDef.id) && !flag)
		{
			hiddenTechs.Add(techDef.id);
			Debug.Log("Knowledge: hide new tech [" + techDef.id + "]");
		}
		else if (!techDef.hiddenAtStart && !IsTechUnlocked(techDef.id) && IsTechHidden(techDef.id))
		{
			hiddenTechs.Remove(techDef.id);
			Debug.Log("Knowledge: unhide tech [" + techDef.id + "]");
		}
		if (techDef.availableAtStart && !IsTechUnlocked(techDef.id))
		{
			UnlockTech(techDef.id, silent: true);
		}
	}

	private void MarkTechRevealed(string techId)
	{
		if (revealedTechs == null)
		{
			revealedTechs = new List<string>();
		}
		if (!revealedTechs.Contains(techId))
		{
			revealedTechs.Add(techId);
		}
	}

	private static void RemoveMissingTechs(List<string> ids)
	{
		if (ids == null)
		{
			return;
		}
		for (int num = ids.Count - 1; num >= 0; num--)
		{
			if (GameBalance.Me.GetDataOrNull<TechDef>(ids[num]) == null)
			{
				ids.RemoveAt(num);
			}
		}
	}

	private void RebuildUnlockedTechRewards()
	{
		List<TechDef> list = GameBalance.Me?.techDefs;
		if (list == null)
		{
			return;
		}
		HashSet<string> hashSet = ((unlockedTechs != null) ? new HashSet<string>(unlockedTechs) : new HashSet<string>());
		HashSet<string> expected = new HashSet<string>();
		HashSet<string> expected2 = new HashSet<string>();
		HashSet<string> expected3 = new HashSet<string>();
		HashSet<string> expected4 = new HashSet<string>();
		for (int i = 0; i < list.Count; i++)
		{
			TechDef techDef = list[i];
			if (techDef != null && hashSet.Contains(techDef.id))
			{
				CollectTechRewardIds(techDef.craftsAfterUnlock, expected);
				CollectTechRewardIds(techDef.alchemyFormulasAfterUnlock, expected2);
				CollectTechRewardIds(techDef.buildingsAfterUnlock, expected3);
				CollectTechRewardIds(techDef.townBuildingsAfterUnlock, expected4);
			}
		}
		UnlockExpectedTechRewards(expected, UnlockCraft);
		UnlockExpectedTechRewards(expected2, UnlockAlchemyFormula);
		UnlockExpectedTechRewards(expected3, UnlockBuilding);
		UnlockExpectedTechRewards(expected4, UnlockTownBuilding);
	}

	private static void CollectTechRewardIds(List<string> source, HashSet<string> expected)
	{
		if (source == null)
		{
			return;
		}
		for (int i = 0; i < source.Count; i++)
		{
			string text = source[i];
			if (!string.IsNullOrEmpty(text))
			{
				expected.Add(text);
			}
		}
	}

	private static void UnlockExpectedTechRewards(HashSet<string> expected, Action<string> unlock)
	{
		foreach (string item in expected)
		{
			unlock(item);
		}
	}

	public void UnPrepareForGame()
	{
		MainGame.Instance.GameSave.questSystemData.OnQuestCompleted -= HandleQuestCompleted;
	}

	private void HandleQuestCompleted(QuestData _)
	{
		TryRevealInspirations();
	}

	private void SyncTalentLevelUpsVisibilityFromBalance()
	{
		RemoveMissingTalentLevelUps(hiddenTalentLevelUps);
		RemoveMissingTalentLevelUps(unknownTalentLevelUps);
		TalentSystemData talentSystemData = MainGame.Instance.GameSave.talentSystemData;
		if (revealedTalentLevelUps == null)
		{
			revealedTalentLevelUps = new List<string>();
			foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
			{
				if (talentLevelUpDef.isHidden && !IsPlayerTalentLevelUpStudied(talentSystemData, talentLevelUpDef) && !hiddenTalentLevelUps.Contains(talentLevelUpDef.id))
				{
					revealedTalentLevelUps.Add(talentLevelUpDef.id);
				}
			}
		}
		else
		{
			RemoveMissingTalentLevelUps(revealedTalentLevelUps);
		}
		foreach (TalentLevelUpDef talentLevelUpDef2 in GameBalance.Me.talentLevelUpDefs)
		{
			bool flag = IsPlayerTalentLevelUpStudied(talentSystemData, talentLevelUpDef2);
			bool flag2 = revealedTalentLevelUps.Contains(talentLevelUpDef2.id);
			if (talentLevelUpDef2.isHidden)
			{
				if (!flag && !flag2 && !hiddenTalentLevelUps.Contains(talentLevelUpDef2.id))
				{
					hiddenTalentLevelUps.Add(talentLevelUpDef2.id);
				}
			}
			else
			{
				hiddenTalentLevelUps.Remove(talentLevelUpDef2.id);
			}
			if (talentLevelUpDef2.isUnknown)
			{
				if (!flag && !flag2 && !unknownTalentLevelUps.Contains(talentLevelUpDef2.id))
				{
					unknownTalentLevelUps.Add(talentLevelUpDef2.id);
				}
			}
			else
			{
				unknownTalentLevelUps.Remove(talentLevelUpDef2.id);
			}
		}
	}

	private static bool IsPlayerTalentLevelUpStudied(TalentSystemData talentSystem, TalentLevelUpDef def)
	{
		if (def.isZombiePerk)
		{
			return false;
		}
		return talentSystem.GetTalentBranch(def.talentId)?.studiedLevelUps.Contains(def.id) ?? false;
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

	public void UnlockVendorForOrders(string vendorId)
	{
		if (!unlockedVendorsForOrders.Contains(vendorId))
		{
			unlockedVendorsForOrders.Add(vendorId);
			Debug.Log("Knowledge: unlock vendor for orders [" + vendorId + "]");
		}
	}

	public bool IsVendorForOrdersUnlocked(string vendorId)
	{
		return unlockedVendorsForOrders.Contains(vendorId);
	}

	public void UnlockTalentBranch(string talentBranchId)
	{
		if (unlockedTalentIds.Contains(talentBranchId))
		{
			Debug.LogWarning("TalentBranch [" + talentBranchId + "] is already unlocked");
		}
		else
		{
			unlockedTalentIds.Add(talentBranchId);
		}
	}

	public bool IsTalentBranchUnlocked(string talentBranchId)
	{
		return unlockedTalentIds.Contains(talentBranchId);
	}

	public bool IsTalentBranchHasWaitingInspiration(string talentBranchId)
	{
		return unlockedTalentIds.Contains(talentBranchId);
	}

	public void AddPhraseToBlackList(string phraseId)
	{
		if (!blackListPhrases.Contains(phraseId))
		{
			blackListPhrases.Add(phraseId);
		}
	}

	public void RemovePhraseFromBlackList(string phraseId)
	{
		blackListPhrases.Remove(phraseId);
	}

	public void UnlockPhrase(string phraseId)
	{
		if (!unlockedPhrases.Contains(phraseId))
		{
			unlockedPhrases.Add(phraseId);
		}
	}

	public void UnlockTech(string techId, bool silent)
	{
		if (!unlockedTechs.Contains(techId))
		{
			unlockedTechs.Add(techId);
			Debug.Log("Knowledge: unlock tech [" + techId + "]");
			OnTechUnlocked?.Invoke(techId, silent);
		}
	}

	public void AddDelayedDemoTechUnlock(string techId)
	{
		if (!string.IsNullOrEmpty(techId) && !IsTechUnlocked(techId))
		{
			if (delayedDemoTechUnlocks == null)
			{
				delayedDemoTechUnlocks = new List<string>();
			}
			if (!delayedDemoTechUnlocks.Contains(techId))
			{
				delayedDemoTechUnlocks.Add(techId);
				Debug.Log("Knowledge: delay demo tech unlock [" + techId + "]");
			}
		}
	}

	public void ApplyDelayedDemoTechUnlocks()
	{
		if (delayedDemoTechUnlocks == null || delayedDemoTechUnlocks.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>(delayedDemoTechUnlocks);
		delayedDemoTechUnlocks.Clear();
		foreach (string item in list)
		{
			TechDef dataOrNull = GameBalance.Me.GetDataOrNull<TechDef>(item);
			if (dataOrNull != null && !IsTechUnlocked(item))
			{
				dataOrNull.Unlock(free: true);
			}
		}
	}

	public void RemoveTech(string techId)
	{
		if (unlockedTechs.Contains(techId))
		{
			unlockedTechs.Remove(techId);
			Debug.Log("Knowledge: remove tech [" + techId + "]");
		}
	}

	public bool IsTechUnlocked(string techId)
	{
		return unlockedTechs.Contains(techId);
	}

	public bool IsTechHidden(string techId)
	{
		return hiddenTechs.Contains(techId);
	}

	public bool IsTechRevealed(string techId)
	{
		if (revealedTechs != null)
		{
			return revealedTechs.Contains(techId);
		}
		return false;
	}

	public void HideTech(string techId)
	{
		if (revealedTechs != null)
		{
			revealedTechs.Remove(techId);
		}
		if (IsTechHidden(techId))
		{
			return;
		}
		hiddenTechs.Add(techId);
		Debug.Log("Knowledge: hide tech [" + techId + "]");
		foreach (TechDef childDefinition in GameBalance.Me.GetData<TechDef>(techId).childDefinitionList)
		{
			HideTech(childDefinition.id);
		}
	}

	public void RevealTech(string techId)
	{
		MarkTechRevealed(techId);
		if (!IsTechHidden(techId))
		{
			return;
		}
		hiddenTechs.Remove(techId);
		Debug.Log("Knowledge: reveal tech [" + techId + "]");
		foreach (TechDef childDefinition in GameBalance.Me.GetData<TechDef>(techId).childDefinitionList)
		{
			RevealTech(childDefinition.id);
		}
		OnTechRevealed?.Invoke(techId);
	}

	public void UnlockTechTab(TechTreeTab techTreeTab)
	{
		if (IsTechTabLocked(techTreeTab))
		{
			lockedTechTabs.Remove(techTreeTab);
		}
	}

	public void LockTechTab(TechTreeTab techTreeTab)
	{
		if (!IsTechTabLocked(techTreeTab))
		{
			lockedTechTabs.Add(techTreeTab);
		}
	}

	public void UnlockMapFightIcon(string icon)
	{
		if (!activeMapFightIcons.Contains(icon))
		{
			activeMapFightIcons.Add(icon);
		}
	}

	public void LockMapFightIcon(string icon)
	{
		if (activeMapFightIcons.Contains(icon))
		{
			activeMapFightIcons.Remove(icon);
		}
	}

	public bool IsTechTabLocked(TechTreeTab techTreeTab)
	{
		return lockedTechTabs.Contains(techTreeTab);
	}

	public void UnlockCharTab(CharacterWindowData.CharPage page)
	{
		if (IsCharTabLocked(page))
		{
			lockedCharacterWindowTabs.Remove(page);
		}
	}

	public void LockCharTab(CharacterWindowData.CharPage page)
	{
		if (!IsCharTabLocked(page))
		{
			lockedCharacterWindowTabs.Add(page);
		}
	}

	public bool IsCharTabLocked(CharacterWindowData.CharPage page)
	{
		return lockedCharacterWindowTabs.Contains(page);
	}

	public void TryUnlockInspirationTab()
	{
		if (IsCharTabLocked(CharacterWindowData.CharPage.Inspiration) && MainGame.Instance.GameSave.talentSystemData.HasTwoZeroFaithInspirationsToBuyInSameBranch())
		{
			UnlockCharTab(CharacterWindowData.CharPage.Inspiration);
		}
	}

	public bool IsTechTabUnlocked(TechTreeTab techTreeTab)
	{
		return !lockedTechTabs.Contains(techTreeTab);
	}

	public void UnlockCraft(string craftId)
	{
		if (!unlockedCrafts.Contains(craftId))
		{
			unlockedCrafts.Add(craftId);
			Debug.Log("Knowledge: unlock craft [" + craftId + "]");
		}
	}

	public void RemoveUnlockedCraft(string craftId)
	{
		if (unlockedCrafts.Remove(craftId))
		{
			Debug.Log("Knowledge: remove unlocked craft [" + craftId + "]");
		}
	}

	public bool IsSurveyCompleted(SurveyDef surveyDef)
	{
		if (surveyDef == null || !surveyDef.isOneTimeCraft)
		{
			return false;
		}
		if (!surveyDef.surveyedAtStart)
		{
			return oneTimeCompletedCrafts.Contains(surveyDef.id);
		}
		return true;
	}

	public bool IsOneTimeCraftCompleted(CraftDefBase craftDef)
	{
		if (craftDef is SurveyDef surveyDef)
		{
			return IsSurveyCompleted(surveyDef);
		}
		if (craftDef != null && craftDef.IsOneTimeCraft())
		{
			return oneTimeCompletedCrafts.Contains(craftDef.id);
		}
		return false;
	}

	public void CompleteOneTimeCraft(CraftDefBase craftDef)
	{
		if (craftDef != null && craftDef.IsOneTimeCraft() && !oneTimeCompletedCrafts.Contains(craftDef.id))
		{
			oneTimeCompletedCrafts.Add(craftDef.id);
			Debug.Log("Knowledge: one time craft completed [" + craftDef.id + "]");
		}
	}

	public void UnlockOrgan(ItemType itemType)
	{
		if (!unlockedOrgans.Contains(itemType))
		{
			unlockedOrgans.Add(itemType);
			Debug.Log($"Knowledge: unlock organ [{itemType}]");
		}
	}

	public void UnlockTownBuilding(string buildingId)
	{
		if (!unlockedTownBuildings.Contains(buildingId))
		{
			unlockedTownBuildings.Add(buildingId);
			Debug.Log("Knowledge: unlock town building [" + buildingId + "]");
		}
	}

	public void LockTownBuilding(string buildingId)
	{
		if (!lockedTownBuildings.Contains(buildingId))
		{
			lockedTownBuildings.Add(buildingId);
			Debug.Log("Knowledge: locked town building [" + buildingId + "]");
		}
	}

	public void UnlockAlchemyFormula(string id)
	{
		if (!unlockedAlchemyFormulas.Contains(id))
		{
			unlockedAlchemyFormulas.Add(id);
			Debug.Log("Knowledge: alchemy formula [" + id + "]");
		}
	}

	public bool IsAlchemyFormulaKnown(AlchemyFormulaDef formula)
	{
		if (formula == null)
		{
			return true;
		}
		if (formula.hiddenAtStart)
		{
			return unlockedAlchemyFormulas.Contains(formula.id);
		}
		return true;
	}

	public void DiscoverAlchemyMix(AlchemyMixDef mixDef)
	{
		if (mixDef != null)
		{
			if (!knownMixCrafts.Contains(mixDef.id))
			{
				knownMixCrafts.Add(mixDef.id);
			}
			CraftDef alchemyWorkBenchCraft = mixDef.AlchemyWorkBenchCraft;
			if (alchemyWorkBenchCraft != null)
			{
				UnlockCraft(alchemyWorkBenchCraft.id);
			}
			AlchemyFormulaDef formula = mixDef.Formula;
			if (formula != null)
			{
				UnlockAlchemyFormula(formula.id);
			}
		}
	}

	public void UnlockTutorial(string id)
	{
		if (!unlockedTutorials.Contains(id))
		{
			unlockedTutorials.Add(id);
		}
	}

	public void AddViewedTutorial(string id)
	{
		if (viewedTutorials == null)
		{
			viewedTutorials = new List<string>();
		}
		if (!viewedTutorials.Contains(id))
		{
			viewedTutorials.Add(id);
			OnTutorialViewed?.Invoke(id);
		}
	}

	public bool HasViewedTutorials()
	{
		if (viewedTutorials != null)
		{
			return viewedTutorials.Count > 0;
		}
		return false;
	}

	public void BlackListCraft(string craftId)
	{
		if (!blackListCrafts.Contains(craftId))
		{
			blackListCrafts.Add(craftId);
			Debug.Log("Knowledge: craft to black list [" + craftId + "]");
		}
	}

	public void RemoveCraftFromBlackList(string craftId)
	{
		if (blackListCrafts.Contains(craftId))
		{
			blackListCrafts.Remove(craftId);
			Debug.Log("Knowledge: craft removed from black list [" + craftId + "]");
		}
	}

	public void UnlockBuilding(string buildingId)
	{
		if (!unlockedBuildings.Contains(buildingId))
		{
			unlockedBuildings.Add(buildingId);
			Debug.Log("Knowledge: unlock building [" + buildingId + "]");
		}
	}

	public void LockBuilding(string buildingId)
	{
		if (!lockedBuildings.Contains(buildingId))
		{
			lockedBuildings.Add(buildingId);
			Debug.Log("Knowledge: lock building [" + buildingId + "]");
		}
	}

	public void RevealTalentLevelUp(string talentLevelUpId)
	{
		if (revealedTalentLevelUps == null)
		{
			revealedTalentLevelUps = new List<string>();
		}
		if (!revealedTalentLevelUps.Contains(talentLevelUpId))
		{
			revealedTalentLevelUps.Add(talentLevelUpId);
		}
		if (hiddenTalentLevelUps.Contains(talentLevelUpId))
		{
			hiddenTalentLevelUps.Remove(talentLevelUpId);
			Debug.Log("Knowledge: reveal talent levelUp [" + talentLevelUpId + "]");
			OnTalentLevelUpRevealed?.Invoke(talentLevelUpId);
		}
	}

	public string GetZombieName()
	{
		if (freeZombieNames.Count == 0)
		{
			Debug.Log("KnowledgeSystem.GetZombieName: freeZombieNames is empty. Use default zombie_name_1.");
			return LLBase.L("zombie_name_1");
		}
		string random = freeZombieNames.GetRandom();
		freeZombieNames.Remove(random);
		return random;
	}

	public bool IsInspirationHidden(string inspirationId)
	{
		return hiddenInspirations.Contains(inspirationId);
	}

	public void TryRevealInspirations()
	{
		for (int num = hiddenInspirations.Count - 1; num >= 0; num--)
		{
			if (TalentSystemCache.Instance.inspirations.TryGetValue(hiddenInspirations[num], out var value) && GameBalance.Me.inspirationLevelsCache.TryGetValue(hiddenInspirations[num], out var value2))
			{
				bool flag = true;
				foreach (string inpsirationLock in value2.inspirationLocks)
				{
					int num2 = inpsirationLock.LastIndexOf('_');
					if (num2 == -1)
					{
						num2 = inpsirationLock.Length;
					}
					if (TalentSystemCache.Instance.inspirations.TryGetValue(inpsirationLock.Substring(0, num2), out var value3) && value3.PurchasedInspirations.Find((InspirationDef x) => x.id == inpsirationLock) == null)
					{
						flag = false;
						break;
					}
				}
				foreach (string techLock in value2.techLocks)
				{
					if (!unlockedTechs.Contains(techLock))
					{
						flag = false;
						break;
					}
				}
				foreach (string questLock in value2.questLocks)
				{
					if (!MainGame.Instance.GameSave.questSystemData.IsQuestInStatus(questLock, QuestStatus.Completed))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					RevealInspiration(value);
				}
			}
		}
	}

	public void RevealInspiration(string inspirationId)
	{
		if (hiddenInspirations.Contains(inspirationId) && TalentSystemCache.Instance.inspirations.TryGetValue(inspirationId, out var value))
		{
			RevealInspiration(value);
		}
	}

	private void RevealInspiration(InspirationData inspirationData)
	{
		Debug.Log("Inspiration: reveal inspiration [" + inspirationData.id + "]");
		hiddenInspirations.Remove(inspirationData.id);
		MainGame.Instance.GameSave.talentSystemData.CompleteInspiration(inspirationData);
	}
}
