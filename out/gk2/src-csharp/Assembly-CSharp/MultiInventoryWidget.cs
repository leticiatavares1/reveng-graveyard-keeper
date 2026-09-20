using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class MultiInventoryWidget : LazyWidget<MultiInventoryWidgetData>
{
	[SerializeField]
	private Transform inventoryContainer;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private LocalizedLabel headerLabel;

	private GamepadNavigationController gamepadNavigationController;

	private List<InventoryWidget> drawnInventories = new List<InventoryWidget>();

	private bool subscribedInventoryEvents;

	private Item selectedBag;

	public Action OnInventoryRemove { get; set; }

	public Action OnInventoryAdd { get; set; }

	public Action OnAnyInventoryRedraw { get; set; }

	public Action OnMoveAllSimilarBtnInteractableChanged { get; set; }

	public MultiInventoryWidgetData Data => data;

	public bool IsBagSelected => selectedBag != null;

	public bool IsMoveAllSimilarBtnInteractable
	{
		get
		{
			for (int i = 0; i < drawnInventories.Count; i++)
			{
				if (drawnInventories[i].InventoryHeaderWidget.IsMoveAllSimilarBtnInteractable)
				{
					return true;
				}
			}
			return false;
		}
	}

	public List<InventoryWidget> DrawnInventories => drawnInventories;

	protected override void SetData(MultiInventoryWidgetData data)
	{
		base.SetData(data);
		scrollRect.verticalNormalizedPosition = 1f;
		data.OnInventoryAdded = OnInventoryAdded;
		data.OnInventoryRemoved = OnInventoryRemoved;
		data.OnMoveAllSimilarTargetChanged = RefreshMoveAllSimilarBtnInteractable;
		foreach (InventoryWidgetDataBase inventoriesDatum in data.inventoriesData)
		{
			AddInventoryWidget(inventoriesDatum);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		UpdateHeader();
		for (int i = 0; i < data.inventoriesData.Count; i++)
		{
			InventoryWidget inventoryWidget = drawnInventories[i];
			inventoryWidget.doNotTriggerOnRedraw = true;
			inventoryWidget.Draw(data.inventoriesData[i]);
			inventoryWidget.doNotTriggerOnRedraw = false;
		}
		OnInventoryRedraw();
	}

	private void UpdateHeader()
	{
		if (headerLabel == null)
		{
			headerLabel = GetComponentInChildren<LocalizedLabel>(includeInactive: true);
		}
		if (!(headerLabel == null))
		{
			headerLabel.langToken = (string.IsNullOrEmpty(data.HeaderLocaleId) ? "ui_multiinventory" : data.HeaderLocaleId);
			headerLabel.Localize();
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (data != null)
		{
			data.OnMoveAllSimilarTargetChanged = null;
		}
		for (int num = drawnInventories.Count - 1; num >= 0; num--)
		{
			ReleaseInventoryWidget(drawnInventories[num]);
		}
		drawnInventories.Clear();
		data?.onWidgetHide?.Invoke();
	}

	public void UpdatePrices(InventoryWidgetBase<InventoryWidgetData>.ItemPriceDelegate priceDelegate, int countModificator)
	{
		if (priceDelegate == null)
		{
			return;
		}
		foreach (InventoryWidget drawnInventory in drawnInventories)
		{
			drawnInventory.UpdatePrices(priceDelegate, countModificator);
		}
	}

	public void UpdateHappinessStatusIcons(Vendor vendor, Func<string, int> getExtraSoldCount = null, float extraUsedHappiness = 0f)
	{
		foreach (InventoryWidget drawnInventory in drawnInventories)
		{
			drawnInventory.UpdateHappinessStatusIcons(vendor, getExtraSoldCount, extraUsedHappiness);
		}
	}

	private void OnInventoryRemoved(int index)
	{
		ReleaseInventoryWidget(drawnInventories[index]);
		drawnInventories.RemoveAt(index);
		OnInventoryRemove?.Invoke();
		OnInventoryRedraw();
	}

	private void OnInventoryAdded(InventoryWidgetDataBase inventoryWidgetData)
	{
		AddInventoryWidget(inventoryWidgetData);
		List<InventoryWidget> list = drawnInventories;
		list[list.Count - 1].Draw(inventoryWidgetData);
		OnInventoryAdd?.Invoke();
		OnInventoryRedraw();
	}

	private void OnInventoryRedraw()
	{
		((RectTransform)base.transform).RefreshContentFitter();
		Canvas.ForceUpdateCanvases();
		OnAnyInventoryRedraw?.Invoke();
	}

	private void AddInventoryWidget(InventoryWidgetDataBase widgetDataBase)
	{
		InventoryWidget inventoryWidget;
		if (widgetDataBase is BagInventoryWidgetData)
		{
			inventoryWidget = UIPrefabsPooler.Instance.GetElementFromPool<BagInventoryWidget>(inventoryContainer);
		}
		else
		{
			InventoryWidgetData obj = widgetDataBase as InventoryWidgetData;
			bool flag = false;
			if (obj.Inventory.Data.TryGetProperty<WhiteListFilterSerializedItemProperty>(out var property))
			{
				for (int i = 0; i < property.WhiteList.itemsIds.Count; i++)
				{
					if (GameBalance.Me.GetData<ItemDef>(property.WhiteList.itemsIds[i]).itemSize == ItemSize.Big)
					{
						flag = true;
					}
				}
			}
			inventoryWidget = ((!flag) ? UIPrefabsPooler.Instance.GetElementFromPool<InventoryWidget>(inventoryContainer) : UIPrefabsPooler.Instance.GetElementFromPool<BigItemInventoryWidget>(inventoryContainer));
		}
		if (widgetDataBase is InventoryWidgetData inventoryWidgetData)
		{
			inventoryWidgetData.OnWidgetPressed = OnWidgetPressed;
		}
		drawnInventories.Add(inventoryWidget);
		inventoryWidget.Init();
		inventoryWidget.onRedraw += OnInventoryRedraw;
		inventoryWidget.InventoryHeaderWidget.OnMoveAllSimilarBtnInteractableChanged += OnHeaderMoveAllSimilarBtnInteractableChanged;
	}

	private void ReleaseInventoryWidget(InventoryWidget inventoryWidget)
	{
		inventoryWidget.InventoryHeaderWidget.OnMoveAllSimilarBtnInteractableChanged -= OnHeaderMoveAllSimilarBtnInteractableChanged;
		inventoryWidget.onRedraw -= OnInventoryRedraw;
		inventoryWidget.Hide();
		inventoryWidget.Data.OnWidgetPressed = null;
		if (inventoryWidget is BagInventoryWidget element)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(element);
		}
		else if (inventoryWidget is BigItemInventoryWidget element2)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(element2);
		}
		else
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(inventoryWidget);
		}
	}

	public void SelectFirstWidget()
	{
		MultiInventoryWidgetMode widgetSelectionMode = data.WidgetSelectionMode;
		if ((widgetSelectionMode == MultiInventoryWidgetMode.SelectionWithMoveBtn || widgetSelectionMode == MultiInventoryWidgetMode.SelectionWithoutMoveBtn || widgetSelectionMode == MultiInventoryWidgetMode.BagMode) && drawnInventories.Count > 0)
		{
			SetDefaultStateForWidgetAndInactiveForOthers(drawnInventories[0]);
		}
	}

	private void OnWidgetPressed(InventoryWidget inventoryWidget)
	{
		MultiInventoryWidgetMode widgetSelectionMode = data.WidgetSelectionMode;
		if (widgetSelectionMode == MultiInventoryWidgetMode.SelectionWithMoveBtn || widgetSelectionMode == MultiInventoryWidgetMode.SelectionWithoutMoveBtn || widgetSelectionMode == MultiInventoryWidgetMode.BagMode)
		{
			SetDefaultStateForWidgetAndInactiveForOthers(inventoryWidget);
		}
		if (data.WidgetSelectionMode == MultiInventoryWidgetMode.BagMode && inventoryWidget.Data.Inventory.Data != selectedBag)
		{
			SetDefaultStateForWidgetAndInactiveForOthers(inventoryWidget);
		}
	}

	public void EnableBagMode(Item bag, Action onBtnPressed)
	{
		selectedBag = bag;
		data.OnMoveAllSimilarPressed = onBtnPressed;
		data.WidgetSelectionMode = MultiInventoryWidgetMode.BagMode;
		InventoryWidget inventoryWidget = drawnInventories[0];
		data.SelectedWidgetData = inventoryWidget.Data;
		SetDefaultStateForWidgetAndInactiveForOthers(inventoryWidget);
	}

	public void DisableBagMode()
	{
		DisableBagMode(selectedBag);
	}

	public void DisableBagMode(Item bag)
	{
		data.OnMoveAllSimilarPressed = null;
		data.SelectedWidgetData = null;
		drawnInventories[0].ChangeMoveAllBtnState(isActive: false);
		for (int i = 0; i < drawnInventories.Count; i++)
		{
			if (data.WidgetSelectionMode == MultiInventoryWidgetMode.BagMode && IsWidgetForBag(drawnInventories[i], selectedBag))
			{
				drawnInventories[i].Data.ItemRelatedWidgetState = ItemRelatedWidgetState.Default;
				drawnInventories[i].ChangeMoveAllBtnState(isActive: false);
				drawnInventories[i].Redraw();
			}
			else if (drawnInventories[i].Data.ItemRelatedWidgetState != ItemRelatedWidgetState.Disabled)
			{
				drawnInventories[i].Data.ItemRelatedWidgetState = ItemRelatedWidgetState.Default;
				drawnInventories[i].ChangeMoveAllBtnState(isActive: false);
				drawnInventories[i].Redraw();
			}
		}
		data.WidgetSelectionMode = MultiInventoryWidgetMode.Default;
		selectedBag = null;
	}

	private void RefreshMoveAllSimilarBtnInteractable()
	{
		for (int i = 0; i < drawnInventories.Count; i++)
		{
			drawnInventories[i].InventoryHeaderWidget.RefreshMoveAllSimilarBtnInteractable();
		}
	}

	private void OnHeaderMoveAllSimilarBtnInteractableChanged()
	{
		OnMoveAllSimilarBtnInteractableChanged?.Invoke();
	}

	private void SetDefaultStateForWidgetAndInactiveForOthers(InventoryWidget inventoryWidget)
	{
		data.SelectedWidgetData = inventoryWidget.Data;
		inventoryWidget.Data.ItemRelatedWidgetState = ItemRelatedWidgetState.Default;
		inventoryWidget.UpdateItemRelatedWidgetStateForEveryThing();
		MultiInventoryWidgetMode widgetSelectionMode = data.WidgetSelectionMode;
		if (widgetSelectionMode == MultiInventoryWidgetMode.SelectionWithMoveBtn || widgetSelectionMode == MultiInventoryWidgetMode.BagMode)
		{
			inventoryWidget.ChangeMoveAllBtnState(isActive: true, data.OnMoveAllSimilarPressed, data.GetMoveAllSimilarTargetInventory);
		}
		for (int i = 0; i < drawnInventories.Count; i++)
		{
			if (!(drawnInventories[i] == inventoryWidget))
			{
				if (data.WidgetSelectionMode == MultiInventoryWidgetMode.BagMode && IsWidgetForBag(drawnInventories[i], selectedBag))
				{
					drawnInventories[i].Data.ItemRelatedWidgetState = ItemRelatedWidgetState.Disabled;
					drawnInventories[i].UpdateItemRelatedWidgetStateForEveryThing();
					drawnInventories[i].ChangeMoveAllBtnState(isActive: false);
				}
				else if (data.WidgetSelectionMode == MultiInventoryWidgetMode.BagMode && drawnInventories[i].Data is BagInventoryWidgetData bagInventoryWidgetData && bagInventoryWidgetData.ParentInventory == inventoryWidget.Data.Inventory)
				{
					drawnInventories[i].Data.ItemRelatedWidgetState = ItemRelatedWidgetState.Inactive;
					drawnInventories[i].UpdateItemRelatedWidgetStateForEveryThing();
					drawnInventories[i].ChangeMoveAllBtnState(isActive: false);
				}
				else if (drawnInventories[i].Data.ItemRelatedWidgetState != ItemRelatedWidgetState.Disabled)
				{
					drawnInventories[i].Data.ItemRelatedWidgetState = ItemRelatedWidgetState.Inactive;
					drawnInventories[i].UpdateItemRelatedWidgetStateForEveryThing();
					drawnInventories[i].ChangeMoveAllBtnState(isActive: false);
				}
			}
		}
	}

	private bool IsWidgetForBag(InventoryWidget widget, Item bag)
	{
		if (bag != null && widget.Data?.Inventory?.Data != null)
		{
			return widget.Data.Inventory.Data.UniqueId == bag.UniqueId;
		}
		return false;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new MultiInventoryWidgetData());
	}
}
