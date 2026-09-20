using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine.UI;

public class UIResourceBasedCraftWindowData : LazyWidgetDataBase
{
	private Action onUpdateDataAction;

	private List<string> allowedItemIds;

	private CraftElementBase currentCraftElement;

	private SurveyDef currentSurveyDef;

	public Item SelectedItem { get; private set; }

	public List<NeedItemData> Ingredients { get; private set; }

	public List<int> IngredientsHasCount { get; private set; }

	public int CraftNeedItemsCount { get; private set; }

	public Item FuelItem { get; private set; }

	public string BtnText { get; private set; }

	public string LabelText { get; private set; }

	public Action OnBtnPressedAction { get; private set; }

	public Action<UIItemCell> OnMainIngredientPressedAction { get; private set; }

	public WgoData WgoData { get; set; }

	public int MainIngredientCount { get; private set; }

	public UIResourceBasedCraftWindowData(WgoData wgoData)
	{
		UIResourceBasedCraftWindowData uIResourceBasedCraftWindowData = this;
		WgoData = wgoData;
		allowedItemIds = new List<string>();
		for (int i = 0; i < GameBalance.Me.surveyDefs.Count; i++)
		{
			SurveyDef surveyDef = GameBalance.Me.surveyDefs[i];
			if (surveyDef.isOneTimeCraft && MainGame.Instance.GameSave.knowledgeSystem.IsSurveyCompleted(surveyDef))
			{
				continue;
			}
			foreach (ItemDef surveyedItemDef in surveyDef.GetSurveyedItemDefs())
			{
				allowedItemIds.Add(surveyedItemDef.id);
			}
		}
		OnBtnPressedAction = OnStartSurvey;
		onUpdateDataAction = OnUpdateData;
		OnMainIngredientPressedAction = OnResourcePickerPressed;
		OnUpdateData();
		void OnUpdateData()
		{
			uIResourceBasedCraftWindowData.Ingredients = new List<NeedItemData>();
			uIResourceBasedCraftWindowData.IngredientsHasCount = new List<int>();
			uIResourceBasedCraftWindowData.CraftNeedItemsCount = 0;
			MultiInventory multiInventory = new MultiInventory(MainGame.PlayerData);
			uIResourceBasedCraftWindowData.FuelItem = new Item("science", wgoData.Inventory.Data.GetTotalCountInInventory("science"));
			if (uIResourceBasedCraftWindowData.SelectedItem != null)
			{
				if (multiInventory.GetTotalCount(uIResourceBasedCraftWindowData.SelectedItem.id) <= 0)
				{
					uIResourceBasedCraftWindowData.SelectedItem = null;
				}
				else
				{
					uIResourceBasedCraftWindowData.currentSurveyDef = GameBalance.GetSurveyDefForItemOrNull(uIResourceBasedCraftWindowData.SelectedItem.id);
					if (uIResourceBasedCraftWindowData.currentSurveyDef == null || MainGame.Instance.GameSave.knowledgeSystem.IsSurveyCompleted(uIResourceBasedCraftWindowData.currentSurveyDef))
					{
						uIResourceBasedCraftWindowData.SelectedItem = null;
						uIResourceBasedCraftWindowData.currentCraftElement = null;
					}
					else
					{
						uIResourceBasedCraftWindowData.currentCraftElement = uIResourceBasedCraftWindowData.CreateCraftElement(uIResourceBasedCraftWindowData.SelectedItem, uIResourceBasedCraftWindowData.currentSurveyDef, wgoData);
						uIResourceBasedCraftWindowData.MainIngredientCount = multiInventory.GetTotalCount(uIResourceBasedCraftWindowData.SelectedItem.id);
					}
				}
			}
			uIResourceBasedCraftWindowData.BtnText = LLBase.L("hint_survey");
			if (uIResourceBasedCraftWindowData.SelectedItem == null)
			{
				uIResourceBasedCraftWindowData.LabelText = LLBase.L("craft_pick_res_hint_surv");
			}
			else
			{
				if (uIResourceBasedCraftWindowData.currentSurveyDef.isScienceFuelCraft)
				{
					uIResourceBasedCraftWindowData.BtnText = LLBase.L("btn_science_decompose");
					uIResourceBasedCraftWindowData.LabelText = LLBase.L("science_decompose", "+" + "science".FontIcon() + uIResourceBasedCraftWindowData.currentSurveyDef.outputItems.chanceOutputItems[0].count.EvaluateInt());
				}
				else
				{
					uIResourceBasedCraftWindowData.LabelText = LLBase.L("ui_survey_requirements");
				}
				for (int j = 1; j < uIResourceBasedCraftWindowData.currentSurveyDef.needItems.Count; j++)
				{
					uIResourceBasedCraftWindowData.Ingredients.Add(uIResourceBasedCraftWindowData.currentSurveyDef.needItems[j]);
					uIResourceBasedCraftWindowData.IngredientsHasCount.Add(multiInventory.GetTotalCount(uIResourceBasedCraftWindowData.currentSurveyDef.needItems[j].id));
					int craftNeedItemsCount = uIResourceBasedCraftWindowData.CraftNeedItemsCount;
					uIResourceBasedCraftWindowData.CraftNeedItemsCount = craftNeedItemsCount + 1;
				}
				for (int k = 0; k < uIResourceBasedCraftWindowData.currentSurveyDef.needItemsFromWgo.Count; k++)
				{
					uIResourceBasedCraftWindowData.Ingredients.Add(uIResourceBasedCraftWindowData.currentSurveyDef.needItemsFromWgo[k]);
					uIResourceBasedCraftWindowData.IngredientsHasCount.Add(wgoData.Inventory.Data.GetTotalCountInInventory(uIResourceBasedCraftWindowData.currentSurveyDef.needItemsFromWgo[k].id));
				}
			}
		}
	}

