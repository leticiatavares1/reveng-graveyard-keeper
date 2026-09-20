using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIAutopsyWindowData : LazyWidgetDataBase
{
	private Item bodyItem;

	private WgoData autopsyTable;

	private ZombieWgoData zombieWgoData;

	private bool isZombie;

	private ItemType selectedItemType;

	private MultiInventory currentMultiInventory;

	public bool IsEmpty { get; private set; }

	public UICorpseWidgetData CorpseWidgetData { get; private set; }

	public BodyOrgansInventoryWidgetData BodyOrgansInventoryWidgetData { get; private set; }

	public BodyPocketInventoryWidgetData BodyPocketInventoryWidgetData { get; private set; }

	public UIInfoWidgetData InfoWidgetData { get; set; }

	public UIAutopsyWindowData(WgoData wgoData)
	{
		autopsyTable = wgoData;
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
			if (isZombie)
			{
				Item collar = zombieWgoData.Collar;
				if (collar != null && !collar.IsEmpty && collar.Definition.redSkullsMaxCollar > collar.Definition.redSkullsMinCollar)
				{
					CorpseWidgetData.CollarRedSkullsLimit = collar.Definition.redSkullsMaxCollar;
				}
			}
			Inventory bodyItemInventory = new Inventory(bodyItem);
			BodyOrgansInventoryWidgetData = new BodyOrgansInventoryWidgetData(isActive: true, wgoData, MainGame.ZombieSystemData.GetZombie(bodyItem.UniqueId), bodyItemInventory, null, null, OnItemCellPressOrgans);
			BodyPocketInventoryWidgetData = new BodyPocketInventoryWidgetData(isActive: true, wgoData, MainGame.ZombieSystemData.GetZombie(bodyItem.UniqueId), bodyItemInventory, null, null, OnItemCellPressPocket);
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
				MainGame.Instance.dropSystem.DropItem(bodyItem, autopsyTable.WorldId, playerData.position.Value);
			}
			else
			{
				playerData.AddOverheadItem(bodyItem);
			}
			autopsyTable.Inventory.RemoveItemFromInventoryByUID(bodyItem);
			GameScene.GetWgoViewGlobal(autopsyTable.UniqueId)?.DrawWidgets();
			LazyUI.GetWindow<UIAutopsyWindow>().Close();
		}
	}

	private void OnItemCellPressPocket(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem != null && !itemCell.DisplayingItem.IsEmpty)
		{
			TryExtractItemFromPocket(itemCell);
		}
	}

	private void OnItemCellPressOrgans(UIItemCell itemCell)
	{
		selectedItemType = itemCell.GetComponentInParent<UIFixedTypeItemCell>().ItemType;
		if (itemCell.DisplayingItem == null)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, TryInsertMainOrgan, CanInsertOrgan);
			window.Open(uIMultiInventoryWindowData);
			currentMultiInventory = uIMultiInventoryWindowData.MultiInventory;
		}
		else if (!itemCell.DisplayingItem.Definition.isOrganMistake)
		{
			if (isZombie)
			{
				UIMultiInventoryWindow window2 = LazyUI.GetWindow<UIMultiInventoryWindow>();
				UIMultiInventoryWindowData uIMultiInventoryWindowData2 = new UIMultiInventoryWindowData(MainGame.PlayerData, TryChangeMainOrgan, CanChangeOrgan, addCurrentPlayerWorldZone: true, "change_organ", GetOrganChangeFailedTooltipLocId);
				window2.Open(uIMultiInventoryWindowData2);
				currentMultiInventory = uIMultiInventoryWindowData2.MultiInventory;
			}
			else
			{
				TryExtractMainOrgan(itemCell);
			}
		}
	}

	private bool CanInsertOrgan(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		bool flag = !isZombie || zombieWgoData.CanAddItemToBody(item);
		return item.Definition.type == selectedItemType && !item.Definition.isOrganMistake && flag;
	}

	private bool CanChangeOrgan(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		bool flag = zombieWgoData.CanChangeItemInBody(zombieWgoData.ZombieItem.GetItemByType(selectedItemType), item);
		return item.Definition.type == selectedItemType && !item.Definition.isOrganMistake && flag;
	}

	private string GetOrganChangeFailedTooltipLocId(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return null;
		}
		if (item.Definition.type != selectedItemType || item.Definition.isOrganMistake)
		{
			return null;
		}
		if (zombieWgoData.CanChangeItemInBody(zombieWgoData.ZombieItem.GetItemByType(selectedItemType), item))
		{
			return null;
		}
		return "ui_operation_failed_desc";
	}

	private void ShowZombieRelatedFailedOperationWindow()
	{
		UIDialogWindowData data = new UIDialogWindowData(LLBase.L("ui_operation_failed_header"), LLBase.L("ui_operation_failed_desc"), new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UIDialogWindow>().Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select));
		LazyUI.GetWindow<UIDialogWindow>().Open(data);
	}

	private void TryExtractItemFromPocket(UIItemCell itemCell)
	{
		Item item = itemCell.DisplayingItem;
		if (isZombie && !zombieWgoData.CanRemoveItemFromBody(item))
		{
			ShowZombieRelatedFailedOperationWindow();
			return;
		}
		CraftDef extractCraft = GameBalance.GetAutopsyCraftDef(AutopsyTypeCraft.PocketExtract);
		if (extractCraft == null)
		{
			Debug.LogError("Can not get extract organ craft for pockets [" + item.id + "]");
			return;
		}
		UIDialogWindowData data = new UIDialogWindowData(LLBase.L("ui_extract_item_header"), LLBase.L("ui_extract_item_desc"), OnCraftStartPressed, LazyUI.GetWindow<UIDialogWindow>().Close);
		LazyUI.GetWindow<UIDialogWindow>().Open(data);
		void OnCraftStartPressed()
		{
			autopsyTable.Inventory.RemoveItemsFromNestedItemById(extractCraft.destinationItemEnd, new List<NeedItemData>
			{
				new NeedItemData(item.id, 1)
			});
			CraftElement craftElement = new CraftElement(extractCraft.id, 1, new List<NeedItemData>(), new CraftParamsData(extractCraft.id, new GameRes()));
			craftElement.SetCustomItems(new List<Item> { item });
			autopsyTable.CraftComponent.AddToQueue(craftElement, addToQueueTop: true);
			autopsyTable.CraftComponent.TryContinueFromQueue();
			LazyUI.GetWindow<UIDialogWindow>().Close();
			LazyUI.GetWindow<UIAutopsyWindow>().Close();
		}
	}

	private void TryExtractMainOrgan(UIItemCell itemCell)
	{
		if (isZombie && !zombieWgoData.CanRemoveItemFromBody(itemCell.DisplayingItem))
		{
			ShowZombieRelatedFailedOperationWindow();
			return;
		}
		CraftDef extractCraft = GameBalance.GetAutopsyCraftDef(AutopsyTypeCraft.ExtractOrgan, itemCell.DisplayingItem.id);
		if (extractCraft == null)
		{
			Debug.LogError("Can not get extract organ craft for organ [" + itemCell.DisplayingItem.id + "]");
			return;
		}
		UICraftSelectionWindowData data = new UICraftSelectionWindowData(autopsyTable, extractCraft, null, OnCraftStartPressed);
		LazyUI.GetWindow<UICraftSelectionWindow>().Open(data);
		void OnCraftStartPressed(CraftDef craftDefinition, List<NeedItemData> selectedNeedItems, CraftParamsData craftParams, int craftsCount = 1)
		{
			CraftElement craftElement = new CraftElement(extractCraft.id, craftsCount, selectedNeedItems, craftParams);
			autopsyTable.CraftComponent.AddToQueue(craftElement, addToQueueTop: true);
			autopsyTable.CraftComponent.TryContinueFromQueue();
			LazyUI.GetWindow<UIAutopsyWindow>().Close();
		}
	}

	private void TryInsertMainOrgan(UIItemCell itemCell)
	{
		Item item = itemCell.DisplayingItem;
		if (isZombie && !zombieWgoData.CanAddItemToBody(item))
		{
			LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
			ShowZombieRelatedFailedOperationWindow();
			return;
		}
		CraftDef insertionCraft = GameBalance.GetAutopsyCraftDef(AutopsyTypeCraft.InsertOrgan, item.id);
		if (insertionCraft == null)
		{
			Debug.LogError($"Can not get insertion organ craft for organ type [{selectedItemType}]");
			return;
		}
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		UICraftSelectionWindowData data = new UICraftSelectionWindowData(autopsyTable, insertionCraft, null, OnCraftStartPressed);
		LazyUI.GetWindow<UICraftSelectionWindow>().Open(data);
		void OnCraftStartPressed(CraftDef craftDefinition, List<NeedItemData> selectedNeedItems, CraftParamsData craftParams, int craftsCount = 1)
		{
			CraftElement craftElement = new CraftElement(insertionCraft.id, craftsCount, selectedNeedItems, craftParams);
			currentMultiInventory.RemoveItemFromInventoryByUID(item);
			autopsyTable.CraftComponent.AddToQueue(craftElement, addToQueueTop: true);
			autopsyTable.CraftComponent.TryContinueFromQueue();
			LazyUI.GetWindow<UIAutopsyWindow>().Close();
		}
	}

	private void TryChangeMainOrgan(UIItemCell itemCell)
	{
		Item item = itemCell.DisplayingItem;
		if (isZombie && !zombieWgoData.CanChangeItemInBody(zombieWgoData.ZombieItem.GetItemByType(selectedItemType), item))
		{
			LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
			ShowZombieRelatedFailedOperationWindow();
			return;
		}
		CraftDef changeCraft = GameBalance.GetAutopsyCraftDef(AutopsyTypeCraft.ChangeOrgan, item.id);
		if (changeCraft == null)
		{
			Debug.LogError($"Can not get change organ craft for organ type [{selectedItemType}]");
			return;
		}
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		UICraftSelectionWindowData data = new UICraftSelectionWindowData(autopsyTable, changeCraft, null, OnCraftStartPressed);
		LazyUI.GetWindow<UICraftSelectionWindow>().Open(data);
		void OnCraftStartPressed(CraftDef craftDefinition, List<NeedItemData> selectedNeedItems, CraftParamsData craftParams, int craftsCount = 1)
		{
			craftParams.selectedOrganTypeForChange = selectedItemType;
			CraftElement craftElement = new CraftElement(changeCraft.id, craftsCount, selectedNeedItems, craftParams);
			craftElement.SetCustomItems(new List<Item> { zombieWgoData.ZombieItem.GetItemByType(selectedItemType) });
			currentMultiInventory.RemoveItemFromInventoryByUID(item);
			autopsyTable.CraftComponent.AddToQueue(craftElement, addToQueueTop: true);
			autopsyTable.CraftComponent.TryContinueFromQueue();
			LazyUI.GetWindow<UIAutopsyWindow>().Close();
		}
	}
}
