using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class InventoryWidget : InventoryWidgetBase<InventoryWidgetData>
{
	[SerializeField]
	protected InventoryHeaderWidget inventoryHeaderWidget;

	[SerializeField]
	protected Image background;

	[SerializeField]
	protected LazyButton widgetBtn;

	[SerializeField]
	protected GridLayoutGroup grid;

	[SerializeField]
	private Sprite backgroundActiveSprite;

	[SerializeField]
	private Sprite backgroundInactiveSprite;

	protected List<UIItemCell> uiItemCells = new List<UIItemCell>();

	public bool doNotTriggerOnRedraw;

	private ItemRelatedWidgetState ItemRelatedWidgetState
	{
		get
		{
			if (data == null)
			{
				return ItemRelatedWidgetState.Default;
			}
			return data.ItemRelatedWidgetState;
		}
	}

	public virtual List<UIItemCell> Cells => uiItemCells;

	public InventoryHeaderWidget InventoryHeaderWidget => inventoryHeaderWidget;

	public InventoryWidgetData Data => data as InventoryWidgetData;

	public event Action onRedraw;

	public override void Init()
	{
		base.Init();
		inventoryHeaderWidget.Init();
		if (widgetBtn != null)
		{
			widgetBtn.onClick.RemoveAllListeners();
			widgetBtn.onClick.AddListener(OnWidgetPress);
		}
	}

	protected override void SetData(InventoryWidgetDataBase data)
	{
		UnsubscribeFromInventoryEvents();
		base.SetData(data);
		SubscribeToInventoryEvents();
	}

	public override void Redraw()
	{
		_ = Data.Inventory.ViewId;
		base.Redraw();
		int num = data.Inventory.Data.InventorySize;
		while (uiItemCells.Count > num)
		{
			int index = uiItemCells.Count - 1;
			UIItemCell uIItemCell = uiItemCells[index];
			uIItemCell.Flush();
			ReleaseCell(uIItemCell);
			uiItemCells.RemoveAt(index);
		}
		int count = uiItemCells.Count;
		while (num - count > 0)
		{
			uiItemCells.Add(GetNewCell());
			num--;
		}
		int inventoryFillSize = data.Inventory.Data.InventoryFillSize;
		for (int i = 0; i < inventoryFillSize; i++)
		{
			Item item = data.Inventory.Data.Inventory[i];
			bool drawAsInteractable = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(item);
			uiItemCells[i].ExtraRedTooltipLocId = data.ExtraRedTooltipLocIdProvider?.Invoke(item);
			ItemRelatedWidgetState cellRelatedWidgetState = GetCellRelatedWidgetState(item, drawAsInteractable);
			uiItemCells[i].Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, cellRelatedWidgetState == ItemRelatedWidgetState.Disabled, 0, drawCounter: true, forceNonEmpty: false, forceDrawCounter: false, cellRelatedWidgetState);
			uiItemCells[i].OnItemCellOver = onItemCellOver;
			uiItemCells[i].OnItemCellOut = onItemCellOut;
			uiItemCells[i].OnItemCellPress = onItemCellPress;
			uiItemCells[i].OnItemCellPress2 = onItemCellPress2;
			uiItemCells[i].OnItemCellDown = onItemCellDown;
			uiItemCells[i].OnWidgetPress = OnWidgetPress;
			uiItemCells[i].gameObject.SetActive(value: true);
		}
		for (int j = inventoryFillSize; j < data.Inventory.Data.InventorySize; j++)
		{
			bool flag = data.CustomItemsAvailableCondition != null && data.CustomItemsAvailableCondition(null);
			ItemRelatedWidgetState widgetState = ((data.DrawEmptyCellsAsDisabledWhenUnavailable && !flag) ? ItemRelatedWidgetState.Disabled : data.ItemRelatedWidgetState);
			uiItemCells[j].DrawEmptyWithState(widgetState, !flag);
			uiItemCells[j].OnWidgetPress = OnWidgetPress;
			uiItemCells[j].gameObject.SetActive(value: true);
		}
		for (int k = 0; k < uiItemCells.Count; k++)
		{
			if (uiItemCells[k].gameObject.activeSelf && uiItemCells[k].DisplayingItem != null && !uiItemCells[k].DisplayingItem.IsEmpty && data.CustomItemsNotShowCondition != null && data.CustomItemsNotShowCondition(uiItemCells[k].DisplayingItem))
			{
				uiItemCells[k].gameObject.SetActive(value: false);
			}
		}
		inventoryHeaderWidget.Draw(Data.InventoryHeaderWidgetData);
		UpdateItemRelatedWidgetStateForWidget();
		OnRedraw();
	}

	public override void Hide()
	{
		base.Hide();
		foreach (UIItemCell uiItemCell in uiItemCells)
		{
			uiItemCell.Flush();
			ReleaseCell(uiItemCell);
		}
		uiItemCells.Clear();
		ChangeMoveAllBtnState(isActive: false);
	}

	public override void UpdateItemRelatedWidgetStateForCells()
	{
		for (int i = 0; i < uiItemCells.Count; i++)
		{
			UIItemCell uIItemCell = uiItemCells[i];
			bool drawAsInteractable = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(uIItemCell.DisplayingItem);
			uIItemCell.SetWidgetState(GetCellRelatedWidgetState(uIItemCell.DisplayingItem, drawAsInteractable));
		}
	}

	private ItemRelatedWidgetState GetCellRelatedWidgetState(Item item, bool drawAsInteractable)
	{
		if (!drawAsInteractable)
		{
			return ItemRelatedWidgetState.Disabled;
		}
		if (item != null && !item.IsEmpty && data.CustomItemSelectedCondition != null && data.CustomItemSelectedCondition(item))
		{
			return ItemRelatedWidgetState.Selected;
		}
		return data.ItemRelatedWidgetState;
	}

	public override void UpdateItemRelatedWidgetStateForWidget()
	{
		if (data.ItemRelatedWidgetState == ItemRelatedWidgetState.Default || data.ItemRelatedWidgetState == ItemRelatedWidgetState.Selected)
		{
			background.sprite = backgroundActiveSprite;
			inventoryHeaderWidget.ChangeActiveViewState(isActive: true);
		}
		else
		{
			background.sprite = backgroundInactiveSprite;
			inventoryHeaderWidget.ChangeActiveViewState(isActive: false);
		}
	}

	protected override void ClearCallbacks()
	{
		UnsubscribeFromInventoryEvents();
		base.ClearCallbacks();
		foreach (UIItemCell uiItemCell in uiItemCells)
		{
			uiItemCell.ClearCallbacks();
		}
	}

	protected virtual UIItemCell GetNewCell()
	{
		return UIPrefabsPooler.Instance.GetElementFromPool<UIItemCell>(grid.transform);
	}

	protected virtual void ReleaseCell(UIItemCell cell)
	{
		UIPrefabsPooler.Instance.ReleaseElementToPool(cell);
	}

	private void Awake()
	{
		Init();
	}

	public virtual void UpdatePrices(ItemPriceDelegate priceDelegate, int countModificator)
	{
		if (priceDelegate == null)
		{
			return;
		}
		foreach (UIItemCell uiItemCell in uiItemCells)
		{
			if (uiItemCell.DisplayingItem == null || uiItemCell.DisplayingItem.IsEmpty)
			{
				uiItemCell.ClearPriceLabel();
			}
			else
			{
				uiItemCell.UpdatePriceLabel(priceDelegate(uiItemCell.DisplayingItem, countModificator));
			}
		}
	}

	public void UpdateHappinessStatusIcons(Vendor vendor, Func<string, int> getExtraSoldCount = null, float extraUsedHappiness = 0f)
	{
		foreach (UIItemCell cell in Cells)
		{
			int extraSoldCount = 0;
			if (getExtraSoldCount != null && cell.DisplayingItem != null)
			{
				extraSoldCount = getExtraSoldCount(cell.DisplayingItem.id);
			}
			cell.UpdateHappinessStatusIcons(vendor, extraSoldCount, extraUsedHappiness);
		}
	}

	public void OnRedraw()
	{
		if (!doNotTriggerOnRedraw)
		{
			this.onRedraw?.Invoke();
		}
	}

	public void ChangeMoveAllBtnState(bool isActive, Action onPress = null, Func<Inventory> getTargetInventory = null)
	{
		inventoryHeaderWidget.SetMoveAllSimilarBtnState(isActive, onPress, getTargetInventory);
	}

	private void OnWidgetPress()
	{
		if (Data.ItemRelatedWidgetState != ItemRelatedWidgetState.Disabled)
		{
			Data.OnWidgetPressed?.Invoke(this);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new InventoryWidgetDataBase(MainGame.PlayerData.Inventory, null, null, null, null, null));
	}
}
