using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITooltipCraftItemCell : MonoBehaviour
{
	private Action onVariableItemChanged;

	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private LazyButton nextItemButton;

	[SerializeField]
	private LazyButton prevItemButton;

	[SerializeField]
	private TextMeshProUGUI runesLabel;

	[SerializeField]
	private Vector2 defaultLayoutSize;

	[SerializeField]
	private Vector2 bigLayoutSize;

	[SerializeField]
	private LayoutElement layoutElement;

	private bool subscribedToInventoryChanges;

	private int currentMultiplier = 1;

	private UICraftItemCellData data;

	private bool isBig;

	private bool drawAsNeedItem = true;

	private GamepadNavigationItem gamepadNavigationItem;

	public LazyButton NextItemButton => nextItemButton;

	public LazyButton PrevItemButton => prevItemButton;

	public UIItemCell ItemCell => itemCell;

	public GamepadNavigationItem GamepadNavigationItem
	{
		get
		{
			TryInitGamepadNavigationItem();
			return gamepadNavigationItem;
		}
	}

	private void Awake()
	{
		nextItemButton.onClick.AddListener(OnNextItem);
		prevItemButton.onClick.AddListener(OnPrevItem);
		TryInitGamepadNavigationItem();
	}

	public void Draw(UICraftItemCellData craftItemCellData, Action onVariableItemChanged, bool isTooltipView = false, bool drawAsNeedItem = true)
	{
		data = craftItemCellData;
		this.drawAsNeedItem = drawAsNeedItem;
		this.onVariableItemChanged = onVariableItemChanged;
		SubscribeToInventoryChanges();
		if (craftItemCellData.itemVariants.Count > 1 && !isTooltipView)
		{
			nextItemButton.gameObject.SetActive(value: true);
			prevItemButton.gameObject.SetActive(value: true);
		}
		else
		{
			nextItemButton.gameObject.SetActive(value: false);
			prevItemButton.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
		runesLabel.text = data.runesStr;
		runesLabel.gameObject.SetActive(!string.IsNullOrEmpty(data.runesStr));
		RedrawItem();
		isBig = data.currentItem.ItemDef != null && data.currentItem.ItemDef.itemSize == ItemSize.Big;
		UpdateSize(isBig);
	}

	public void Flush()
	{
		currentMultiplier = 1;
		drawAsNeedItem = true;
		onVariableItemChanged = null;
		nextItemButton.gameObject.SetActive(value: false);
		prevItemButton.gameObject.SetActive(value: false);
		UnsubscribeFromInventoryChanges();
		if (isBig)
		{
			UpdateSize(big: false);
		}
	}

	private void UpdateSize(bool big)
	{
		if (big)
		{
			layoutElement.minWidth = bigLayoutSize.x;
			layoutElement.minHeight = bigLayoutSize.y;
		}
		else
		{
			layoutElement.minWidth = defaultLayoutSize.x;
			layoutElement.minHeight = defaultLayoutSize.y;
		}
	}

	private void RedrawItem(List<Item> items = null)
	{
		if (!drawAsNeedItem)
		{
			itemCell.Draw(new Item(data.currentItem.Id, data.currentItem.GetCount(data.WgoData)), isNeedItem: false, -1, isCraftResult: false, currentMultiplier, drawAsNonInteractable: false, 0, drawCounter: true, forceNonEmpty: false, forceDrawCounter: true);
			itemCell.UpdateCountLabelAsRegularItem(currentMultiplier);
		}
		else if (data.needDurability > 0f)
		{
			itemCell.Draw(new Item(data.currentItem.Id, data.currentItem.GetCount(data.WgoData)), isNeedItem: true, (!data.MultiInventory.HasItemWithEnoughDurability(data.currentItem.Id, data.needDurability)) ? 1 : 0, isCraftResult: false, currentMultiplier);
		}
		else
		{
			itemCell.Draw(new Item(data.currentItem.Id, data.currentItem.GetCount(data.WgoData)), isNeedItem: true, data.MultiInventory.GetTotalCount(data.currentItem.Id), isCraftResult: false, currentMultiplier);
		}
		if (data.itemVariants.Count > 1)
		{
			itemCell.StarIcon.gameObject.SetActive(value: false);
		}
		itemCell.SetNativeSizeForIcon();
	}

	private void OnNextItem()
	{
		data.NextItem();
		RedrawItem();
		onVariableItemChanged?.Invoke();
	}

	private void OnPrevItem()
	{
		data.PrevItem();
		RedrawItem();
		onVariableItemChanged?.Invoke();
	}

	private void SubscribeToInventoryChanges()
	{
		if (!subscribedToInventoryChanges)
		{
			MainGame.PlayerData.inventory.OnItemsAdd += RedrawItem;
			MainGame.PlayerData.inventory.OnItemsRemove += RedrawItem;
			subscribedToInventoryChanges = true;
		}
	}

	private void UnsubscribeFromInventoryChanges()
	{
		if (subscribedToInventoryChanges)
		{
			MainGame.PlayerData.inventory.OnItemsAdd -= RedrawItem;
			MainGame.PlayerData.inventory.OnItemsRemove -= RedrawItem;
			subscribedToInventoryChanges = false;
		}
	}

	private void TryInitGamepadNavigationItem()
	{
		if (gamepadNavigationItem == null)
		{
			gamepadNavigationItem = GetComponent<GamepadNavigationItem>();
			if (gamepadNavigationItem != null)
			{
				gamepadNavigationItem.SetCallbacks(itemCell.OnGamepadOver, itemCell.OnGamepadOut, itemCell.OnGamepadPress);
			}
		}
	}

	public void SetMultiplierValue(int multiplier)
	{
		currentMultiplier = multiplier;
		itemCell.OnMultiplierChange(currentMultiplier);
	}
}
