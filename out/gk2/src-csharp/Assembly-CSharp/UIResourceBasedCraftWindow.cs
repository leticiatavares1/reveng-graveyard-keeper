using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResourceBasedCraftWindow : LazyWindow<UIResourceBasedCraftWindowData>
{
	[SerializeField]
	private UIInfoWidget infoWidget;

	[SerializeField]
	private TextMeshProUGUI btnLabel;

	[SerializeField]
	private TextMeshProUGUI descLabel;

	[SerializeField]
	private UIItemCell mainIngredient;

	[SerializeField]
	private List<UIItemCell> ingredients;

	[SerializeField]
	private GameObject plusMainIngredientObj;

	[SerializeField]
	private GameObject ingredientsParent;

	[SerializeField]
	private LazyButton craftBtn;

	[SerializeField]
	private TextMeshProUGUI startTip;

	public override void Init()
	{
		base.Init();
		craftBtn.onClick.RemoveAllListeners();
		craftBtn.onClick.AddListener(OnBtnPressed);
		craftBtn.SetCallbacksIntoGamepadNavigationItem();
	}

	public override void Redraw()
	{
		base.Redraw();
		UIInfoWidgetData uIInfoWidgetData = new UIInfoWidgetData(data.WgoData);
		uIInfoWidgetData.ExcludePlayerFromMultiinventoryWhenCountItemsForFuel = false;
		infoWidget.Draw(uIInfoWidgetData);
		btnLabel.text = data.BtnText;
		descLabel.text = data.LabelText;
		if (data.SelectedItem == null || data.SelectedItem.IsEmpty)
		{
			mainIngredient.DrawEmptyInteractable();
			plusMainIngredientObj.SetActive(value: true);
		}
		else
		{
			mainIngredient.Draw(new Item(data.SelectedItem.id, data.MainIngredientCount));
			plusMainIngredientObj.SetActive(value: false);
		}
		mainIngredient.OnItemCellPress = OnMainIngredientPressed;
		craftBtn.interactable = data.CanStartCraft();
		if (data.Ingredients.Count > 0)
		{
			ingredientsParent.SetActive(value: true);
			for (int i = 0; i < ingredients.Count; i++)
			{
				if (i < data.Ingredients.Count)
				{
					ingredients[i].gameObject.SetActive(value: true);
					ingredients[i].Draw(new Item(data.Ingredients[i].id, (i < data.CraftNeedItemsCount) ? data.Ingredients[i].GetCount(data.WgoData) : data.Ingredients[i].GetCount()), isNeedItem: true, data.IngredientsHasCount[i]);
				}
				else
				{
					ingredients[i].gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			ingredientsParent.SetActive(value: false);
			foreach (UIItemCell ingredient in ingredients)
			{
				ingredient.gameObject.SetActive(value: false);
			}
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: false);
			if (data.SelectedItem == null || data.SelectedItem.IsEmpty)
			{
				base.GamepadNavigationController.SetFocusedItem(mainIngredient.GamepadNavigationItem);
			}
			else
			{
				base.GamepadNavigationController.SetFocusedItem(craftBtn.GetComponent<GamepadNavigationItem>());
			}
			startTip.text = new LazyGameKeyTip(GameKey.AlchemyStart, data.BtnText, craftBtn.interactable, gamepadOnly: true, translate: false).ToString();
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void OnBtnPressed()
	{
		data.OnBtnPressedAction?.Invoke();
	}

	private bool OnStartSurveyPressed()
	{
		if (craftBtn.interactable)
		{
			OnBtnPressed();
			return true;
		}
		return false;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.SurveyStart, OnStartSurveyPressed);
		return gameKeyDelegates;
	}

	private void OnMainIngredientPressed(UIItemCell itemCell)
	{
		data.OnMainIngredientPressedAction?.Invoke(itemCell);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Wgo wgo = Wgo.Spawn(new WgoData("survey_wgo", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		SurveyInteractionHandler surveyInteractionHandler = new SurveyInteractionHandler();
		surveyInteractionHandler.Init(wgo);
		surveyInteractionHandler.HasInteraction(MainGame.PlayerController);
		surveyInteractionHandler.Interact(MainGame.PlayerController);
		MainGame.PlayerData.Inventory.AddItemToInventory(new Item("clean_paper", 15));
		MainGame.PlayerData.Inventory.AddItemToInventory(new Item("faith", 15));
		MainGame.PlayerData.Inventory.AddItemToInventory(new Item("wheat_seed"));
		MainGame.PlayerData.Inventory.AddItemToInventory(new Item("cabbage:1"));
		MainGame.PlayerData.Inventory.AddItemToInventory(new Item("mushroom_brown"));
		MainGame.Instance.GameSave.talentSystemData.GetTalentBranch("talent_yellow").curTalentValue += 5;
	}
}
