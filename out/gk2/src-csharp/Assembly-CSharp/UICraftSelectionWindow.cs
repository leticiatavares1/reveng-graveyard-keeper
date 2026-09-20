using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICraftSelectionWindow : UIBaseCraftSelectionWindow, IUIWindowCustomOperable
{
	[SerializeField]
	private LazyButton addToQueueButton;

	[SerializeField]
	private TextMeshProUGUI gamepadTipQueue;

	[SerializeField]
	private UIMixItemCell boostItemCell;

	[SerializeField]
	private TextMeshProUGUI addToQueueButtonText;

	private UICraftSelectionWindowData Data => data as UICraftSelectionWindowData;

	public override void Init()
	{
		base.Init();
		addToQueueButton.onClick.AddListener(OnAddToQueuePressed);
		startCraftButton.onClick.AddListener(OnStartCraftPressed);
	}

	public override void DeInit()
	{
		base.DeInit();
		addToQueueButton.onClick.RemoveAllListeners();
		startCraftButton.onClick.RemoveAllListeners();
	}

	public override void Redraw()
	{
		base.Redraw();
		DrawBaseElements();
		UpdateTalent();
		DrawBoostItemCell();
		bool active = !(data.WgoData.Worker is ZombieWgoData) && !Data.IsGravePartRemove;
		startCraftButton.gameObject.SetActive(active);
		gamepadTipStartCraft.gameObject.SetActive(active);
		((RectTransform)base.transform).RefreshContentFitter();
		bool active2 = !data.CraftDefinition.isAutopsyCraft && !data.CraftDefinition.isAddToQueueDisabled;
		addToQueueButton.gameObject.SetActive(active2);
		gamepadTipQueue.gameObject.SetActive(active2);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		UpdateCraftGamepadTips();
	}

	private void DrawBoostItemCell()
	{
		boostItemCell.gameObject.SetActive(value: false);
		CraftDef craftDefinition = data.CraftDefinition;
		AlchemyMixDef mixDef = craftDefinition as AlchemyMixDef;
		if (mixDef != null && mixDef.BoostCraft != null)
		{
			boostItemCell.gameObject.SetActive(value: true);
			boostItemCell.runesLabel.text = mixDef.BoostCraft.GetBoostRunesAsString();
			boostItemCell.uiItemCell.DrawCustom(mixDef.BoostCraft.GetCraftResultIcon(data.WgoData), 1, interactable: true);
			boostItemCell.uiItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
			{
				UITooltip.ShowAlchemyBoostInfo(cell.transform as RectTransform, mixDef.BoostCraft);
			};
		}
	}

	protected override void UpdateButtonsText()
	{
		startCraftButtonText.text = LLBase.L("ui_craft");
		AddTalentLockText(startCraftButtonText);
		addToQueueButtonText.text = (Data.IsGravePartRemove ? LLBase.L("remove") : LLBase.L("ui_add_to_queue"));
	}

	protected override void Update()
	{
		base.Update();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.StartCraft, base.OnStartCraft);
		gameKeyDelegates.Add(GameKey.AddCraftToQueue, OnAddToQueue);
		gameKeyDelegates.Add(GameKey.DpadUp, OnDpadUpPressed);
		gameKeyDelegates.Add(GameKey.DpadDown, OnDpadDownPressed);
		gameKeyDelegates.Add(GameKey.Up, OnDpadUpPressed);
		gameKeyDelegates.Add(GameKey.Down, OnDpadDownPressed);
		gameKeyDelegates.Add(GameKey.RightClick, OnPressedBack);
		return gameKeyDelegates;
	}

	protected override void UpdateCraftGamepadTips()
	{
		base.UpdateCraftGamepadTips();
		if (!(gamepadTipQueue == null) && !(addToQueueButton == null))
		{
			if (data != null)
			{
				gamepadTipQueue.text = new LazyGameKeyTip(GameKey.AddCraftToQueue, Data.IsGravePartRemove ? "remove" : "ui_add_to_queue", addToQueueButton.interactable).ToString();
			}
			else
			{
				gamepadTipQueue.text = new LazyGameKeyTip(GameKey.AddCraftToQueue, "ui_add_to_queue", addToQueueButton.interactable).ToString();
			}
		}
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

	protected override void UpdateButtonsInteractableState()
	{
		bool flag = !(data.WgoData?.Worker is ZombieWgoData) && !Data.IsGravePartRemove;
		startCraftButton.interactable = data.CanStartCraft && flag;
		addToQueueButton.interactable = !data.CraftDefinition.isFuelCraft && !data.CraftDefinition.isAutopsyCraft && IsCraftAvailableByExtensionState() && !data.CraftDefinition.isAddToQueueDisabled;
		UpdateCraftGamepadTips();
	}

	private bool IsCraftAvailableByExtensionState()
	{
		if (data?.WgoData == null || data.CraftDefinition == null)
		{
			return false;
		}
		return data.WgoData.CraftComponent.IsCraftAllowedByAttachedExtensions(data.CraftDefinition);
	}

	private void OnAddToQueuePressed()
	{
		onAddToCraftQueuePressed?.Invoke();
		Close();
	}

	private bool OnAddToQueue()
	{
		if (addToQueueButton.interactable)
		{
			OnAddToQueuePressed();
			return true;
		}
		return false;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		WgoData wgoData = new WgoData("woodworking_workbench_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		UICraftSelectionWindowData uICraftSelectionWindowData = new UICraftSelectionWindowData(wgoData, wgoData.CraftComponent.CraftsIn[0] as CraftDef, null, null);
		LazyUI.GetWindow<UICraftSelectionWindow>().Open(uICraftSelectionWindowData);
	}

	[LazyUITest]
	protected void TestDrawStar()
	{
		WgoData wgoData = new WgoData("test_workbench", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		UICraftSelectionWindowData uICraftSelectionWindowData = new UICraftSelectionWindowData(wgoData, wgoData.CraftComponent.CraftsIn[0] as CraftDef, null, null);
		LazyUI.GetWindow<UICraftSelectionWindow>().Open(uICraftSelectionWindowData);
	}

	bool IUIWindowCustomOperable.get_IsShown()
	{
		return base.IsShown;
	}
}
