using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LazyButton))]
public abstract class UIBaseCraftWidget<T> : LazyWidget<UIBaseCraftWidgetData> where T : UIBaseCraftWidgetData
{
	private Action onPress;

	private Action onOver;

	private Action onOut;

	private Action onPressPlusQueue;

	private Action onPressMinusQueue;

	[SerializeField]
	protected UIItemCell outputItem;

	[SerializeField]
	protected Transform outputItemContainer;

	[SerializeField]
	protected UICraftItemCell itemIngredientPrefab;

	[SerializeField]
	protected Transform ingredientsContainer;

	[SerializeField]
	protected Image selectionFrame;

	[SerializeField]
	private Transform requirementsContainer;

	[SerializeField]
	private GameObject requirementsGo;

	[SerializeField]
	private LazyButton widgetButton;

	[SerializeField]
	private LazyButton addToQueueButton;

	[SerializeField]
	[Space]
	protected LazyButton plusCraftButton;

	[SerializeField]
	protected LazyButton minusCraftButton;

	[SerializeField]
	private UITalentIcon talentIcon;

	[SerializeField]
	private RectTransform progressBarParent;

	[SerializeField]
	private UIProgressCellsInfoWidget progressCellsInfoWidget;

	protected List<UICraftItemCell> displayedIngredients = new List<UICraftItemCell>();

	protected List<ProgressCell> progressCells = new List<ProgressCell>();

	private List<UICraftRequirementWidget> craftRequirementWidgets = new List<UICraftRequirementWidget>();

	private bool subscribedToDataChanges;

	private readonly HoldRepeatValueChanger craftCountHold = new HoldRepeatValueChanger();

	public LazyButton WidgetButton => widgetButton;

	public LazyButton AddToQueueButton => addToQueueButton;

	public CraftDef CraftDef => data.CraftDefinition;

	public override void Init()
	{
		base.Init();
		itemIngredientPrefab.gameObject.SetActive(value: false);
		widgetButton.onEnter.AddListener(OnOver);
		widgetButton.onExit.AddListener(OnOut);
		widgetButton.onClick.AddListener(OnPress);
	}

	public override void DeInit()
	{
		base.DeInit();
		widgetButton.onEnter.RemoveAllListeners();
		widgetButton.onExit.RemoveAllListeners();
		widgetButton.onClick.RemoveAllListeners();
	}

	protected override void SetData(UIBaseCraftWidgetData data)
	{
		base.SetData(data);
		SubscribeToDataChanges();
	}

	public override void Redraw()
	{
		base.Redraw();
		onPress = data.OnPress;
		onOver = data.OnOver;
		onOut = data.OnOut;
		onPressPlusQueue = data.OnPressPlusQueue;
		onPressMinusQueue = data.OnPressMinusQueue;
		outputItem = UIPrefabsPooler.Instance.GetElementFromPool<UIItemCell>(outputItemContainer);
		outputItem.transform.SetParent(outputItemContainer);
		outputItem.DrawCraftOutput(data.CraftDefinition.GetOutputPreview(data.WgoData));
		outputItem.GamepadNavigationItem.group = 1;
		foreach (UICraftItemCellData craftItemCellsDatum in data.CraftItemCellsData)
		{
			UICraftItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftItemCell>(ingredientsContainer.transform);
			elementFromPool.Draw(craftItemCellsDatum, OnNeedItemChange);
			elementFromPool.GamepadNavigationItem.group = 1;
			displayedIngredients.Add(elementFromPool);
		}
		UpdateRequirements();
		UpdateTalenticon();
		UpdateProgressBar();
		UpdateProgressChanceBar();
	}

	public override void Hide()
	{
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.Flush();
			displayedIngredient.GamepadNavigationItem.group = 0;
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedIngredient);
		}
		outputItem.Flush();
		outputItem.GamepadNavigationItem.group = 0;
		UIPrefabsPooler.Instance.ReleaseElementToPool(outputItem);
		foreach (UICraftRequirementWidget craftRequirementWidget in craftRequirementWidgets)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(craftRequirementWidget);
		}
		craftRequirementWidgets.Clear();
		displayedIngredients.Clear();
		HideSelection();
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

	protected virtual void OnNeedItemChange()
	{
		UpdateProgressBar();
	}

	protected void OnPress()
	{
		onPress?.Invoke();
	}

	private void OnOver()
	{
		ShowSelection();
		onOver?.Invoke();
	}

	private void OnOut()
	{
		HideSelection();
		onOut?.Invoke();
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

	private void ShowSelection()
	{
		selectionFrame.gameObject.SetActive(value: true);
		if (!data.CraftDefinition.IsMultipleCraftsDisabled)
		{
			UpdateCraftCountElementsActiveStatus(isActive: true);
		}
	}

	private void HideSelection()
	{
		UpdateCraftCountElementsActiveStatus(isActive: false);
		selectionFrame.gameObject.SetActive(value: false);
	}

	private void UpdateCounters()
	{
		outputItem.OnMultiplierChange(data.CraftsCount);
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.SetMultiplierValue(data.CraftsCount);
		}
	}

	private void HideCells()
	{
		foreach (ProgressCell progressCell in progressCells)
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
		foreach (NeedItemData currentNeedItem in data.GetCurrentNeedItems())
		{
			if (!currentNeedItem.IsGroup && currentNeedItem.ItemDef != null && currentNeedItem.ItemDef.qualityType == ItemDef.QualityType.Star && currentNeedItem.ItemDef.quality > 1)
			{
				dictionary2.Add(currentNeedItem.ItemDef, currentNeedItem.ItemDef.quality - 1);
			}
		}
		int num2 = data.CraftDefinition.duration.EvaluateInt();
		for (int i = 0; i < num2; i++)
		{
			ProgressCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<ProgressCell>(progressBarParent);
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
			ItemDef itemDef = null;
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
					itemDef = current3;
					dictionary2[current3]--;
					if (dictionary2[current3] == 0)
					{
						dictionary2.Remove(current3);
					}
				}
			}
			elementFromPool.Show(i, num2, i > craftStartTicks - 1, isFailed: false, quality, perkDef, itemDef);
			progressCells.Add(elementFromPool);
		}
	}

	private void UpdateTalenticon()
	{
		talentIcon.Draw(GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent), data.CraftDefinition.talentLock, data.DisplayableWorker.GetMasteryLevelForTalentBranch(data.WgoData.Definition.talent, data.CraftDefinition) >= data.CraftDefinition.talentLock, data.CraftDefinition.isStarCraft || data.CraftDefinition.isAutopsyCraft || data.CraftDefinition.isPocketExtractCraft);
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

	private void Update()
	{
		if (data == null)
		{
			craftCountHold.Reset();
		}
		else
		{
			craftCountHold.Tick(HoldRepeatValueChanger.GetPointerHoldDirection(plusCraftButton, minusCraftButton), ChangeCraftCount);
		}
	}

	private void ChangeCraftCount(int delta)
	{
		if (delta != 0 && data != null)
		{
			data.AddCraftsCount(delta);
			UpdateCounters();
			UpdateRequirements();
		}
	}

	private void UpdateCraftCountElementsActiveStatus(bool isActive)
	{
		plusCraftButton.gameObject.SetActive(isActive);
		minusCraftButton.gameObject.SetActive(isActive);
	}

	private void SubscribeToDataChanges()
	{
		if (!subscribedToDataChanges)
		{
			data.SubscribeToDataChanges();
			subscribedToDataChanges = true;
		}
	}

	private void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChanges)
		{
			data.UnsubscribeFromDataChanges();
			subscribedToDataChanges = false;
		}
	}
}
