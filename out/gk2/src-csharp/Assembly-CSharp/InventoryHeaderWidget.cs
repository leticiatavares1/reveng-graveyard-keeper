using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHeaderWidget : LazyWidget<InventoryHeaderWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI header;

	[SerializeField]
	private Image headerBackground;

	[SerializeField]
	private Image headerIcon;

	[SerializeField]
	private Image decorWhenNoHeaderIcon;

	[SerializeField]
	private LazyButton moveAllSimilarItemsToBagBtn;

	[SerializeField]
	private Sprite headerBackgroundActiveSprite;

	[SerializeField]
	private Sprite headerBackgroundInactiveSprite;

	[SerializeField]
	private TextStyle headerActiveStyle;

	[SerializeField]
	private TextStyle headerInactiveStyle;

	private Func<Inventory> getMoveAllSimilarTargetInventory;

	private Inventory subscribedSourceInventory;

	private Inventory subscribedTargetInventory;

	private bool moveAllSimilarBtnShouldBeShown;

	private bool subscribedToInputEvents;

	public LazyButton MoveAllSimilarItemsToBagBtn => moveAllSimilarItemsToBagBtn;

	public InventoryHeaderWidgetData Data => data;

	public bool IsMoveAllSimilarBtnInteractable
	{
		get
		{
			if (moveAllSimilarItemsToBagBtn != null && moveAllSimilarBtnShouldBeShown)
			{
				return moveAllSimilarItemsToBagBtn.interactable;
			}
			return false;
		}
	}

	public event Action OnMoveAllSimilarBtnInteractableChanged;

	public override void Init()
	{
		base.Init();
		if (!(moveAllSimilarItemsToBagBtn == null))
		{
			moveAllSimilarItemsToBagBtn.onClick.RemoveAllListeners();
			moveAllSimilarItemsToBagBtn.onClick.AddListener(OnMoveAllSimilarItemFromPlayerToChestToBagBtnPressed);
			moveAllSimilarItemsToBagBtn.onEnterSound = string.Empty;
			moveAllSimilarItemsToBagBtn.onNotInteractableEnterSound = string.Empty;
			moveAllSimilarItemsToBagBtn.onEnter.RemoveAllListeners();
			moveAllSimilarItemsToBagBtn.onNotInteractableEnter.RemoveAllListeners();
			moveAllSimilarItemsToBagBtn.onExit.RemoveAllListeners();
			moveAllSimilarItemsToBagBtn.onNotInteractableExit.RemoveAllListeners();
			moveAllSimilarItemsToBagBtn.onEnter.AddListener(OnMoveAllSimilarBtnOver);
			moveAllSimilarItemsToBagBtn.onNotInteractableEnter.AddListener(OnMoveAllSimilarBtnOver);
			moveAllSimilarItemsToBagBtn.onExit.AddListener(OnMoveAllSimilarBtnOut);
			moveAllSimilarItemsToBagBtn.onNotInteractableExit.AddListener(OnMoveAllSimilarBtnOut);
			moveAllSimilarItemsToBagBtn.SetCallbacksIntoGamepadNavigationItem();
			moveAllSimilarItemsToBagBtn.gameObject.SetActive(value: false);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		UpdateHeader();
		if (moveAllSimilarBtnShouldBeShown)
		{
			RefreshMoveAllSimilarBtnInteractable();
		}
	}

	public override void Hide()
	{
		SetMoveAllSimilarBtnState(isActive: false);
		base.Hide();
	}

	public void ChangeActiveViewState(bool isActive)
	{
		data.IsActiveViewState = isActive;
		UpdateViewState();
	}

	public void SetMoveAllSimilarBtnState(bool isActive, Action onPress = null, Func<Inventory> getTargetInventory = null)
	{
		if (moveAllSimilarItemsToBagBtn == null)
		{
			return;
		}
		bool isMoveAllSimilarBtnInteractable = IsMoveAllSimilarBtnInteractable;
		moveAllSimilarBtnShouldBeShown = isActive;
		ApplyMoveAllSimilarBtnVisibility();
		moveAllSimilarItemsToBagBtn.onClick.RemoveAllListeners();
		if (onPress != null)
		{
			moveAllSimilarItemsToBagBtn.onClick.AddListener(delegate
			{
				onPress();
			});
		}
		getMoveAllSimilarTargetInventory = (isActive ? getTargetInventory : null);
		if (isActive)
		{
			RefreshMoveAllSimilarBtnInteractable();
			return;
		}
		HideMoveAllSimilarTooltip(immediately: true);
		UnsubscribeFromMoveAllSimilarInventories();
		NotifyMoveAllSimilarBtnInteractableChanged(isMoveAllSimilarBtnInteractable);
	}

	public void RefreshMoveAllSimilarBtnInteractable()
	{
		if (moveAllSimilarBtnShouldBeShown)
		{
			UpdateMoveAllSimilarInventorySubscriptions();
			UpdateMoveAllSimilarBtnInteractable();
		}
	}

	private void UpdateHeader()
	{
		string headerIconId = data.HeaderIconId;
		if (!string.IsNullOrEmpty(headerIconId))
		{
			headerIcon.gameObject.SetActive(value: true);
			headerIcon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(headerIconId);
			decorWhenNoHeaderIcon.gameObject.SetActive(value: false);
			headerIcon.SetNativeSize();
		}
		else
		{
			headerIcon.gameObject.SetActive(value: false);
			decorWhenNoHeaderIcon.gameObject.SetActive(value: true);
		}
		string text = ((!string.IsNullOrEmpty(data.CustomHeaderId)) ? data.CustomHeaderId : data.Inventory?.ViewId);
		if (data.DrawHeader && !string.IsNullOrEmpty(text))
		{
			header.text = LLBase.L(text);
			header.transform.parent.gameObject.SetActive(value: true);
		}
		else
		{
			header.transform.parent.gameObject.SetActive(value: false);
		}
		UpdateViewState();
	}

	private void UpdateViewState()
	{
		if (data.IsActiveViewState)
		{
			headerBackground.sprite = headerBackgroundActiveSprite;
			headerActiveStyle.ApplyStyle(header);
		}
		else
		{
			headerBackground.sprite = headerBackgroundInactiveSprite;
			headerInactiveStyle.ApplyStyle(header);
		}
	}

	private void OnMoveAllSimilarItemFromPlayerToChestToBagBtnPressed()
	{
		data.OnMoveAllSimilarItemFromPlayerToChest?.Invoke();
	}

	private void OnMoveAllSimilarBtnOver()
	{
		UITooltip.ShowSimpleInfo(moveAllSimilarItemsToBagBtn.transform, LLBase.L("btn_hint_move_all_identical_items"));
	}

	private void OnMoveAllSimilarBtnOut()
	{
		UITooltip.Hide();
	}

	private void HideMoveAllSimilarTooltip(bool immediately)
	{
		if (!(moveAllSimilarItemsToBagBtn == null) && UITooltip.IsTooltipShowingAtTarget(moveAllSimilarItemsToBagBtn.transform as RectTransform))
		{
			if (immediately)
			{
				UITooltip.HideImmediately();
			}
			else
			{
				UITooltip.Hide();
			}
		}
	}

	private void UpdateMoveAllSimilarInventorySubscriptions()
	{
		Inventory inventory = data?.Inventory;
		Inventory inventory2 = getMoveAllSimilarTargetInventory?.Invoke();
		if (subscribedSourceInventory != inventory)
		{
			UnsubscribeInventory(ref subscribedSourceInventory);
			SubscribeInventory(inventory, ref subscribedSourceInventory);
		}
		if (subscribedTargetInventory != inventory2)
		{
			UnsubscribeInventory(ref subscribedTargetInventory);
			SubscribeInventory(inventory2, ref subscribedTargetInventory);
		}
	}

	private void SubscribeInventory(Inventory inventory, ref Inventory subscribedInventory)
	{
		if (inventory != null)
		{
			inventory.OnItemsAdd += OnMoveAllSimilarInventoriesChanged;
			inventory.OnItemsRemove += OnMoveAllSimilarInventoriesChanged;
			subscribedInventory = inventory;
		}
	}

	private void UnsubscribeInventory(ref Inventory subscribedInventory)
	{
		if (subscribedInventory != null)
		{
			subscribedInventory.OnItemsAdd -= OnMoveAllSimilarInventoriesChanged;
			subscribedInventory.OnItemsRemove -= OnMoveAllSimilarInventoriesChanged;
			subscribedInventory = null;
		}
	}

	private void UnsubscribeFromMoveAllSimilarInventories()
	{
		UnsubscribeInventory(ref subscribedSourceInventory);
		UnsubscribeInventory(ref subscribedTargetInventory);
	}

	private void OnMoveAllSimilarInventoriesChanged(List<Item> items)
	{
		UpdateMoveAllSimilarBtnInteractable();
	}

	private void UpdateMoveAllSimilarBtnInteractable()
	{
		if (moveAllSimilarBtnShouldBeShown && !(moveAllSimilarItemsToBagBtn == null))
		{
			bool isMoveAllSimilarBtnInteractable = IsMoveAllSimilarBtnInteractable;
			Inventory inventory = data?.Inventory;
			Inventory inventory2 = getMoveAllSimilarTargetInventory?.Invoke();
			moveAllSimilarItemsToBagBtn.interactable = inventory != null && inventory2 != null && inventory2.CanTakeAnyItemsExistingInMeFromOtherInventory(inventory);
			NotifyMoveAllSimilarBtnInteractableChanged(isMoveAllSimilarBtnInteractable);
		}
	}

	private void NotifyMoveAllSimilarBtnInteractableChanged(bool wasInteractable)
	{
		if (wasInteractable != IsMoveAllSimilarBtnInteractable)
		{
			this.OnMoveAllSimilarBtnInteractableChanged?.Invoke();
		}
	}

	private void ApplyMoveAllSimilarBtnVisibility()
	{
		if (moveAllSimilarItemsToBagBtn == null)
		{
			return;
		}
		bool flag = moveAllSimilarBtnShouldBeShown && !LazyInput.IsGamepadActive;
		if (moveAllSimilarItemsToBagBtn.gameObject.activeSelf != flag)
		{
			moveAllSimilarItemsToBagBtn.gameObject.SetActive(flag);
			if (!flag)
			{
				HideMoveAllSimilarTooltip(immediately: true);
			}
		}
	}

	private void OnInputChanged()
	{
		ApplyMoveAllSimilarBtnVisibility();
	}

	private void TrySubscribeToInputEvents()
	{
		if (!subscribedToInputEvents)
		{
			LazyInput.OnInputChanged += OnInputChanged;
			LazyInput.OnActiveGamepadChangedEvent += OnInputChanged;
			subscribedToInputEvents = true;
		}
	}

	private void TryUnsubscribeFromInputEvents()
	{
		if (subscribedToInputEvents)
		{
			LazyInput.OnInputChanged -= OnInputChanged;
			LazyInput.OnActiveGamepadChangedEvent -= OnInputChanged;
			subscribedToInputEvents = false;
		}
	}

	private void OnEnable()
	{
		TrySubscribeToInputEvents();
		ApplyMoveAllSimilarBtnVisibility();
	}

	private void OnDisable()
	{
		TryUnsubscribeFromInputEvents();
		HideMoveAllSimilarTooltip(immediately: true);
	}

	protected override void TestDraw()
	{
	}
}
