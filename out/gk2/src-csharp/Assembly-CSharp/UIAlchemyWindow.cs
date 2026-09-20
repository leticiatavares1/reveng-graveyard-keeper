using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAlchemyWindow : LazyWindow<UIAlchemyWindowData>
{
	[Space]
	[SerializeField]
	private UIInfoWidget uiInfoWidget;

	[Space]
	[SerializeField]
	private UIAlchemyIngredient ingredientPrefab;

	[SerializeField]
	private UIItemCell result;

	[SerializeField]
	private GameObject emptyResultObj;

	[SerializeField]
	private GameObject resultUnknown;

	[SerializeField]
	private TextMeshProUGUI fuelResultLabel;

	[SerializeField]
	private TextStyle fuelResultEnoughStyle;

	[SerializeField]
	private TextStyle fuelResultNotEnoughStyle;

	[SerializeField]
	private UITalentIcon talentIconResult;

	[Space]
	[SerializeField]
	private LazyButton createBtn;

	[SerializeField]
	private UIItemCell boostItemCell;

	[SerializeField]
	private GameObject boostEmptyObj;

	[SerializeField]
	private GameObject boostLockedObject;

	[SerializeField]
	private TextMeshProUGUI fuelBoostLabel;

	[SerializeField]
	private LazyButton plusBtn;

	[SerializeField]
	private LazyButton minusBtn;

	[SerializeField]
	private TextMeshProUGUI startTip;

	[SerializeField]
	private GameObject smallFlaskObj;

	[SerializeField]
	private GameObject bigFlaskObj;

	[SerializeField]
	private GameObject[] smallRedFill;

	[SerializeField]
	private GameObject[] smallGreenFill;

	[SerializeField]
	private GameObject[] smallBlueFill;

	[SerializeField]
	private GameObject[] bigRedFill;

	[SerializeField]
	private GameObject[] bigGreenFill;

	[SerializeField]
	private GameObject[] bigBlueFill;

	private List<UIAlchemyIngredient> ingredients = new List<UIAlchemyIngredient>();

	private int craftCount;

	private Vector3Int sumBeforeItemSelect;

	private CraftElementBase currentCraftElement;

	private CraftElement boostCraftElement;

	private string mixCraftId;

	private bool isBigView;

	private readonly HoldRepeatValueChanger craftCountHold = new HoldRepeatValueChanger();

	public override void Init()
	{
		base.Init();
		ingredientPrefab.gameObject.SetActive(value: false);
		createBtn.onClick.AddListener(OnStartMix);
		AttachAlchemyMouseTooltips();
		LazyWindowsStackController.OnWindowClosed += delegate(LazyWidgetBase window)
		{
			if (base.IsShown && window is UICraftWindow)
			{
				RedrawAlchemyTabLite();
			}
		};
	}

	private void AttachAlchemyMouseTooltips()
	{
		UIMouseTooltip.Attach(fuelResultLabel.transform.parent.gameObject, "tt_alchemy_3", null, addRaycastTarget: true);
		UIMouseTooltip.Attach(fuelBoostLabel.transform.parent.gameObject, "tt_alchemy_5", null, addRaycastTarget: true);
		UIMouseTooltip.Attach(smallFlaskObj.transform.parent.gameObject, "tt_alchemy_6", null, addRaycastTarget: true, disableChildRaycasts: true);
		Transform transform = smallFlaskObj.transform.parent.parent.Find("Decor_2");
		if (transform != null)
		{
			UIMouseTooltip.Attach(transform.gameObject, "tt_alchemy_7", null, addRaycastTarget: true, disableChildRaycasts: false, new UIMouseTooltipEdges(0f, 50f));
		}
	}

	public override void Open(UIAlchemyWindowData data)
	{
		base.Open(data);
		UIInfoWidgetData uIInfoWidgetData = new UIInfoWidgetData(data.Wgo.Data);
		uiInfoWidget.Draw(uIInfoWidgetData);
		DisplayAlchemyTab();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	public override void Close()
	{
		ClearPlayerWorker();
		craftCountHold.Reset();
		base.Close();
	}

	protected override void Update()
	{
		TickCraftCountHold();
		base.Update();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.AlchemyStart, OnStartAlchemyPressed);
		gameKeyDelegates.Add(GameKey.PrevTab, OnMinusPressed);
		gameKeyDelegates.Add(GameKey.NextTab, OnPlusPressed);
		return gameKeyDelegates;
	}

	protected override void PrintTips()
	{
		if (data != null)
		{
			List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
			list.Add(LazyGameKeyTip.Select());
			list.Add(LazyGameKeyTip.Back());
			list.Add(new LazyGameKeyTip(GameKey.NextTab, "+", active: true, gamepadOnly: true, translate: false));
			list.Add(new LazyGameKeyTip(GameKey.PrevTab, "-", active: true, gamepadOnly: true, translate: false));
			lazyButtonTips.Print(list);
		}
	}

	private bool OnPlusPressed()
	{
		if (plusBtn == null || !plusBtn.gameObject.activeSelf || !plusBtn.interactable)
		{
			return false;
		}
		craftCountHold.Press(1, ChangeCraftCount);
		return true;
	}

	private bool OnMinusPressed()
	{
		if (minusBtn == null || !minusBtn.gameObject.activeSelf || !minusBtn.interactable)
		{
			return false;
		}
		craftCountHold.Press(-1, ChangeCraftCount);
		return true;
	}

	private bool OnStartAlchemyPressed()
	{
		if (createBtn.interactable)
		{
			OnStartMix();
			return true;
		}
		return false;
	}

	private void DisplayAlchemyTab()
	{
		bool flag = data.Wgo.Data.WorkbenchExtensionsCrafts.Count > 0;
		boostItemCell.gameObject.SetActive(flag);
		boostLockedObject.SetActive(!flag);
		craftCount = 1;
		sumBeforeItemSelect = default(Vector3Int);
		currentCraftElement = null;
		boostCraftElement = null;
		boostItemCell.DrawEmptyInteractable();
		boostEmptyObj.SetActive(value: true);
		boostItemCell.OnItemCellPress = OnBoostBtnClick;
		boostItemCell.OnItemCellPress2 = OnBoostBtnClick2;
		boostItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
		{
			if (boostCraftElement != null)
			{
				UITooltip.ShowAlchemyBoostInfo(cell.transform as RectTransform, boostCraftElement.Definition);
			}
		};
		UpdateBoostFuel();
		foreach (UIAlchemyIngredient ingredient in ingredients)
		{
			ingredient.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < data.Wgo.Data.Definition.inventorySize; i++)
		{
			if (ingredients.Count < i + 1)
			{
				UIAlchemyIngredient item = UnityEngine.Object.Instantiate(ingredientPrefab, ingredientPrefab.transform.parent);
				ingredients.Add(item);
			}
			ingredients[i].gameObject.SetActive(value: true);
			ingredients[i].cell.DrawEmptyInteractable();
			ingredients[i].cell.OnItemCellPress = OnIngredientPressed;
			ingredients[i].cell.OnItemCellPress2 = OnIngredientPressed2;
			ingredients[i].plusObj.SetActive(value: true);
			UIMouseTooltip.Attach(ingredients[i].plusObj, "tt_alchemy_2", null, addRaycastTarget: true);
		}
		isBigView = data.Wgo.Data.Definition.inventorySize > 2;
		if (isBigView)
		{
			bigFlaskObj.SetActive(value: true);
			smallFlaskObj.SetActive(value: false);
		}
		else
		{
			bigFlaskObj.SetActive(value: false);
			smallFlaskObj.SetActive(value: true);
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		RedrawAlchemyTabLite();
	}

	private void RedrawAlchemyTabLite(AlchemyMixDef mix = null)
	{
		UpdateCountInWindow();
		talentIconResult.gameObject.SetActive(value: false);
		List<string> list = new List<string>();
		for (int i = 0; i < ingredients.Count; i++)
		{
			if (ingredients[i].gameObject.activeSelf && ingredients[i].cell.DisplayingItem != null && !ingredients[i].cell.DisplayingItem.IsEmpty)
			{
				list.Add(ingredients[i].cell.DisplayingItem.id);
			}
		}
		if (mix == null)
		{
			mixCraftId = AlchemyMixDef.MixId(list.ToArray(), boostCraftElement?.Definition);
		}
		else
		{
			mixCraftId = mix.id;
		}
		AlchemyMixDef alchemyMixDef = GameBalance.GetAlchemyMixDef(mixCraftId);
		bool flag = alchemyMixDef != null;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = data.Wgo.Data.GetCraftableMultiInventory(excludeWorkerInventory: true).GetTotalCount("alchemy_flask") >= craftCount;
		fuelResultLabel.text = string.Format("{0}{1}", "alchemy_flask".FontIcon(), craftCount);
		if (flag4)
		{
			fuelResultEnoughStyle.ApplyStyle(fuelResultLabel);
		}
		else
		{
			fuelResultNotEnoughStyle.ApplyStyle(fuelResultLabel);
		}
		if (flag)
		{
			flag2 = true;
			flag3 = MainGame.PlayerController.WorkerMultiInventory.HasItemsById(GetNeedItems());
			Debug.Log($"#mix# Has mix with id: {mixCraftId} enoughMastery:[{flag2}] enoughItems:[{flag3}] enoughFlasks:[{flag4}]");
			if (MainGame.Instance.GameSave.knowledgeSystem.IsAlchemyFormulaKnown(alchemyMixDef.Formula))
			{
				result.Draw(new Item(alchemyMixDef.ResultItem.id, craftCount));
				result.ShowMouseSelectionFrame = false;
				result.LazyButton.interactable = true;
				result.GamepadNavigationItem.Active = true;
				result.gameObject.SetActive(value: true);
				resultUnknown.gameObject.SetActive(value: false);
				emptyResultObj.SetActive(value: false);
				talentIconResult.gameObject.SetActive(value: true);
				talentIconResult.Draw(GameBalance.Me.GetData<TalentDef>("talent_blue"), alchemyMixDef.talentLock.ToString());
				UIMouseTooltip component = result.GetComponent<UIMouseTooltip>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
			else
			{
				result.gameObject.SetActive(value: false);
				resultUnknown.gameObject.SetActive(value: true);
				emptyResultObj.SetActive(value: false);
				talentIconResult.gameObject.SetActive(value: true);
				talentIconResult.Draw(GameBalance.Me.GetData<TalentDef>("talent_blue"), alchemyMixDef.talentLock, flag2, isStar: false);
			}
		}
		else
		{
			Debug.Log("#mix# No mix with id: " + mixCraftId);
			result.DrawEmpty(drawAsNonInteractable: true);
			result.NoSelectionFrames = true;
			result.LazyButton.interactable = false;
			result.GamepadNavigationItem.Active = false;
			result.gameObject.SetActive(value: true);
			resultUnknown.gameObject.SetActive(value: false);
			emptyResultObj.SetActive(value: true);
			UIMouseTooltip component2 = result.GetComponent<UIMouseTooltip>();
			if (component2 != null)
			{
				component2.SetLocalizationId(string.Empty);
			}
		}
		Vector3Int vector3Int = CalcSum();
		if (isBigView)
		{
			for (int j = 0; j < bigRedFill.Length; j++)
			{
				bigRedFill[j].SetActive(vector3Int.x > j);
			}
			for (int k = 0; k < bigGreenFill.Length; k++)
			{
				bigGreenFill[k].SetActive(vector3Int.y > k);
			}
			for (int l = 0; l < bigBlueFill.Length; l++)
			{
				bigBlueFill[l].SetActive(vector3Int.z > l);
			}
		}
		else
		{
			for (int m = 0; m < smallRedFill.Length; m++)
			{
				smallRedFill[m].SetActive(vector3Int.x > m);
			}
			for (int n = 0; n < smallGreenFill.Length; n++)
			{
				smallGreenFill[n].SetActive(vector3Int.y > n);
			}
			for (int num = 0; num < smallBlueFill.Length; num++)
			{
				smallBlueFill[num].SetActive(vector3Int.z > num);
			}
		}
		createBtn.interactable = flag && flag2 && flag3 && flag4;
		minusBtn.interactable = craftCount > 1 && flag;
		plusBtn.interactable = flag;
		if (LazyInput.IsGamepadActive)
		{
			startTip.text = new LazyGameKeyTip(GameKey.AlchemyStart, LLBase.L("hint_alchemy"), createBtn.interactable, gamepadOnly: true, translate: false).ToString();
		}
	}

	private void TickCraftCountHold()
	{
		if (!base.IsShownAndTop)
		{
			craftCountHold.Reset();
		}
		else
		{
			craftCountHold.Tick(GetCraftCountHoldDirection(), ChangeCraftCount);
		}
	}

	private int GetCraftCountHoldDirection()
	{
		int pointerHoldDirection = HoldRepeatValueChanger.GetPointerHoldDirection(plusBtn, minusBtn);
		if (pointerHoldDirection != 0)
		{
			return pointerHoldDirection;
		}
		if (!LazyInput.IsGamepadActive)
		{
			return 0;
		}
		bool flag = HoldRepeatValueChanger.IsKeyHeld(GameKey.NextTab) && plusBtn != null && plusBtn.interactable;
		bool flag2 = HoldRepeatValueChanger.IsKeyHeld(GameKey.PrevTab) && minusBtn != null && minusBtn.interactable;
		if (flag == flag2)
		{
			return 0;
		}
		if (!flag)
		{
			return -1;
		}
		return 1;
	}

	private void ChangeCraftCount(int delta)
	{
		if (delta != 0)
		{
			int num = Math.Clamp(craftCount + delta, 1, 999);
			if (num != craftCount)
			{
				craftCount = num;
				RedrawAlchemyTabLite();
			}
		}
	}

	private void OnBoostBtnClick(UIItemCell cell)
	{
		OpenCraftWindow();
	}

	private void OnBoostBtnClick2(UIItemCell cell)
	{
		boostItemCell.DrawEmptyInteractable();
		boostEmptyObj.SetActive(value: true);
		boostItemCell.OnItemCellPress = OnBoostBtnClick;
		boostItemCell.OnItemCellPress2 = OnBoostBtnClick2;
		boostItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
		{
			if (boostCraftElement != null)
			{
				UITooltip.ShowAlchemyBoostInfo(cell.transform as RectTransform, boostCraftElement.Definition);
			}
		};
		boostCraftElement = null;
		RedrawAlchemyTabLite();
		UpdateBoostFuel();
	}

	private void UpdateBoostFuel()
	{
		int num = 0;
		if (boostCraftElement != null)
		{
			NeedItemData needItemData = boostCraftElement.Def.needItems.Find((NeedItemData i) => i.id == "fire");
			if (needItemData != null)
			{
				num = needItemData.GetCount(data.Wgo.Data);
			}
		}
		fuelBoostLabel.text = string.Format("{0}{1}", "fire".FontIcon(), num);
	}

	private void OpenCraftWindow()
	{
		UIAlchemyBoostsWindow boostsWindow = LazyUI.GetWindow<UIAlchemyBoostsWindow>();
		bool wasPlayerSetAsWorker = false;
		if (data.Wgo.Data.Worker == null)
		{
			data.Wgo.Data.TrySetWorker(MainGame.PlayerController);
			wasPlayerSetAsWorker = true;
		}
		List<CraftElement> list2 = new List<CraftElement>();
		foreach (string key in data.Wgo.Data.WorkbenchExtensionsCrafts.Keys)
		{
			foreach (CraftDefBase item in data.Wgo.Data.WorkbenchExtensionsCrafts[key])
			{
				if (!item.id.StartsWith("mix") && item.id.EndsWith("_boost") && item is CraftDef craftDef)
				{
					list2.Add(new CraftElement(craftDef, new CraftParamsData(craftDef.id, data.Wgo.Data)));
				}
			}
		}
		UIAlchemyBoostsWindowData uIAlchemyBoostsWindowData = new UIAlchemyBoostsWindowData(data.Wgo, MainGame.PlayerData, list2, delegate(CraftElement element, List<NeedItemData> list)
		{
			if (element.Definition.id.EndsWith("_boost"))
			{
				boostCraftElement = element;
				boostItemCell.DrawCustom(element.Definition.GetCraftResultIcon(), 1, interactable: true);
				boostEmptyObj.SetActive(value: false);
				boostItemCell.OnItemCellPress = OnBoostBtnClick;
				boostItemCell.OnItemCellPress2 = OnBoostBtnClick2;
				boostItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
				{
					if (boostCraftElement != null)
					{
						UITooltip.ShowAlchemyBoostInfo(cell.transform as RectTransform, boostCraftElement.Definition);
					}
				};
				UpdateBoostFuel();
				boostsWindow.Close();
				RedrawAlchemyTabLite();
			}
		}, (CraftElement craftElement, List<NeedItemData> list) => MainGame.PlayerController.WorkerMultiInventory.HasItemsById(list, data.Wgo.Data));
		boostsWindow.Open(uIAlchemyBoostsWindowData, delegate
		{
			if (wasPlayerSetAsWorker)
			{
				data.Wgo.Data.ClearWorker();
			}
		});
	}

	private void ClearPlayerWorker()
	{
		if (data?.Wgo?.Data?.Worker == MainGame.PlayerController)
		{
			data.Wgo.Data.ClearWorker();
		}
	}

	private Vector3Int CalcSum()
	{
		Vector3Int vector3Int = default(Vector3Int);
		for (int i = 0; i < ingredients.Count; i++)
		{
			if (ingredients[i].gameObject.activeSelf && ingredients[i].cell.DisplayingItem != null && !ingredients[i].cell.DisplayingItem.IsEmpty)
			{
				vector3Int += ingredients[i].cell.DisplayingItem.Definition.GetRunesAsVector3Int();
			}
		}
		if (boostCraftElement != null)
		{
			vector3Int += boostCraftElement.Definition.GetBoostRunesAsVector3Int();
		}
		return vector3Int;
	}

	private void OnIngredientPressed(UIItemCell ingredient)
	{
		sumBeforeItemSelect = default(Vector3Int);
		for (int i = 0; i < ingredients.Count; i++)
		{
			if (ingredients[i] != ingredient.GetComponentInParent<UIAlchemyIngredient>() && ingredients[i].gameObject.activeSelf && ingredients[i].cell.DisplayingItem != null && !ingredients[i].cell.DisplayingItem.IsEmpty)
			{
				sumBeforeItemSelect += ingredients[i].cell.DisplayingItem.Definition.GetRunesAsVector3Int();
			}
		}
		UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
		UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, delegate(UIItemCell uiItemCell)
		{
			craftCount = 1;
			ingredient.Draw(new Item(uiItemCell.DisplayingItem.id), isNeedItem: true, uiItemCell.DisplayingItem.Count);
			ingredient.GetComponentInParent<UIAlchemyIngredient>().plusObj.SetActive(value: false);
			LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
			RedrawAlchemyTabLite();
		}, IsItemValidForMix);
		window.Open(uIMultiInventoryWindowData);
	}

	private void OnIngredientPressed2(UIItemCell ingredient)
	{
		craftCount = 1;
		ingredient.DrawEmptyInteractable();
		ingredient.OnItemCellPress = OnIngredientPressed;
		ingredient.OnItemCellPress2 = OnIngredientPressed2;
		ingredient.GetComponentInParent<UIAlchemyIngredient>().plusObj.SetActive(value: true);
		RedrawAlchemyTabLite();
	}

	private void OnStartMix()
	{
		AlchemyMixDef alchemyMixDef = GameBalance.GetAlchemyMixDef(mixCraftId);
		Debug.Log($"OnStartMix mixCraftId:[{mixCraftId}] mixDef == null:[{alchemyMixDef == null}]");
		if (alchemyMixDef == null)
		{
			Debug.LogError("#alch# Can't start mix, mixDef is missing. mixCraftId:[" + mixCraftId + "]");
			return;
		}
		CraftDef alchemyWorkBenchCraft = alchemyMixDef.AlchemyWorkBenchCraft;
		AlchemyFormulaDef formula = alchemyMixDef.Formula;
		if (alchemyWorkBenchCraft == null || formula == null)
		{
			Debug.LogError($"#alch# Can't start mix [{mixCraftId}], invalid related defs. AlchemyWorkBenchCraft null:[{alchemyWorkBenchCraft == null}] Formula null:[{formula == null}]");
			return;
		}
		if (boostCraftElement != null && data.Wgo.Data.CraftComponent.TryStartCraft(boostCraftElement))
		{
			data.Wgo.Data.CraftComponent.TryFinishCurCraft();
		}
		currentCraftElement = new CraftElementMix(alchemyMixDef, new CraftParamsData(mixCraftId, data.Wgo.Data));
		currentCraftElement.Count = craftCount;
		MainGame.PlayerController.WorkerMultiInventory.RemoveItems(GetNeedItems());
		data.Wgo.Data.GetCraftableMultiInventory(excludeWorkerInventory: true).RemoveItem(new Item("alchemy_flask", craftCount));
		if (data.Wgo.Data.CraftComponent.TryStartCraft(currentCraftElement))
		{
			data.Wgo.Data.ClearWorker();
			Close();
		}
	}

	private List<NeedItemData> GetNeedItems()
	{
		AlchemyMixDef alchemyMixDef = GameBalance.GetAlchemyMixDef(mixCraftId);
		List<NeedItemData> list = new List<NeedItemData>();
		for (int i = 0; i < alchemyMixDef.ingredients.Length; i++)
		{
			list.Add(new NeedItemData(alchemyMixDef.ingredients[i], craftCount));
		}
		return list;
	}

	private bool IsItemValidForMix(Item ingredient)
	{
		if (ingredient == null)
		{
			return false;
		}
		if (ingredient.IsEmpty)
		{
			return false;
		}
		if (!ingredient.Definition.canBeUsedInAlchemy)
		{
			return false;
		}
		Vector3Int runesAsVector3Int = ingredient.Definition.GetRunesAsVector3Int();
		SurveyDef surveyDefForItemOrNull = GameBalance.GetSurveyDefForItemOrNull(ingredient.Definition.id);
		if (surveyDefForItemOrNull == null && runesAsVector3Int != Vector3Int.zero)
		{
			return false;
		}
		if (surveyDefForItemOrNull != null && !MainGame.Instance.GameSave.knowledgeSystem.IsSurveyCompleted(surveyDefForItemOrNull))
		{
			return false;
		}
		Vector3Int vector3Int = runesAsVector3Int + sumBeforeItemSelect;
		if (vector3Int.x > 5 || vector3Int.y > 5 || vector3Int.z > 5)
		{
			return false;
		}
		if (vector3Int.x < 0 || vector3Int.y < 0 || vector3Int.z < 0)
		{
			return false;
		}
		return true;
	}

	private void UpdateCountInWindow()
	{
		foreach (UIAlchemyIngredient ingredient in ingredients)
		{
			if (!ingredient.gameObject.gameObject.activeSelf || ingredient.cell.DisplayingItem == null || ingredient.cell.DisplayingItem.IsEmpty)
			{
				continue;
			}
			int num = 1;
			foreach (UIAlchemyIngredient ingredient2 in ingredients)
			{
				if (!(ingredient2 == ingredient) && ingredient.gameObject.gameObject.activeSelf && ingredient.cell.DisplayingItem != null && !ingredient.cell.DisplayingItem.IsEmpty && ingredient2.gameObject.gameObject.activeSelf && ingredient2.cell.DisplayingItem != null && !ingredient2.cell.DisplayingItem.IsEmpty && ingredient2.cell.DisplayingItem.id == ingredient.cell.DisplayingItem.id)
				{
					num++;
				}
			}
			ingredient.cell.OnMultiplierChange(craftCount * num);
		}
		if (result.DisplayingItem != null && !result.DisplayingItem.IsEmpty)
		{
			result.OnMultiplierChange(craftCount);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		WgoData wgoData = new WgoData("alchemy_mix", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		wgoData.Inventory.AddItemToInventory(new Item("alchemy_flask", 5));
		AlchemyInteractionHandler alchemyInteractionHandler = new AlchemyInteractionHandler();
		alchemyInteractionHandler.Init(wgo);
		alchemyInteractionHandler.HasInteraction(MainGame.PlayerController);
		alchemyInteractionHandler.Interact(MainGame.PlayerController);
	}

	[LazyUITest]
	protected void TestDrawFull()
	{
		WgoData wgoData = new WgoData("alchemy_mix_2", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		Wgo wgo = Wgo.Spawn(wgoData, MainGame.PlayerController.CurrentGameScene.transform);
		wgoData.Inventory.AddItemToInventory(new Item("alchemy_flask", 10));
		AlchemyInteractionHandler alchemyInteractionHandler = new AlchemyInteractionHandler();
		alchemyInteractionHandler.Init(wgo);
		alchemyInteractionHandler.HasInteraction(MainGame.PlayerController);
		alchemyInteractionHandler.Interact(MainGame.PlayerController);
	}
}
