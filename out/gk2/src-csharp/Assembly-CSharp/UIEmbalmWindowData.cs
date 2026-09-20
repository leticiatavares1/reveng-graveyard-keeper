using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIEmbalmWindowData : LazyWidgetDataBase
{
	private Item bodyItem;

	private WgoData table;

	private ZombieWgoData zombieWgoData;

	private bool isZombie;

	private MultiInventory currentMultiInventory;

	public bool IsEmpty { get; private set; }

	public UICorpseWidgetData CorpseWidgetData { get; private set; }

	public BodyOrgansInventoryWidgetData BodyOrgansInventoryWidgetData { get; private set; }

	public BodyPocketInventoryWidgetData BodyPocketInventoryWidgetData { get; private set; }

	public UIInfoWidgetData InfoWidgetData { get; set; }

	public UIEmbalmWindowData(WgoData wgoData)
	{
		table = wgoData;
		foreach (Item item in wgoData.Inventory.Data.Inventory)
		{
			if (item.Definition.itemGroupIds.Contains("body"))
			{
				bodyItem = item;
				zombieWgoData = MainGame.ZombieSystemData.GetZombie(bodyItem.UniqueId);
				isZombie = zombieWgoData != null;
				break;
			}
		}
		IsEmpty = bodyItem == null;
		InfoWidgetData = new UIInfoWidgetData(wgoData);
		if (!IsEmpty)
		{
			CorpseWidgetData = new UICorpseWidgetData(bodyItem, wgoData, TakeBody, !IsEmpty, GameKey.ExtractBody, LLBase.L("ui_grave_corpse_widget_header"), null, LLBase.L("btn_take_body_two_lines"));
			Inventory bodyItemInventory = new Inventory(bodyItem);
			BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData(isActive: true, wgoData, MainGame.ZombieSystemData.GetZombie(bodyItem.UniqueId), bodyItemInventory, null, null);
			BodyPocketInventoryWidgetData = new BodyPocketInventoryWidgetData(isActive: true, wgoData, MainGame.ZombieSystemData.GetZombie(bodyItem.UniqueId), bodyItemInventory, null, null, OnItemCellPressPocket);
			BodyPocketInventoryWidgetData.FirstEmptyIsInteractable = true;
		}
		else
		{
			CorpseWidgetData = new UICorpseWidgetData(GameKey.ExtractBody);
			BodyPocketInventoryWidgetData = new BodyPocketInventoryWidgetData();
			BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData();
		}
	}

	private void TakeBody()
	{
		if (!IsEmpty)
		{
			PlayerData playerData = MainGame.PlayerData;
			if (!playerData.HasFreeOverheadSlot)
			{
				MainGame.Instance.dropSystem.DropItem(bodyItem, table.WorldId, table.Position);
			}
			else
			{
				playerData.AddOverheadItem(bodyItem);
			}
			table.Inventory.RemoveItemFromInventoryByUID(bodyItem);
			GameScene.GetWgoViewGlobal(table.UniqueId)?.DrawWidgets();
			LazyUI.GetWindow<UIEmbalmWindow>().Close();
		}
	}

	private void OnItemCellPressPocket(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem == null || itemCell.DisplayingItem.IsEmpty)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, TryEmbalm, CanEmbalm);
			window.Open(uIMultiInventoryWindowData);
			currentMultiInventory = uIMultiInventoryWindowData.MultiInventory;
		}
	}

	private void TryEmbalm(UIItemCell itemCell)
	{
		Item displayingItem = itemCell.DisplayingItem;
		if (CanEmbalm(displayingItem))
		{
			CraftDef autopsyCraftDef = GameBalance.GetAutopsyCraftDef(AutopsyTypeCraft.Embalm, displayingItem.id);
			if (autopsyCraftDef == null)
			{
				Debug.LogError($"Can not get insertion craft for embalm item [{displayingItem}]");
				return;
			}
			Item item = new Item(displayingItem.id);
			CraftElement craftElement = new CraftElement(autopsyCraftDef.id, 1, new List<NeedItemData>(), new CraftParamsData(autopsyCraftDef.id, new GameRes()));
			craftElement.SetCustomItems(new List<Item> { item });
			table.CraftComponent.TryStartCraft(craftElement);
			currentMultiInventory.RemoveItemFromInventoryByUID(displayingItem, 1);
			LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
			LazyUI.GetWindow<UIEmbalmWindow>().Close();
		}
	}

	private bool CanEmbalm(Item item)
	{
		if (item == null || item.IsEmpty || item.Definition.type != ItemType.Embalm)
		{
			return false;
		}
		if (bodyItem == null)
		{
			return false;
		}
		if (BodyPocketInventoryWidget.GetOccupiedSlotCount(bodyItem, zombieWgoData) >= 6)
		{
			return false;
		}
		if (isZombie)
		{
			return zombieWgoData.CanAddItemToBody(item);
		}
		return true;
	}
}
