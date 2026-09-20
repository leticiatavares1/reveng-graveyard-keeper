using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UISingleCraftWindow : UIBaseCraftSelectionWindow
{
	[SerializeField]
	private UIInfoWidget infoWidget;

	private int initialCraftsCount;

	public override void Init()
	{
		base.Init();
		startCraftButton.onClick.AddListener(OnStartCraftPressed);
	}

	public override void DeInit()
	{
		base.DeInit();
		startCraftButton.onClick.RemoveAllListeners();
	}

	protected override void SetData(UIBaseCraftSelectionWindowData data)
	{
		base.SetData(data);
		SetupCraftsCountCache();
		SubscribeToDataChanges();
	}

	public override void Redraw()
	{
		base.Redraw();
		DrawBaseElements();
		UpdateTalent();
		UIInfoWidgetData uIInfoWidgetData = new UIInfoWidgetData(data.WgoData, data.CraftDefinition);
		infoWidget.Draw(uIInfoWidgetData);
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	protected override void OnStartCraftPressed()
	{
		if (data.CraftQueue.Count == 0)
		{
			onStartCraftPressed?.Invoke();
		}
		else
		{
			UpdateCraftsCountValue();
		}
		Close();
	}

	protected override void UpdateButtonsInteractableState()
	{
		startCraftButton.interactable = (data.CraftQueue.Count == 0 && data.CanStartCraft) || data.CraftQueue.Count > 0;
		UpdateCraftGamepadTips();
	}

	protected override string GetStartCraftGamepadTipKey()
	{
		if (data != null && data.CraftQueue != null && data.CraftQueue.Count > 0)
		{
			return "ui_update";
		}
		return "ui_create";
	}

	protected override void UpdateButtonsText()
	{
		if (data.CraftQueue.Count == 0)
		{
			startCraftButtonText.text = LLBase.L("ui_create");
		}
		else
		{
			startCraftButtonText.text = LLBase.L("ui_update");
		}
		AddTalentLockText(startCraftButtonText);
	}

	protected override void UpdateCraftCountElementsInteractableStatus()
	{
		if (data.CraftComponent.CraftElementsQueue.Count == 0)
		{
			base.UpdateCraftCountElementsInteractableStatus();
		}
		else
		{
			minusCraftButton.interactable = (data.CraftQueue[0].IsStarted ? (data.CraftsCount > 1) : (data.CraftsCount > 0));
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateGamepadDependentStuff();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.StartCraft, base.OnStartCraft);
		gameKeyDelegates.Add(GameKey.DpadUp, OnDpadUpPressed);
		gameKeyDelegates.Add(GameKey.DpadDown, OnDpadDownPressed);
		gameKeyDelegates.Add(GameKey.Up, OnDpadUpPressed);
		gameKeyDelegates.Add(GameKey.Down, OnDpadDownPressed);
		gameKeyDelegates.Add(GameKey.RightClick, OnPressedBack);
		return gameKeyDelegates;
	}

	protected override bool OnDpadUpPressed()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		if (base.GamepadNavigationController.FocusedItem.TryGetComponent<UICraftItemCell>(out var component) && component.NextItemButton.gameObject.activeSelf && component.NextItemButton.interactable)
		{
			component.NextItemButton.onClick?.Invoke();
			return true;
		}
		return base.OnDpadUpPressed();
	}

	protected override bool OnDpadDownPressed()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		if (base.GamepadNavigationController.FocusedItem.TryGetComponent<UICraftItemCell>(out var component) && component.PrevItemButton.gameObject.activeSelf && component.PrevItemButton.interactable)
		{
			component.PrevItemButton.onClick?.Invoke();
			return true;
		}
		return base.OnDpadDownPressed();
	}

	protected override void PrintTips()
	{
		PrintTips(base.GamepadNavigationController.FocusedItem);
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null)
		{
			UICraftItemCell component;
			if (gamepadNavigationItem == outputItem.UIItemCell.GamepadNavigationItem)
			{
				AddCraftCountGamepadTips(list);
			}
			else if (gamepadNavigationItem.TryGetComponent<UICraftItemCell>(out component))
			{
				if (component.NextItemButton.gameObject.activeSelf)
				{
					list.Add(new LazyGameKeyTip(GameKey.DpadUp, "tip_next", component.NextItemButton.interactable));
				}
				if (component.PrevItemButton.gameObject.activeSelf)
				{
					list.Add(new LazyGameKeyTip(GameKey.DpadDown, "tip_prev", component.PrevItemButton.interactable));
				}
			}
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	protected override void UpdateCounters()
	{
		outputItem.UIItemCell.OnMultiplierChange(data.CraftsCount, data.CraftsCount != 1);
		int multiplierValue = ((data.CraftQueue.Count > 0 && data.CraftQueue[0].IsStarted) ? (data.CraftsCount - 1) : data.CraftsCount);
		foreach (UICraftItemCell displayedIngredient in displayedIngredients)
		{
			displayedIngredient.SetMultiplierValue(multiplierValue);
		}
	}

	public override void Close()
	{
		base.Close();
		UnsubscribeFromDataChanges();
	}

	private void RemoveFromQueue(CraftElementBase craftElement)
	{
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (currentCraftElement != null && currentCraftElement == craftElement)
		{
			data.CraftComponent.RemoveCurNotStartedCraft();
		}
		else
		{
			data.CraftComponent.RemoveFromQueue(craftElement);
		}
	}

	private void SetupCraftsCountCache()
	{
		initialCraftsCount = 1;
		if (data.CraftQueue.Count == 1)
		{
			initialCraftsCount = data.CraftQueue[0].Count;
		}
		else if (data.CraftQueue.Count == 2)
		{
			initialCraftsCount = data.CraftQueue[0].Count + data.CraftQueue[1].Count;
		}
		data.SetCraftCount(initialCraftsCount);
	}

	private void UpdateCraftsCountValue()
	{
		int num = data.CraftsCount - initialCraftsCount;
		int num2 = Mathf.Abs(num);
		if (num > 0)
		{
			if (data.CraftQueue.Count == 1)
			{
				data.CraftQueue[0].Count += num;
			}
			else
			{
				data.CraftQueue[1].Count += num;
			}
		}
		else
		{
			if (num >= 0)
			{
				return;
			}
			if (data.CraftQueue.Count == 1)
			{
				RemoveCraftsCount(data.CraftQueue[0], num2);
			}
			else if (data.CraftQueue.Count == 2)
			{
				if (data.CraftQueue[1].Count > num2)
				{
					data.CraftQueue[1].Count -= num2;
					return;
				}
				num2 -= data.CraftQueue[1].Count;
				RemoveFromQueue(data.CraftQueue[1]);
				RemoveCraftsCount(data.CraftQueue[0], num2);
			}
		}
	}

	private void RemoveCraftsCount(CraftElementBase craftElement, int removableCount)
	{
		if (removableCount != 0)
		{
			if (craftElement.Count <= removableCount)
			{
				RemoveFromQueue(craftElement);
			}
			else
			{
				craftElement.Count -= removableCount;
			}
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Wgo wgo = Wgo.Spawn(new WgoData("descent_ladder_forest_broken", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		CraftInteractionHandler craftInteractionHandler = new CraftInteractionHandler();
		craftInteractionHandler.Init(wgo);
		craftInteractionHandler.HasInteraction(MainGame.PlayerController);
		craftInteractionHandler.Interact(MainGame.PlayerController);
	}
}
