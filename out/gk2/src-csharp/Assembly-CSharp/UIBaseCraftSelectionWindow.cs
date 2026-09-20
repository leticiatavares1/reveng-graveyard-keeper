using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBaseCraftSelectionWindow : LazyWindow<UIBaseCraftSelectionWindowData>
{
	protected Action onAddToCraftQueuePressed;

	protected Action onStartCraftPressed;

	protected Action onPlusQueuePressed;

	protected Action onMinusQueuePressed;

	[SerializeField]
	protected TextMeshProUGUI headerLabel;

	[SerializeField]
	protected Transform requirementsContainer;

	[SerializeField]
	protected GameObject requirementsGo;

	[SerializeField]
	protected LazyButton startCraftButton;

	[SerializeField]
	protected TextMeshProUGUI gamepadTipStartCraft;

	[SerializeField]
	private TextMeshProUGUI talentLabel;

	[SerializeField]
	private TextMeshProUGUI talentValueLabel;

	[SerializeField]
	protected TextStyle greenTalentStyle;

	[SerializeField]
	protected TextStyle redTalentStyle;

	[SerializeField]
	protected TextStyle starCraftTalentStyle;

	[SerializeField]
	protected TextStyle commonTalentStyle;

	[SerializeField]
	protected TextStyle commonTalentStyleButton;

	[SerializeField]
	protected TextStyle redTalentStyleButton;

	[SerializeField]
	protected TextStyle slashTalentStyleButton;

	[SerializeField]
	private Image talentOverlap;

	[SerializeField]
	[Space]
	protected LazyButton plusCraftButton;

	[SerializeField]
	protected LazyButton minusCraftButton;

	[SerializeField]
	protected RectTransform progressBarParent;

	[SerializeField]
	private GameObject backgroundProgressBarCellImagePrefab;

	[SerializeField]
	private RectTransform backgroundProgressBarCellGroupParent;

	[SerializeField]
	protected UIProgressCellsInfoWidget progressCellsInfoWidget;

	[SerializeField]
	protected TextMeshProUGUI startCraftButtonText;

	[SerializeField]
	protected List<UICraftItemCell> displayedIngredients = new List<UICraftItemCell>();

	protected List<ProgressCellCraft> progressCells = new List<ProgressCellCraft>();

	private List<GameObject> backgroundProgressBarCellImages = new List<GameObject>();

	protected List<UICraftRequirementWidget> craftRequirementWidgets = new List<UICraftRequirementWidget>();

	[SerializeField]
	protected UICraftSelectionOutputItemCell outputItem;

	protected bool subscribedToDataChanges;

	private readonly HoldRepeatValueChanger craftCountHold = new HoldRepeatValueChanger();

	public CraftDef CraftDef => data.CraftDefinition;

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < 18; i++)
		{
			backgroundProgressBarCellImages.Add(UnityEngine.Object.Instantiate(backgroundProgressBarCellImagePrefab, backgroundProgressBarCellGroupParent));
		}
		backgroundProgressBarCellImagePrefab.gameObject.SetActive(value: false);
	}

	protected override void SetData(UIBaseCraftSelectionWindowData data)
	{
		base.SetData(data);
		onAddToCraftQueuePressed = data.OnCraftToQueueAdded;
		onStartCraftPressed = data.OnCraftStarted;
		onPlusQueuePressed = data.OnPressPlusQueue;
		onMinusQueuePressed = data.OnPressMinusQueue;
	}

	public override void Open(UIBaseCraftSelectionWindowData data)
	{
		base.Open(data);
		SubscribeToDataChanges();
	}

	public override void Hide()
	{
		foreach (UICraftRequirementWidget craftRequirementWidget in craftRequirementWidgets)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(craftRequirementWidget);
		}
		craftRequirementWidgets.Clear();
		HideCells();
		progressCellsInfoWidget.Hide();
		UpdateCraftCountElementsActiveStatus(isActive: false);
		craftCountHold.Reset();
		if (data != null)
		{
			UnsubscribeFromDataChanges();
		}
		base.Hide();
	}

	protected void DrawBaseElements()
	{
		outputItem.Draw(data.CraftDefinition.GetOutputPreview(data.WgoData));
		outputItem.UIItemCell.GamepadNavigationItem.group = 1;
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < data.CraftItemCellsData.Count; i++)
		{
			UICraftItemCell uICraftItemCell = displayedIngredients[i];
			uICraftItemCell.Draw(data.CraftItemCellsData[i], OnNeedItemChange);
			uICraftItemCell.GamepadNavigationItem.group = 1;
		}
		headerLabel.text = LLBase.L(data.CraftDefinition.id);
		UpdateRequirements();
		UpdateProgressBar();
		UpdateProgressChanceBar();
		UpdateCraftCountElements();
		UpdateCraftCountElementsInteractableStatus();
		UpdateButtonsText();
		UpdateButtonsInteractableState();
		UpdateCounters();
	}

	protected virtual void OnStartCraftPressed()
	{
		onStartCraftPressed?.Invoke();
		Close();
	}

	protected void UpdateTalent()
	{
		int talentLock = data.CraftDefinition.talentLock;
		if (talentLock == 0)
		{
			talentOverlap.gameObject.SetActive(value: false);
			return;
		}
		talentOverlap.gameObject.SetActive(value: true);
		bool flag = data.DisplayableWorker.GetMasteryLevelForTalentBranch(data.WgoData.Definition.talent, data.CraftDefinition) >= talentLock;
		talentLabel.text = data.WgoData.Definition.talent.FontIcon() ?? "";
		talentValueLabel.text = talentLock.ToString();
		if (data.CraftDefinition.isStarCraft || data.CraftDefinition.isAutopsyCraft)
		{
			starCraftTalentStyle.ApplyStyle(talentValueLabel);
		}
		else if (flag)
		{
			greenTalentStyle.ApplyStyle(talentValueLabel);
		}
		else
		{
			redTalentStyle.ApplyStyle(talentValueLabel);
		}
	}

	protected bool OnStartCraft()
	{
		if (startCraftButton.interactable)
		{
			OnStartCraftPressed();
			return true;
		}
		return false;
	}

	protected virtual void OnPressPlusQueue()
	{
		ChangeCraftCount(1);
	}

	protected virtual void OnPressMinusQueue()
	{
		ChangeCraftCount(-1);
	}

	protected virtual void ChangeCraftCount(int delta)
	{
		if (delta != 0 && data != null)
		{
			data.AddCraftsCount(delta);
			UpdateCounters();
			UpdateRequirements();
			UpdateCraftCountElementsInteractableStatus();
			UpdateButtonsInteractableState();
		}
	}

	protected override void Update()
	{
		TickCraftCountHold();
		base.Update();
	}

	protected virtual void TickCraftCountHold()
	{
		if (!base.IsShownAndTop || data == null)
		{
			craftCountHold.Reset();
		}
		else
		{
			craftCountHold.Tick(GetCraftCountHoldDirection(), ChangeCraftCount);
		}
	}

	protected virtual int GetCraftCountHoldDirection()
	{
		int pointerHoldDirection = HoldRepeatValueChanger.GetPointerHoldDirection(plusCraftButton, minusCraftButton);
		if (pointerHoldDirection != 0)
		{
			return pointerHoldDirection;
		}
		if (!CanChangeCraftCountWithGamepad())
		{
			return 0;
		}
		bool flag = HoldRepeatValueChanger.IsAnyKeyHeld(GameKey.DpadUp, GameKey.Up) || HoldRepeatValueChanger.GetAxisHoldDirection(vertical: true) > 0;
		bool flag2 = HoldRepeatValueChanger.IsAnyKeyHeld(GameKey.DpadDown, GameKey.Down) || HoldRepeatValueChanger.GetAxisHoldDirection(vertical: true) < 0;
		if (flag == flag2)
		{
			return 0;
		}
		if (flag && (plusCraftButton == null || !plusCraftButton.gameObject.activeSelf || !plusCraftButton.interactable))
		{
			return 0;
		}
		if (flag2 && (minusCraftButton == null || !minusCraftButton.gameObject.activeSelf || !minusCraftButton.interactable))
		{
			return 0;
		}
		if (!flag)
		{
			return -1;
		}
		return 1;
	}

	protected virtual bool CanChangeCraftCountWithGamepad()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null || outputItem == null)
		{
			return false;
		}
		if (plusCraftButton != null && plusCraftButton.gameObject.activeSelf)
		{
			return base.GamepadNavigationController.FocusedItem == outputItem.UIItemCell.GamepadNavigationItem;
		}
		return false;
	}

	protected virtual void UpdateButtonsText()
	{
		startCraftButtonText.text = LLBase.L("ui_craft");
	}

	protected void AddTalentLockText(TextMeshProUGUI label)
	{
		if (data != null && data.CraftDefinition != null && data.DisplayableWorker != null && data.WgoData != null && !(label == null) && !data.CraftDefinition.isStarCraft && !data.CraftDefinition.isAutopsyCraft)
		{
			int masteryLevelForTalentBranch = data.DisplayableWorker.GetMasteryLevelForTalentBranch(data.WgoData.Definition.talent, data.CraftDefinition);
			int talentLock = data.CraftDefinition.talentLock;
			if (talentLock > 0 && masteryLevelForTalentBranch < talentLock)
			{
				string str = talentLock.ToString() ?? "";
				label.text = label.text + " " + data.WgoData.Definition.talent.FontIcon() + " " + redTalentStyleButton.ApplyStyleToString(masteryLevelForTalentBranch.ToString()) + slashTalentStyleButton.ApplyStyleToString("/") + commonTalentStyleButton.ApplyStyleToString(str);
			}
		}
	}

	protected virtual string GetStartCraftGamepadTipKey()
	{
		return "ui_craft";
	}

	protected virtual void UpdateCraftGamepadTips()
	{
		if (!(gamepadTipStartCraft == null) && !(startCraftButton == null))
		{
			gamepadTipStartCraft.text = new LazyGameKeyTip(GameKey.StartCraft, GetStartCraftGamepadTipKey(), startCraftButton.interactable).ToString();
			AddTalentLockText(gamepadTipStartCraft);
		}
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		UpdateCraftGamepadTips();
	}

	protected virtual void UpdateButtonsInteractableState()
	{
		startCraftButton.interactable = data.CanStartCraft;
		UpdateCraftGamepadTips();
	}

	protected virtual bool OnDpadUpPressed()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		if (plusCraftButton.gameObject.activeSelf && plusCraftButton.interactable && base.GamepadNavigationController.FocusedItem == outputItem.UIItemCell.GamepadNavigationItem)
		{
			return true;
		}
		return false;
	}

	protected virtual bool OnDpadDownPressed()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		if (minusCraftButton.gameObject.activeSelf && minusCraftButton.interactable && base.GamepadNavigationController.FocusedItem == outputItem.UIItemCell.GamepadNavigationItem)
		{
			return true;
		}
		return false;
	}

	private void OnNeedItemChange()
	{
		UpdateProgressBar();
		UpdateButtonsInteractableState();
	}

	private void UpdateRequirements()
	{
		if (data.CraftDefinition.isAuto)
		{
			requirementsGo.gameObject.SetActive(value: false);
			return;
		}
		requirementsGo.gameObject.SetActive(value: true);
		for (int i = 0; i < data.CraftRequirementWidgetData.Count; i++)
		{
			UICraftRequirementWidget uICraftRequirementWidget;
			if (i > craftRequirementWidgets.Count - 1)
			{
				uICraftRequirementWidget = UIPrefabsPooler.Instance.GetElementFromPool<UICraftRequirementWidget>(requirementsContainer);
				craftRequirementWidgets.Add(uICraftRequirementWidget);
			}
			else
			{
				uICraftRequirementWidget = craftRequirementWidgets[i];
			}
			uICraftRequirementWidget.gameObject.SetActive(value: true);
			uICraftRequirementWidget.Draw(data.CraftRequirementWidgetData[i]);
		}
	}

	private void UpdateCraftCountElements()
	{
		if (data != null)
		{
			UpdateCraftCountElementsActiveStatus(!data.CraftDefinition.IsMultipleCraftsDisabled);
		}
	}

	protected virtual void UpdateCounters()
	{
		outputItem.UIItemCell.OnMultiplierChange(data.CraftsCount);
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.SetMultiplierValue(data.CraftsCount);
		}
	}

	private void HideCells()
	{
		foreach (GameObject backgroundProgressBarCellImage in backgroundProgressBarCellImages)
		{
			backgroundProgressBarCellImage.gameObject.SetActive(value: false);
		}
		foreach (ProgressCellCraft progressCell in progressCells)
		{
			progressCell.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(progressCell);
		}
		progressCells.Clear();
	}

	private void UpdateProgressBar()
	{
		HideCells();
		int craftStartTicks = data.ParamsData.CraftStartTicks;
		int perksCraftAddTotalProgressTicksValue = data.ParamsData.PerksCraftAddTotalProgressTicksValue;
		Dictionary<PerkDef, int> dictionary = new Dictionary<PerkDef, int>();
		Dictionary<ItemDef, int> dictionary2 = new Dictionary<ItemDef, int>();
		PerkSystemData perkSystemData = MainGame.Instance.GameSave.perkSystemData;
		foreach (string perkId in data.CraftDefinition.linkedPerks)
		{
			int num = perkSystemData.activePerks.FindIndex((PerkData x) => x.id == perkId);
			if (num != -1 && perkSystemData.activePerks[num].Definition.craftStartTicks > 0)
			{
				dictionary.Add(perkSystemData.activePerks[num].Definition, perkSystemData.activePerks[num].Definition.craftStartTicks);
			}
		}
		foreach (NeedItemData currentNeedItem in data.CurrentNeedItems)
		{
			if (!currentNeedItem.IsGroup && currentNeedItem.ItemDef != null && currentNeedItem.ItemDef.qualityType == ItemDef.QualityType.Star && currentNeedItem.ItemDef.quality > 1)
			{
				dictionary2.Add(currentNeedItem.ItemDef, currentNeedItem.ItemDef.quality - 1);
			}
		}
		int num2 = data.CraftDefinition.duration.EvaluateInt() + perksCraftAddTotalProgressTicksValue;
		for (int i = 0; i < Math.Clamp(num2, 0, 18); i++)
		{
			ProgressCellCraft elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<ProgressCellCraft>(progressBarParent);
			int quality = -1;
			int num3 = i + 1;
			if (data.CraftDefinition.isStarCraft)
			{
				if (num3 == data.CraftDefinition.goldLevel)
				{
					quality = 3;
				}
				else if (num3 == data.CraftDefinition.silverLevel)
				{
					quality = 2;
				}
				else if (num3 == data.CraftDefinition.bronzeLevel)
				{
					quality = 1;
				}
			}
			PerkDef perkDef = null;
			using (Dictionary<PerkDef, int>.KeyCollection.Enumerator enumerator3 = dictionary.Keys.GetEnumerator())
			{
				if (enumerator3.MoveNext())
				{
					PerkDef current2 = enumerator3.Current;
					perkDef = current2;
					dictionary[current2]--;
					if (dictionary[current2] == 0)
					{
						dictionary.Remove(current2);
					}
				}
			}
			if (perkDef == null)
			{
				using Dictionary<ItemDef, int>.KeyCollection.Enumerator enumerator4 = dictionary2.Keys.GetEnumerator();
				if (enumerator4.MoveNext())
				{
					ItemDef current3 = enumerator4.Current;
					dictionary2[current3]--;
					if (dictionary2[current3] == 0)
					{
						dictionary2.Remove(current3);
					}
				}
			}
			int autopsyQuality = -1;
			AutopsyTypeCraft autopsyTypeCraft = data.CraftDefinition.autopsyTypeCraft;
			if (autopsyTypeCraft == AutopsyTypeCraft.ExtractOrgan || autopsyTypeCraft == AutopsyTypeCraft.InsertOrgan)
			{
				if (num3 == data.CraftDefinition.goldLevel)
				{
					autopsyQuality = 2;
				}
				else if (num3 == data.CraftDefinition.silverLevel)
				{
					autopsyQuality = 1;
				}
			}
			else if (data.CraftDefinition.autopsyTypeCraft == AutopsyTypeCraft.ChangeOrgan && num3 == data.CraftDefinition.goldLevel)
			{
				autopsyQuality = 2;
			}
			elementFromPool.Show(i, num2, i > craftStartTicks - 1, isFailed: false, quality, autopsyQuality);
			progressCells.Add(elementFromPool);
		}
		for (int j = 0; j < progressCells.Count; j++)
		{
			backgroundProgressBarCellImages[j].gameObject.SetActive(value: true);
		}
		progressBarParent.RefreshContentFitter();
	}

	private void UpdateProgressChanceBar()
	{
		progressCellsInfoWidget.Hide();
		if (!data.CraftDefinition.isAuto)
		{
			int masteryLevelForTalentBranch = data.DisplayableWorker.GetMasteryLevelForTalentBranch(data.WgoData.Definition.talent, data.CraftDefinition);
			int perksCraftMasteryBonusValue = data.DisplayableWorker.GetPerksCraftMasteryBonusValue(data.CraftDefinition);
			bool flag = data.CraftDefinition.isStarCraft || data.CraftDefinition.isAutopsyCraft || data.CraftDefinition.isPocketExtractCraft;
			UIProgressCellsInfoWidgetData uIProgressCellsInfoWidgetData = new UIProgressCellsInfoWidgetData(masteryLevelForTalentBranch, data.CraftDefinition.talentLock, GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent), flag, (flag && masteryLevelForTalentBranch < data.CraftDefinition.talentLock) ? "tt_craft_tick_part" : "tt_craft_tick", perksCraftMasteryBonusValue);
			progressCellsInfoWidget.Draw(uIProgressCellsInfoWidgetData);
		}
	}

	private void UpdateCraftCountElementsActiveStatus(bool isActive)
	{
		plusCraftButton.gameObject.SetActive(isActive);
		minusCraftButton.gameObject.SetActive(isActive);
	}

	protected virtual void UpdateCraftCountElementsInteractableStatus()
	{
		minusCraftButton.interactable = data.CraftsCount > 1;
	}

	protected void AddCraftCountGamepadTips(List<LazyGameKeyTip> tips)
	{
		if (tips != null)
		{
			if (plusCraftButton != null && plusCraftButton.gameObject.activeSelf)
			{
				tips.Add(new LazyGameKeyTip(GameKey.DpadUp, "+", plusCraftButton.interactable));
			}
			if (minusCraftButton != null && minusCraftButton.gameObject.activeSelf)
			{
				tips.Add(new LazyGameKeyTip(GameKey.DpadDown, "-", minusCraftButton.interactable));
			}
		}
	}

	protected virtual void SubscribeToDataChanges()
	{
		if (!subscribedToDataChanges)
		{
			data.SubscribeToDataChanges();
			subscribedToDataChanges = true;
		}
	}

	protected virtual void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChanges)
		{
			data.UnsubscribeFromDataChanges();
			subscribedToDataChanges = false;
		}
	}
}
