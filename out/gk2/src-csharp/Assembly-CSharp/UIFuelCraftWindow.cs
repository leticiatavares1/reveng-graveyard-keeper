using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFuelCraftWindow : UIBaseCraftSelectionWindow
{
	[SerializeField]
	private UIInfoWidget infoWidget;

	[SerializeField]
	private TextMeshProUGUI descriptionLabel;

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
		SubscribeToDataChanges();
	}

	public override void Redraw()
	{
		base.Redraw();
		DrawBaseElements();
		outputItem.UIItemCell.ShowMouseSelectionFrame = false;
		UpdateDrawState();
		UpdateTalent();
		if (data.WgoData.id == "alchemy_flask_1_shed")
		{
			headerLabel.text = LLBase.L("ui_place");
			descriptionLabel.text = LLBase.L("ui_alchemy_flask_1_shed");
		}
		else
		{
			descriptionLabel.text = LLBase.L("ui_fuel_helper");
		}
		UIInfoWidgetData uIInfoWidgetData = new UIInfoWidgetData(data.WgoData, data.CraftDefinition);
		infoWidget.Draw(uIInfoWidgetData);
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	protected override void ChangeCraftCount(int delta)
	{
		if (delta == 0 || data == null)
		{
			return;
		}
		if (data.CraftComponent.CraftElementsQueue.Count == 0)
		{
			base.ChangeCraftCount(delta);
			return;
		}
		if (delta > 0)
		{
			ChangeQueueElementCount(delta);
		}
		else
		{
			RemoveQueueElementCount(-delta);
		}
		UpdateButtonsInteractableState();
	}

	protected override void UpdateButtonsText()
	{
		startCraftButtonText.text = LLBase.L("ui_place");
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
			minusCraftButton.interactable = data.CraftQueue.Count > 1 || data.CraftQueue[0].Count != 1 || !data.CraftQueue[0].IsStarted;
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override string GetStartCraftGamepadTipKey()
	{
		return "ui_place";
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

	protected override void PrintTips()
	{
		PrintTips(base.GamepadNavigationController.FocusedItem);
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null && gamepadNavigationItem == outputItem.UIItemCell.GamepadNavigationItem)
		{
			AddCraftCountGamepadTips(list);
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	protected override void SubscribeToDataChanges()
	{
		if (!subscribedToDataChanges)
		{
			data.SubscribeToDataChanges();
			data.CraftComponent.OnCraftAddedToQueue += UpdateDrawState;
			data.CraftComponent.OnCraftRemovedFromQueue += UpdateDrawState;
			subscribedToDataChanges = true;
		}
	}

	protected override void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChanges)
		{
			data.UnsubscribeFromDataChanges();
			data.CraftComponent.OnCraftAddedToQueue -= UpdateDrawState;
			data.CraftComponent.OnCraftRemovedFromQueue -= UpdateDrawState;
			subscribedToDataChanges = false;
		}
	}

	public override void Close()
	{
		base.Close();
		UnsubscribeFromDataChanges();
	}

	private void UpdateDrawState(CraftElementBase element = null)
	{
		outputItem.UIItemCell.OnMultiplierChange(1);
		data.SetCraftCount(1);
		if (data.CraftComponent.CraftElementsQueue.Count == 2)
		{
			outputItem.UpdateQueueCount(data.CraftComponent.CraftElementsQueue[0].Count + data.CraftComponent.CraftElementsQueue[1].Count);
		}
		else if (data.CraftComponent.CraftElementsQueue.Count == 1)
		{
			outputItem.UpdateQueueCount(data.CraftComponent.CraftElementsQueue[0].Count);
		}
		else
		{
			outputItem.UpdateQueueCount(0);
		}
		UpdateCraftCountElementsInteractableStatus();
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

	private void ChangeQueueElementCount(int delta)
	{
		if (data.CraftQueue.Count > 1)
		{
			data.CraftQueue[1].Count = Math.Min(data.CraftQueue[1].Count + delta, 999);
		}
		else
		{
			data.CraftQueue[0].Count = Math.Min(data.CraftQueue[0].Count + delta, 999);
		}
		UpdateDrawState();
	}

	private void RemoveQueueElementCount(int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (data.CraftQueue.Count == 0)
			{
				break;
			}
			OnMinusQueueElement();
		}
	}

	private void OnMinusQueueElement()
	{
		if (data.CraftQueue.Count > 1)
		{
			data.CraftQueue[1].Count--;
			if (data.CraftQueue[1].Count == 0)
			{
				RemoveFromQueue(data.CraftQueue[1]);
			}
		}
		else if (data.CraftQueue[0].Count != 1 || !data.CraftQueue[0].IsStarted)
		{
			data.CraftQueue[0].Count--;
			if (data.CraftQueue[0].Count == 0)
			{
				RemoveFromQueue(data.CraftQueue[0]);
			}
		}
		UpdateDrawState();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Wgo wgo = Wgo.Spawn(new WgoData("test_firewood_shed_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		CraftInteractionHandler craftInteractionHandler = new CraftInteractionHandler();
		craftInteractionHandler.Init(wgo);
		craftInteractionHandler.HasInteraction(MainGame.PlayerController);
		craftInteractionHandler.Interact(MainGame.PlayerController);
	}
}
