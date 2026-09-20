using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGardenBedWindow : LazyWindow<UIGardenBedWindowData>
{
	[SerializeField]
	private UIInfoWidget uiInfoWidget;

	[SerializeField]
	private Image customBedImage;

	[SerializeField]
	private Image customBedImageGamepad;

	[SerializeField]
	private UIGardenBedSlot inputItem;

	[SerializeField]
	private UIItemCell outputItem;

	[SerializeField]
	private Transform progressBarParent;

	[SerializeField]
	private TextMeshProUGUI tickDuration;

	[SerializeField]
	private TextMeshProUGUI resultLabel;

	[SerializeField]
	private TextMeshProUGUI fertilizingLabel;

	[SerializeField]
	private TextMeshProUGUI masteryRequirementLabel;

	[SerializeField]
	private UIProgressCellsInfoWidget progressCellsInfoWidget;

	[SerializeField]
	private Color toReplace;

	[SerializeField]
	private RectTransform slotsPos1;

	[SerializeField]
	private RectTransform slotsPos2;

	[SerializeField]
	private RectTransform slotsObj;

	[SerializeField]
	private GameObject craftProgressObj;

	[SerializeField]
	private UIItemCell seedItemCell;

	[SerializeField]
	private UIDialogWindowButton plantButton;

	[SerializeField]
	private GameObject emptyBedBackObj;

	[SerializeField]
	private GameObject growingBedBackObj;

	[SerializeField]
	private GameObject emptyPlantCellObj;

	[SerializeField]
	[Space]
	private Image bedIcon;

	[SerializeField]
	private UITalentIcon bedTalent;

	[SerializeField]
	private UIWorkerIcon workerIcon;

	[SerializeField]
	private UIWorkerIcon workerIcon2;

	[SerializeField]
	private Image workerBack;

	[SerializeField]
	private Image workerBack2;

	[SerializeField]
	private Sprite workerBackCommon;

	[SerializeField]
	private Sprite workerBackCommon2;

	[SerializeField]
	private Sprite workerBackGrowing;

	[SerializeField]
	private Sprite workerBackGrowing2;

	[SerializeField]
	private GameObject arrowDecor;

	[SerializeField]
	private GameObject leftDecor;

	[SerializeField]
	private List<UIGardenBedSlot> perkWidgets = new List<UIGardenBedSlot>();

	private List<ProgressCellCraft> progressCells = new List<ProgressCellCraft>();

	private bool isSubscribedToDataChanges;

	private bool customBedImageShown;

	private Item selectedSeed;

	private CraftDefBase selectedSeedCraft;

	public override void Init()
	{
		base.Init();
		AttachGarden3Tooltip();
		UIMouseTooltip.Attach(masteryRequirementLabel.gameObject, "tt_garden_6", null, addRaycastTarget: true, disableChildRaycasts: false, new UIMouseTooltipEdges(0f, 0f, -3f, -3f), new Vector2(16f, -2f));
	}

	protected override void SetData(UIGardenBedWindowData data)
	{
		base.SetData(data);
		SubscribeToDataChanges();
	}

	public override void Redraw()
	{
		base.Redraw();
		UpdateCraftOutput();
		uiInfoWidget.Draw(data.UIInfoWidgetData);
		if (data.IsGrowing)
		{
			selectedSeed = null;
			selectedSeedCraft = null;
			slotsObj.anchoredPosition = slotsPos1.anchoredPosition;
			workerBack.sprite = workerBackGrowing;
			workerBack2.sprite = workerBackGrowing2;
			customBedImage.sprite = uiInfoWidget.Icon;
			customBedImageGamepad.sprite = uiInfoWidget.Icon;
			customBedImageShown = true;
			if (LazyInput.IsGamepadActive)
			{
				customBedImageGamepad.gameObject.SetActive(value: true);
				customBedImage.gameObject.SetActive(value: false);
			}
			else
			{
				customBedImage.gameObject.SetActive(value: true);
				customBedImageGamepad.gameObject.SetActive(value: false);
			}
			arrowDecor.SetActive(value: true);
			leftDecor.SetActive(value: true);
			craftProgressObj.SetActive(value: true);
			workerIcon.WorkerIconParent.gameObject.SetActive(value: false);
			workerIcon2.WorkerIconParent.gameObject.SetActive(value: false);
		}
		else
		{
			craftProgressObj.SetActive(value: false);
			slotsObj.anchoredPosition = slotsPos2.anchoredPosition;
			workerBack.sprite = workerBackCommon;
			workerBack2.sprite = workerBackCommon2;
			arrowDecor.SetActive(value: false);
			leftDecor.SetActive(value: false);
			customBedImage.gameObject.SetActive(value: false);
			customBedImageGamepad.gameObject.SetActive(value: false);
			customBedImageShown = false;
			workerIcon.WorkerIconParent.gameObject.SetActive(value: true);
			workerIcon2.WorkerIconParent.gameObject.SetActive(value: true);
		}
		UpdateProgressBar();
		UpdateCraftLabels();
		UpdateFertilizerLabel();
		UpdateInputItem();
		UpdatePlantControls();
		UpdatePerks();
		UpdateBedTalentIcon();
		UpdateProgressChanceBar();
		UpdateTickDuration();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (customBedImageShown)
		{
			if (LazyInput.IsGamepadActive)
			{
				customBedImageGamepad.gameObject.SetActive(value: true);
				customBedImage.gameObject.SetActive(value: false);
			}
			else
			{
				customBedImage.gameObject.SetActive(value: true);
				customBedImageGamepad.gameObject.SetActive(value: false);
			}
		}
	}

	public override void Hide()
	{
		base.Hide();
		HideCells();
		UnSubscribeToDataChanges();
	}

	private void HideCells()
	{
		foreach (ProgressCellCraft progressCell in progressCells)
		{
			progressCell.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(progressCell);
		}
		progressCells.Clear();
	}

	private void UpdateProgressBar()
	{
		if (!data.IsGrowing)
		{
			craftProgressObj.SetActive(value: false);
			return;
		}
		int totalProgressTicks = data.CraftElement.TotalProgressTicks;
		int succeededProgressTicks = data.CraftElement.SucceededProgressTicks;
		int num = data.CraftElement.TotalProgressTicks - data.CraftElement.FailedProgressTicks;
		if (data.CraftElement.ProgressTicks >= totalProgressTicks)
		{
			Close();
			return;
		}
		HideCells();
		for (int i = 0; i < totalProgressTicks; i++)
		{
			ProgressCellCraft elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<ProgressCellCraft>(progressBarParent);
			elementFromPool.Show(i, totalProgressTicks, i >= succeededProgressTicks && i < num, i >= num, -1, -1, data.CraftDefinition.GetQualityForGardenProgressTick(i + 1));
			progressCells.Add(elementFromPool);
		}
		UIMouseTooltip.Attach(progressBarParent.gameObject, "tt_garden_7", null, addRaycastTarget: true, disableChildRaycasts: true, UIMouseTooltipEdges.All(-3f));
		for (int j = 0; j < progressCells.Count; j++)
		{
			if (progressCells[j].PlusOneObject != null && progressCells[j].PlusOneObject.activeSelf)
			{
				UIMouseTooltip.Attach(progressCells[j].PlusOneObject, "tt_garden_4", null, addRaycastTarget: true, disableChildRaycasts: true);
			}
		}
		((RectTransform)progressBarParent.transform).RefreshContentFitter();
	}

	private void UpdateCraftLabels()
	{
		if (!data.IsGrowing)
		{
			resultLabel.text = string.Empty;
			masteryRequirementLabel.text = string.Empty;
		}
		else
		{
			resultLabel.text = LLBase.L(data.CraftDefinition.GetOutputPreview(data.WgoData).itemId);
			masteryRequirementLabel.text = data.WgoData.Definition.talent.FontIcon() + "<space=2px>" + data.CraftDefinition.talentLock;
		}
	}

	private void UpdateFertilizerLabel()
	{
		fertilizingLabel.gameObject.SetActive(!data.IsGrowing);
	}

	private void UpdateTickDuration()
	{
		if (data.IsGrowing)
		{
			tickDuration.text = "";
			TimeSpan timeSpan = TimeSpan.FromSeconds(data.WgoData.Definition.autocraftTickDuration.EvaluateFloat(data.WgoData));
			tickDuration.text = "cell_time".FontIcon() + timeSpan.ToString("m\\:ss");
			uiInfoWidget.TurnOnTickDuration();
		}
	}

	private void UpdateInputItem()
	{
		if (!data.IsGrowing)
		{
			inputItem.gameObject.SetActive(value: false);
			craftProgressObj.SetActive(value: false);
			return;
		}
		List<ItemDef> list = new List<ItemDef>();
		foreach (ChanceOutputItem chanceOutputItem in data.CraftDefinition.addItemsToWgoOnFinish.chanceOutputItems)
		{
			ItemDef itemDef = GameBalance.Me.GetData<ItemDef>(chanceOutputItem.id);
			if (itemDef.itemGroupIds.Contains("seed"))
			{
				list.Add(itemDef);
			}
		}
		if (list.Count < 0)
		{
			Debug.LogError("No seed item for growing craft:[" + data.CraftDefinition.id + "]");
			return;
		}
		int num = int.MaxValue;
		ItemDef itemDef2 = null;
		for (int i = 0; i < list.Count; i++)
		{
			ItemDef itemDef3 = list[i];
			if (itemDef3.quality < num)
			{
				itemDef2 = itemDef3;
				num = itemDef3.quality;
			}
		}
		if (itemDef2 == null)
		{
			Debug.LogError("No valid seed item for growing craft:[" + data.CraftDefinition.id + "]");
			inputItem.gameObject.SetActive(value: false);
		}
		else
		{
			inputItem.DrawSeedSlot(new Item(itemDef2.id));
			inputItem.gameObject.SetActive(value: true);
		}
	}

	private void UpdateBedTalentIcon()
	{
		CraftParamsData.GardenType gardenTypeFromWgo = GetGardenTypeFromWgo(data.WgoData);
		Debug.Log($"Garden type: {gardenTypeFromWgo}");
		int num = ((gardenTypeFromWgo != CraftParamsData.GardenType.Vineyard) ? MainGame.PlayerData.GetResInt("g_garden_farming_base") : MainGame.PlayerData.GetResInt("g_vineyard_farming_base"));
		Image image = bedIcon;
		EasySpritesCollection instance = LazySingletonSO<EasySpritesCollection>.Instance;
		image.sprite = instance.GetSprite("i_b_garden_bed_" + ((gardenTypeFromWgo != CraftParamsData.GardenType.Vineyard) ? MainGame.PlayerData.GetResInt("g_garden_lvl") : MainGame.PlayerData.GetResInt("g_vineyard_lvl")), "i_placeholder");
		bedTalent.Draw(GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent), num, isEnoughMastery: true, isStar: false);
		AttachGarden3Tooltip();
		bedIcon.BlueColorReplace(toReplace);
		if (data.IsGrowing)
		{
			workerIcon.SetTalentValue(GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent), num + GetFertilizersMasteryBonus());
			workerIcon2.SetTalentValue(GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent), num + GetFertilizersMasteryBonus());
		}
	}

	private int GetFertilizersMasteryBonus()
	{
		int num = 0;
		foreach (UIGardenBedSlot perkWidget in perkWidgets)
		{
			if (perkWidget.PerkData != null)
			{
				num += perkWidget.PerkData.Definition.craftMasteryBonus;
			}
		}
		return num;
	}

	private void AttachGarden3Tooltip()
	{
		if (!(bedTalent == null))
		{
			RectTransform rectTransform = (RectTransform)bedTalent.transform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			RectTransform orCreateOverlay = UIMouseTooltip.GetOrCreateOverlay(rectTransform, "HoverArea");
			UIMouseTooltip.FitOverlayToPreferredSize(orCreateOverlay, rectTransform);
			orCreateOverlay.SetAsLastSibling();
			UIMouseTooltip.Attach(orCreateOverlay.gameObject, "tt_garden_3", null, addRaycastTarget: true);
		}
	}

	private static CraftParamsData.GardenType GetGardenTypeFromWgo(WgoData wgoData)
	{
		if (wgoData.Definition.wgoGroup.Contains("vineyard_objects"))
		{
			return CraftParamsData.GardenType.Vineyard;
		}
		return CraftParamsData.GardenType.None;
	}

	private void UpdateProgressChanceBar()
	{
		if (!data.IsGrowing)
		{
			craftProgressObj.SetActive(value: false);
			return;
		}
		progressCellsInfoWidget.Hide();
		UIProgressCellsInfoWidgetData uIProgressCellsInfoWidgetData = new UIProgressCellsInfoWidgetData(data.CraftElement.ParamsData.MasteryValue, data.CraftDefinition.talentLock, GameBalance.Me.GetData<TalentDef>(data.WgoData.Definition.talent), isStarCraft: true, (data.CraftElement.ParamsData.MasteryValue < data.CraftDefinition.talentLock) ? "tt_garden_5_part" : "tt_garden_5");
		progressCellsInfoWidget.Draw(uIProgressCellsInfoWidgetData);
	}

	private void SubscribeToDataChanges()
	{
		if (!isSubscribedToDataChanges && data.IsGrowing)
		{
			data.CraftElement.OnProgressChanged += UpdateProgressBar;
			data.CraftElement.OnProgressChanged += UpdateCraftOutput;
			isSubscribedToDataChanges = true;
		}
	}

	private void UnSubscribeToDataChanges()
	{
		if (isSubscribedToDataChanges && data.IsGrowing)
		{
			data.CraftElement.OnProgressChanged -= UpdateProgressBar;
			data.CraftElement.OnProgressChanged -= UpdateCraftOutput;
			isSubscribedToDataChanges = false;
		}
	}

	private void UpdateCraftOutput()
	{
		if (!data.IsGrowing)
		{
			craftProgressObj.SetActive(value: false);
			outputItem.gameObject.SetActive(value: false);
			return;
		}
		OutputPreview outputPreview = data.CraftDefinition.GetOutputPreview(data.WgoData);
		outputItem.Draw(new Item(outputPreview.itemId));
		outputItem.ShowMouseSelectionFrame = false;
		outputItem.TooltipPlacementPriority = TooltipPlacementPriority.BottomRight;
		outputItem.gameObject.SetActive(value: true);
		outputItem.GamepadNavigationItem.Active = true;
	}

	private void UpdatePerks()
	{
		int resInt = MainGame.PlayerData.GetResInt("g_garden_fertilizer_slots");
		for (int i = 0; i < perkWidgets.Count; i++)
		{
			if (resInt > i)
			{
				if (HasPerkForSlot(i, out var slotPerk))
				{
					perkWidgets[i].DrawFertilizerSlot(slotPerk, TryApplyFertilizer, !data.IsGrowing);
				}
				else
				{
					perkWidgets[i].DrawEmpty(TryApplyFertilizer, !data.IsGrowing);
				}
			}
			else
			{
				perkWidgets[i].DrawLocked();
			}
			perkWidgets[i].slotIndex = i;
		}
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Print(LazyGameKeyTip.Back());
	}

	private bool HasPerkForSlot(int uiSlotIndex, out PerkData slotPerk)
	{
		slotPerk = null;
		for (int i = 0; i < data.GardenPerks.Count; i++)
		{
			int gameResInt = data.WgoData.GetGameResInt("perk_fertilize_" + data.GardenPerks[i].Definition.id);
			if (gameResInt > 0 && gameResInt - 1 == uiSlotIndex)
			{
				slotPerk = data.GardenPerks[i];
				return true;
			}
		}
		return false;
	}

	private void AddFertilizerPerk(UIItemCell cell, UIGardenBedSlot fertilizerSlot)
	{
		if (fertilizerSlot.PerkData != null)
		{
			RemoveFertilizerPerk(fertilizerSlot.PerkData);
		}
		CraftDefBase craftDefBase = GardenInteractionHandler.TryFindGardenCraft(cell.DisplayingItem, data.WgoData);
		if (craftDefBase == null)
		{
			EndFertilizerApply();
			return;
		}
		data.TrySetWorker();
		if (GardenInteractionHandler.TryApplyFertilizer(cell.DisplayingItem, craftDefBase, data.WgoData))
		{
			foreach (PerkData activePerk in data.WgoData.ActivePerks)
			{
				if (!data.GardenPerks.Contains(activePerk))
				{
					data.WgoData.SetGameRes("perk_fertilize_" + activePerk.Definition.id, fertilizerSlot.slotIndex + 1);
				}
			}
			EndFertilizerApply();
		}
		data.TryRemoveWorker();
	}

	private void RemoveFertilizerPerk(PerkData perk)
	{
		data.WgoData.RemovePerk(perk);
	}

	private void TryApplyFertilizer(UIGardenBedSlot gardenFertilizerSlot)
	{
		if (data.CraftElement == null)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, delegate(UIItemCell x)
			{
				AddFertilizerPerk(x, gardenFertilizerSlot);
			}, IsItemValid);
			window.Open(uIMultiInventoryWindowData);
		}
	}

	private void EndFertilizerApply()
	{
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		data.TryRemoveWorker();
		data.UpdateData();
		Redraw();
	}

	private bool IsItemValid(Item item)
	{
		if (item == null || !item.IsFertilizer)
		{
			return false;
		}
		foreach (PerkData activePerk in data.WgoData.ActivePerks)
		{
			string fertilizerItemId = activePerk.Definition.fertilizerItemId;
			if (!string.IsNullOrEmpty(fertilizerItemId))
			{
				ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(fertilizerItemId);
				if (dataOrNull != null && dataOrNull.isFertilizer && dataOrNull.id == item.id)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void UpdatePlantControls()
	{
		SetOptionalActive(emptyBedBackObj, !data.IsGrowing);
		SetOptionalActive(growingBedBackObj, data.IsGrowing);
		if (seedItemCell == null && plantButton == null)
		{
			return;
		}
		bool flag = !data.IsGrowing;
		SetOptionalActive(emptyPlantCellObj, flag && (selectedSeed == null || selectedSeed.IsEmpty));
		if (seedItemCell != null)
		{
			seedItemCell.gameObject.SetActive(flag);
			if (flag)
			{
				UpdateSeedItemCell();
			}
		}
		if (plantButton != null)
		{
			plantButton.gameObject.SetActive(flag);
			if (flag)
			{
				plantButton.Draw(new UIDialogWindowData.ButtonData(OnPlantButtonPress, LLBase.L("ui_plant"), CanPlantSelectedSeed, replaceForGamepad: true, GameKey.Fold));
			}
		}
	}

	private void UpdateSeedItemCell()
	{
		if (selectedSeed == null || selectedSeed.IsEmpty)
		{
			seedItemCell.DrawEmptyInteractable();
			seedItemCell.OnItemCellPress = OnSeedItemCellPress;
			return;
		}
		int selectedSeedNeedCount = GetSelectedSeedNeedCount();
		int availableSeedCount = GetAvailableSeedCount(selectedSeed.id);
		seedItemCell.Draw(new Item(selectedSeed.id, selectedSeedNeedCount), isNeedItem: true, availableSeedCount);
		seedItemCell.OnItemCellPress = OnSeedItemCellPress;
		seedItemCell.OnItemCellPress2 = OnSeedItemCellPress2;
		seedItemCell.TooltipPlacementPriority = TooltipPlacementPriority.TopRight;
	}

	private void OnSeedItemCellPress(UIItemCell cell)
	{
		UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
		UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, OnSeedSelected, IsSeedValid);
		window.Open(uIMultiInventoryWindowData);
	}

	private void OnSeedItemCellPress2(UIItemCell cell)
	{
		selectedSeed = null;
		selectedSeedCraft = null;
		UpdatePlantControls();
	}

	private void OnSeedSelected(UIItemCell cell)
	{
		selectedSeed = new Item(cell.DisplayingItem.id);
		selectedSeedCraft = GardenInteractionHandler.TryFindGardenCraft(selectedSeed, data.WgoData);
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		UpdatePlantControls();
	}

	private bool IsSeedValid(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		if (!GardenInteractionHandler.IsSeedableSeed(data.WgoData.id, item))
		{
			return false;
		}
		CraftDefBase craftDefBase = GardenInteractionHandler.TryFindGardenCraft(item, data.WgoData, logWarning: false);
		if (craftDefBase != null)
		{
			return craftDefBase.needItems.Count > 0;
		}
		return false;
	}

	private bool CanPlantSelectedSeed()
	{
		if (data.IsGrowing || selectedSeed == null || selectedSeed.IsEmpty)
		{
			return false;
		}
		if (selectedSeedCraft == null)
		{
			selectedSeedCraft = GardenInteractionHandler.TryFindGardenCraft(selectedSeed, data.WgoData);
		}
		if (selectedSeedCraft == null || selectedSeedCraft.needItems.Count == 0)
		{
			return false;
		}
		return GetAvailableSeedCount(selectedSeed.id) >= GetSelectedSeedNeedCount();
	}

	private int GetSelectedSeedNeedCount()
	{
		if (selectedSeedCraft == null || selectedSeedCraft.needItems.Count == 0)
		{
			return 0;
		}
		return selectedSeedCraft.needItems[0].GetCount(data.WgoData);
	}

	private int GetAvailableSeedCount(string itemId)
	{
		return new MultiInventory(MainGame.PlayerData).GetTotalCount(itemId);
	}

	private void OnPlantButtonPress()
	{
		if (!CanPlantSelectedSeed())
		{
			return;
		}
		if (MainGame.PlayerController.GetMasteryLevelForTalentBranch("talent_green") <= 0)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "gardening_no_mastery", null, null, SpeechBubbleType.Think));
			return;
		}
		data.TrySetWorker();
		data.WgoData.CraftComponent.Clear();
		bool num = GardenInteractionHandler.TryApplySeed(selectedSeed, selectedSeedCraft, data.WgoData);
		data.TryRemoveWorker();
		if (!num)
		{
			UpdatePlantControls();
			return;
		}
		LazyAudio.Play("planting");
		Close();
	}

	private static void SetOptionalActive(GameObject obj, bool value)
	{
		if (obj != null)
		{
			obj.SetActive(value);
		}
	}

	protected override void TestDraw()
	{
	}
}
