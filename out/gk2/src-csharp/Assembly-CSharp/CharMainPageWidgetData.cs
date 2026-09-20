using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class CharMainPageWidgetData : LazyWidgetDataBase
{
	private PlayerInventoryUIItemOpHandler playerInventoryUIItemOpHandler;

	public PlayerData PlayerData { get; private set; }

	public MultiInventoryWidgetData MultiInventoryWidgetData { get; private set; }

	public ToolBeltInventoryWidgetData ToolBeltInventoryWidgetData { get; private set; }

	public PerksWidgetData PerksWidgetData { get; private set; }

	public PerksWidgetData BuffsWidgetData { get; private set; }

	public MoneyWidgetData MoneyWidgetData { get; private set; }

	public BagInventoryWidgetData BagInventoryWidgetData => playerInventoryUIItemOpHandler.BagInventoryWidgetData;

	public bool IsBagShown => playerInventoryUIItemOpHandler.IsBagShown;

	public Action OnMoveAllSimilarItemFromPlayerToBag { get; private set; }

	public Action<Item> OnBagHide
	{
		get
		{
			return playerInventoryUIItemOpHandler.OnBagHide;
		}
		set
		{
			playerInventoryUIItemOpHandler.OnBagHide = value;
		}
	}

	public Action<Item> OnBagShow
	{
		get
		{
			return playerInventoryUIItemOpHandler.OnBagShow;
		}
		set
		{
			playerInventoryUIItemOpHandler.OnBagShow = value;
		}
	}

	public Action OnHideBagPressed { get; private set; }

	public CharMainPageWidgetData(GameSave gameSave)
	{
		CharMainPageWidgetData charMainPageWidgetData = this;
		PlayerData = gameSave.playerData;
		MultiInventoryWidgetData = new MultiInventoryWidgetData();
		playerInventoryUIItemOpHandler = new PlayerInventoryUIItemOpHandler(gameSave.playerData, MultiInventoryWidgetData);
		ToolBeltInventoryWidgetData = new ToolBeltInventoryWidgetData(gameSave.playerData.toolBeltInventory, delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, playerInventoryUIItemOpHandler.TryUnEquipItem, playerInventoryUIItemOpHandler.TryUnEquipItem, null, playerInventoryUIItemOpHandler.ToolBeltItemsAvailabilityCondition);
		List<InventoryWidgetDataBase> widgetsDataForInventory = InventoryWidgetDataHelper.GetWidgetsDataForInventory(gameSave.playerData.inventory, delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, playerInventoryUIItemOpHandler.OnPlayerInvItemPressed, playerInventoryUIItemOpHandler.OnPlayerInvItemPressed2, playerInventoryUIItemOpHandler.OnPlayerInventoryPressedDown, playerInventoryUIItemOpHandler.PlayerItemsAvailabilityCondition);
		if (widgetsDataForInventory.Count > 0)
		{
			widgetsDataForInventory[0].CustomItemSelectedCondition = playerInventoryUIItemOpHandler.IsShownBag;
		}
		MultiInventoryWidgetData.AddRange(widgetsDataForInventory);
		OnHideBagPressed = playerInventoryUIItemOpHandler.HideBag;
		OnMoveAllSimilarItemFromPlayerToBag = delegate
		{
			charMainPageWidgetData.BagInventoryWidgetData.Inventory.TakeAllItemsExistingInMeFromOtherInventory(charMainPageWidgetData.MultiInventoryWidgetData.SelectedWidgetData.Inventory);
		};
		MultiInventoryWidgetData.GetMoveAllSimilarTargetInventory = () => charMainPageWidgetData.BagInventoryWidgetData?.Inventory;
		if (gameSave.playerData.CurrentWorldZoneData != null)
		{
			MultiInventoryWidgetData.AddRange(InventoryWidgetDataHelper.GetWidgetsDataForMultiInventory(new MultiInventory(gameSave.playerData.CurrentWorldZoneData), null, null, null, null, null, playerInventoryUIItemOpHandler.PlayerItemsAvailabilityCondition, addBags: true, disableHeaderForFirstWidget: false, ItemRelatedWidgetState.Disabled, ItemRelatedWidgetState.Disabled));
		}
		PerksWidgetData = new PerksWidgetData(gameSave, new List<PerkType> { PerkType.Default });
		BuffsWidgetData = new PerksWidgetData(gameSave, new List<PerkType> { PerkType.Buff });
		MoneyWidgetData = new MoneyWidgetData();
		MoneyWidgetData.Money = () => gameSave.playerData.GetResInt("money");
	}

	public void HideBag()
	{
		playerInventoryUIItemOpHandler?.HideBag();
	}
}
