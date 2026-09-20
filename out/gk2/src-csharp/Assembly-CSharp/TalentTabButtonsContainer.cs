using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TalentTabButtonsContainer : MonoBehaviour
{
	[SerializeField]
	private TalentTabButton tabButtonPrefab;

	[Space]
	private List<TalentTabButton> talentTabButtons = new List<TalentTabButton>();

	[SerializeField]
	private ScrollRect scrollRect;

	private int currentTab;

	private Action<string> onTalentSelected;

	private Canvas canvas;

	public void Init(Action<string> onTalentSelected, Canvas canvas)
	{
		this.onTalentSelected = onTalentSelected;
		this.canvas = canvas;
		foreach (TalentDef talentDef in GameBalance.Me.talentDefs)
		{
			string talentId = talentDef.id;
			TalentTabButton talentTabButton = tabButtonPrefab.Copy(base.transform, activate: false, talentId);
			talentTabButton.Init(talentId, delegate
			{
				DisplayTab(talentId);
			});
			talentTabButtons.Add(talentTabButton);
			talentTabButton.UpdateState(isActive: false, canvas);
		}
		talentTabButtons[0].UpdateState(isActive: true, canvas);
	}

	public void Draw(string currentTabName = "", bool blockCurrentTalentLevelUpActionIndicator = true)
	{
		if (!string.IsNullOrEmpty(currentTabName))
		{
			UpdateCurrentTab(currentTabName);
		}
		if (blockCurrentTalentLevelUpActionIndicator)
		{
			MainGame.Instance.GameSave.talentSystemData.GetTalentBranch(talentTabButtons[currentTab].TalentId).BlockTalentLevelUpActionIndicator();
		}
		foreach (TalentTabButton talentTabButton in talentTabButtons)
		{
			talentTabButton.gameObject.SetActive(MainGame.Instance.GameSave.knowledgeSystem.IsTalentBranchUnlocked(talentTabButton.TalentId));
			talentTabButton.UpdateSorting(canvas);
			TalentData talentBranch = MainGame.Instance.GameSave.talentSystemData.GetTalentBranch(talentTabButton.TalentId);
			bool actionIndicatorState = talentBranch.HasAvailableTalentLevelUpActionIndicator || MainGame.Instance.GameSave.talentSystemData.HasTwoZeroFaithInspirationsToBuy(talentBranch);
			talentTabButton.SetActionIndicatorState(actionIndicatorState);
			talentTabButton.DrawMasteryValue();
		}
		foreach (TalentTabButton talentTabButton2 in talentTabButtons)
		{
			talentTabButton2.UpdateState(talentTabButtons[currentTab] == talentTabButton2, canvas);
		}
	}

	public bool OnPressedPrevTechTab(GamepadNavigationController gamepadNavigationController = null, AutoScroll autoScroll = null)
	{
		int num = currentTab;
		num--;
		if (num < 0)
		{
			num = talentTabButtons.Count - 1;
		}
		DisplayTab(talentTabButtons[num].TalentId);
		if (gamepadNavigationController != null && autoScroll != null && LazyInput.IsGamepadActive)
		{
			autoScroll.SkipNextAutoscroll = true;
			gamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			scrollRect.DOKill();
			scrollRect.verticalNormalizedPosition = 1f;
		}
		return true;
	}

	public bool OnPressedNextTechTab(GamepadNavigationController gamepadNavigationController = null, AutoScroll autoScroll = null)
	{
		int num = currentTab;
		num++;
		if (num > talentTabButtons.Count - 1)
		{
			num = 0;
		}
		DisplayTab(talentTabButtons[num].TalentId);
		if (gamepadNavigationController != null && autoScroll != null && LazyInput.IsGamepadActive)
		{
			autoScroll.SkipNextAutoscroll = true;
			gamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			scrollRect.DOKill();
			scrollRect.verticalNormalizedPosition = 1f;
		}
		return true;
	}

	private void DisplayTab(string talendId)
	{
		UpdateCurrentTab(talendId);
		onTalentSelected?.Invoke(talendId);
		if (scrollRect != null)
		{
			scrollRect.ResetPosition();
		}
	}

	public void UpdateCurrentTab(string currentTalentId)
	{
		talentTabButtons[currentTab].UpdateState(isActive: false, canvas);
		currentTab = talentTabButtons.IndexOf(talentTabButtons.Find((TalentTabButton t) => t.TalentId == currentTalentId));
		talentTabButtons[currentTab].UpdateState(isActive: true, canvas);
	}

	private void Awake()
	{
		tabButtonPrefab.gameObject.SetActive(value: false);
	}
}
