using System;
using LazyBearTechnology;

public class UIBaseChestWindowData : LazyWidgetDataBase
{
	private Inventory playerInventory;

	private Inventory wgoInventory;

	public WgoData Chest { get; private set; }

	public MultiInventoryWidgetData FirstMultiInventoryData { get; private set; }

	public MultiInventoryWidgetData SecondMultiInventoryData { get; private set; }

	public Action OnMoveAllSimilarItemFromPlayerToChest { get; private set; }

	public MoneyWidgetData MoneyWidgetData { get; private set; }

	public UIBaseChestWindowData(Inventory playerInventory, MultiInventory worldZoneMultiInventory, WgoData wgoData)
	{
		UIBaseChestWindowData uIBaseChestWindowData = this;
		Chest = wgoData;
		this.playerInventory = playerInventory;
		wgoInventory = wgoData.Inventory;
		FirstMultiInventoryData = new MultiInventoryWidgetData(MultiInventoryWidgetMode.SelectionWithMoveBtn);
		SecondMultiInventoryData = new MultiInventoryWidgetData(MultiInventoryWidgetMode.SelectionWithoutMoveBtn);
		InventoryUIItemMoveOpHandler moveOpHandler = new InventoryUIItemMoveOpHandler(() => uIBaseChestWindowData.FirstMultiInventoryData.SelectedWidgetData.Inventory, () => uIBaseChestWindowData.SecondMultiInventoryData.SelectedWidgetData.Inventory);
		FirstMultiInventoryData.AddRange(InventoryWidgetDataHelper.GetWidgetsDataForInventory(playerInventory, delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, moveOpHandler.OnInventory1ItemPress1, moveOpHandler.OnInventory1ItemPress2, null, ItemAvailabilityConditionLeft, addBags: true, disableHeaderForFirstWidget: false, ItemRelatedWidgetState.Default, ItemRelatedWidgetState.Inactive));
		if (worldZoneMultiInventory != null)
		{
			FirstMultiInventoryData.AddRange(InventoryWidgetDataHelper.GetWidgetsDataForMultiInventory(worldZoneMultiInventory, delegate
			{
				LazyAudio.PlayAndForget("gui_hover_light");
			}, null, null, null, null, (Item _) => false, addBags: true, disableHeaderForFirstWidget: false, ItemRelatedWidgetState.Disabled, ItemRelatedWidgetState.Disabled));
		}
		SecondMultiInventoryData.AddRange(InventoryWidgetDataHelper.GetWidgetsDataForInventory(wgoInventory, delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, moveOpHandler.OnInventory2ItemPress1, moveOpHandler.OnInventory2ItemPress2, null, ItemAvailabilityConditionRight));
		if (SecondMultiInventoryData.inventoriesData[0] is InventoryWidgetData inventoryWidgetData)
		{
			inventoryWidgetData.InventoryHeaderWidgetData.HeaderIconId = "comm-header_2-type_icon-simple_chest";
		}
		OnMoveAllSimilarItemFromPlayerToChest = delegate
		{
			uIBaseChestWindowData.SecondMultiInventoryData.SelectedWidgetData.Inventory.TakeAllItemsExistingInMeFromOtherInventory(uIBaseChestWindowData.FirstMultiInventoryData.SelectedWidgetData.Inventory);
		};
		FirstMultiInventoryData.OnMoveAllSimilarPressed = OnMoveAllSimilarItemFromPlayerToChest;
		FirstMultiInventoryData.GetMoveAllSimilarTargetInventory = () => uIBaseChestWindowData.SecondMultiInventoryData.SelectedWidgetData?.Inventory;
		SecondMultiInventoryData.OnSelectedWidgetChanged = delegate
		{
			uIBaseChestWindowData.FirstMultiInventoryData.OnMoveAllSimilarTargetChanged?.Invoke();
		};
		playerInventory.OnBagRemoved += FirstMultiInventoryData.OnBagRemoved;
		wgoInventory.OnBagRemoved += SecondMultiInventoryData.OnBagRemoved;
		playerInventory.OnBagAdded += OnBagAddedToPlayer;
		wgoInventory.OnBagAdded += OnBagAddedToWgo;
		FirstMultiInventoryData.onWidgetHide = delegate
		{
			playerInventory.OnBagRemoved -= uIBaseChestWindowData.FirstMultiInventoryData.OnBagRemoved;
			playerInventory.OnBagAdded -= OnBagAddedToPlayer;
		};
		SecondMultiInventoryData.onWidgetHide = delegate
		{
			uIBaseChestWindowData.wgoInventory.OnBagRemoved -= uIBaseChestWindowData.SecondMultiInventoryData.OnBagRemoved;
			uIBaseChestWindowData.wgoInventory.OnBagAdded -= OnBagAddedToWgo;
		};
		MoneyWidgetData = new MoneyWidgetData();
		MoneyWidgetData.Money = () => MainGame.PlayerData.GetResInt("money");
		void OnBagAddedToPlayer(Item bag)
		{
			uIBaseChestWindowData.OnBagAdded(bag, uIBaseChestWindowData.FirstMultiInventoryData, playerInventory, delegate
			{
				LazyAudio.PlayAndForget("gui_hover_light");
			}, null, moveOpHandler.OnInventory1ItemPress1, moveOpHandler.OnInventory1ItemPress2);
		}
		void OnBagAddedToWgo(Item bag)
		{
			uIBaseChestWindowData.OnBagAdded(bag, uIBaseChestWindowData.SecondMultiInventoryData, uIBaseChestWindowData.wgoInventory, delegate
			{
				LazyAudio.PlayAndForget("gui_hover_light");
			}, null, moveOpHandler.OnInventory2ItemPress1, moveOpHandler.OnInventory2ItemPress2);
		}
	}

	private bool ItemAvailabilityConditionLeft(Item item)
	{
		if (item == null)
		{
			return true;
		}
		if (item.Definition.isBag && SecondMultiInventoryData.SelectedWidgetData.Inventory.Data.IsBag)
		{
			return false;
		}
		if (wgoInventory.Data.TryGetProperty<BlackListFilterSerializedItemProperty>(out var property) && property.BlackList.Contains(item.Definition))
		{
			return false;
		}
		if (wgoInventory.Data.TryGetProperty<WhiteListFilterSerializedItemProperty>(out var property2) && !property2.WhiteList.IsEmpty && !property2.WhiteList.Contains(item.Definition))
		{
			return false;
		}
		return true;
	}

	private bool ItemAvailabilityConditionRight(Item item)
	{
		if (item == null)
		{
			return true;
		}
		if (item.Definition.isBag && FirstMultiInventoryData.SelectedWidgetData.Inventory.Data.IsBag)
		{
			return false;
		}
		return true;
	}

	private void OnBagAdded(Item bag, MultiInventoryWidgetData targetData, Inventory parentInventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Func<Item, bool> itemsAvailableCondition = null)
	{
		Inventory inventoryFromBag = Inventory.GetInventoryFromBag(bag, parentInventory);
		BagInventoryWidgetData bagInventoryWidgetData = new BagInventoryWidgetData(inventoryFromBag, new InventoryHeaderWidgetData(inventoryFromBag, "comm-header_2-type_icon-simple_bag", inventoryFromBag.Data.id), onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, null, itemsAvailableCondition);
		bagInventoryWidgetData.ParentInventory = parentInventory;
		bagInventoryWidgetData.ItemRelatedWidgetState = ItemRelatedWidgetState.Inactive;
		targetData.OnBagAdded(bagInventoryWidgetData);
	}
}