	private void OnStartSurvey()
	{
		if (WgoData.CraftComponent.TryStartCraft(currentCraftElement))
		{
			currentCraftElement = WgoData.CraftComponent.CurrentCraftElement;
			if (!currentSurveyDef.isScienceFuelCraft)
			{
				LazyUI.GetWindow<UIResourceBasedCraftWindow>().Close();
				WgoData.ClearWorker();
				return;
			}
			currentCraftElement.UpdateActualOutputBeforeFinish();
			WgoData.CraftComponent.TryFinishCurCraft();
			onUpdateDataAction?.Invoke();
			LazyUI.GetWindow<UIResourceBasedCraftWindow>().Redraw();
		}
	}

	public bool CanStartCraft()
	{
		if (SelectedItem == null)
		{
			return false;
		}
		if (currentCraftElement != null)
		{
			return currentCraftElement.CanStartCraft(WgoData) == CraftStatus.OK;
		}
		return false;
	}

	private void OnResourcePickerPressed(UIItemCell windowItemCell)
	{
		UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
		UIMultiInventoryWindowData data = new UIMultiInventoryWindowData(MainGame.PlayerData, delegate(UIItemCell uiItemCell)
		{
			SelectedItem = uiItemCell.DisplayingItem;
			currentSurveyDef = GameBalance.GetSurveyDefForItemOrNull(uiItemCell.DisplayingItem.id);
			currentCraftElement = CreateCraftElement(SelectedItem, currentSurveyDef, WgoData);
			onUpdateDataAction?.Invoke();
			LazyUI.GetWindow<UIResourceBasedCraftWindow>().Redraw();
			LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		}, IsItemAllowed);
		window.Open(data);
	}

	private bool IsItemAllowed(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return allowedItemIds.Contains(item.id);
	}

	private CraftElementSurvey CreateCraftElement(Item selectedItem, SurveyDef surveyDef, WgoData wgoData)
	{
		List<NeedItemData> list = new List<NeedItemData>();
		list.Add(new NeedItemData(selectedItem.id, surveyDef.SurveyedItem.count));
		for (int i = 1; i < surveyDef.needItems.Count; i++)
		{
			list.Add(surveyDef.needItems[i]);
		}
		return new CraftElementSurvey(surveyDef, list, new CraftParamsData(surveyDef.id, wgoData), selectedItem.id);
	}
}
