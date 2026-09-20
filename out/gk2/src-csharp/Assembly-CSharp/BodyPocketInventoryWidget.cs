using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class BodyPocketInventoryWidget : InventoryWidgetBase<BodyPocketInventoryWidgetData>
{
	public const int SlotCount = 6;

	[SerializeField]
	private TextMeshProUGUI headerLabel;

	[SerializeField]
	private TextStyle activeStyle;

	[SerializeField]
	private TextStyle inactiveStyle;

	[SerializeField]
	private Sprite activeFilledBackSprite;

	[SerializeField]
	private Sprite activeEmptyCellBackSprite;

	[SerializeField]
	private Sprite inactiveCellBackSprite;

	private List<UIItemCell> cells = new List<UIItemCell>();

	private BodyPocketInventoryWidgetData Data => data as BodyPocketInventoryWidgetData;

	public override void Init()
	{
		base.Init();
		cells = GetComponentsInChildren<UIItemCell>(includeInactive: true).ToList();
	}

	public override void Redraw()
	{
		base.Redraw();
		SubscribeToInventoryEvents();
		foreach (UIItemCell cell in cells)
		{
			cell.OnItemCellOver = onItemCellOver;
			cell.OnItemCellOut = onItemCellOut;
			if (!Data.FirstEmptyIsInteractable)
			{
				cell.OnItemCellPress = onItemCellPress;
				cell.OnItemCellPress2 = onItemCellPress2;
			}
		}
		int num = 0;
		if (Data.IsActive)
		{
			foreach (Item item2 in data.Inventory.Data.Inventory)
			{
				if (!ShouldSkipItem(item2, Data.ZombieWgoData))
				{
					int num2 = Mathf.Min(item2.Count, cells.Count - num);
					for (int i = 0; i < num2; i++)
					{
						Item item = ((item2.Count == 1) ? item2 : Item.Copy(item2));
						item.Count = 1;
						cells[num].Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: false, 0, drawCounter: true, forceNonEmpty: false, forceDrawCounter: false, data.ItemRelatedWidgetState);
						cells[num].Background.sprite = activeFilledBackSprite;
						num++;
					}
					if (num >= cells.Count)
					{
						break;
					}
				}
			}
			for (int j = num; j < cells.Count; j++)
			{
				UIItemCell uIItemCell = cells[j];
				if (j == num && Data.FirstEmptyIsInteractable)
				{
					uIItemCell.DrawCustom("i_slot-plus", 1, interactable: true);
					uIItemCell.Background.sprite = activeFilledBackSprite;
					uIItemCell.OnItemCellPress = onItemCellPress;
					uIItemCell.OnItemCellPress2 = onItemCellPress2;
				}
				else
				{
					uIItemCell.DrawEmpty();
					uIItemCell.Background.sprite = activeEmptyCellBackSprite;
				}
			}
		}
		else
		{
			foreach (UIItemCell cell2 in cells)
			{
				cell2.DrawEmpty();
				cell2.Background.sprite = inactiveCellBackSprite;
			}
		}
		if (Data.IsActive)
		{
			activeStyle.ApplyStyle(headerLabel);
		}
		else
		{
			inactiveStyle.ApplyStyle(headerLabel);
		}
	}

	public static int GetOccupiedSlotCount(Item bodyItem, ZombieWgoData zombieWgoData)
	{
		if (bodyItem == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Item item in bodyItem.Inventory)
		{
			if (!ShouldSkipItem(item, zombieWgoData))
			{
				num += item.Count;
			}
		}
		return num;
	}

	private static bool ShouldSkipItem(Item item, ZombieWgoData zombieWgoData)
	{
		if (item.IsEmpty)
		{
			return true;
		}
		if (item.Definition.isMainOrgan)
		{
			return true;
		}
		if (item.Definition.itemGroupIds.Contains("burial_reward"))
		{
			return true;
		}
		if (zombieWgoData == null)
		{
			return false;
		}
		if (zombieWgoData.equippedArmor == item.UniqueId)
		{
			return true;
		}
		if (zombieWgoData.equippedHand == item.UniqueId)
		{
			return true;
		}
		if (zombieWgoData.equippedCollar == item.UniqueId)
		{
			return true;
		}
		return false;
	}

	public override void UpdateItemRelatedWidgetStateForCells()
	{
		for (int i = 0; i < cells.Count; i++)
		{
			UIItemCell uIItemCell = cells[i];
			bool flag = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(uIItemCell.DisplayingItem);
			uIItemCell.SetWidgetState((!flag) ? ItemRelatedWidgetState.Disabled : data.ItemRelatedWidgetState);
		}
	}

	protected override void ClearCallbacks()
	{
		UnsubscribeFromInventoryEvents();
		base.ClearCallbacks();
		foreach (UIItemCell cell in cells)
		{
			cell.Flush();
		}
	}

	protected override void TestDraw()
	{
	}
}
