using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolBeltInventoryWidget : InventoryWidgetBase<ToolBeltInventoryWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI armorLabel;

	[SerializeField]
	private TextMeshProUGUI swordLabel;

	[SerializeField]
	private TextMeshProUGUI bowLabel;

	private List<UIFixedTypeItemCell> fixedTypeItemCells = new List<UIFixedTypeItemCell>();

	private Dictionary<ItemType, UIFixedTypeItemCell> itemCellsByType = new Dictionary<ItemType, UIFixedTypeItemCell>();

	public override void Init()
	{
		base.Init();
		fixedTypeItemCells = GetComponentsInChildren<UIFixedTypeItemCell>(includeInactive: true).ToList();
		foreach (UIFixedTypeItemCell fixedTypeItemCell in fixedTypeItemCells)
		{
			if (!itemCellsByType.TryAdd(fixedTypeItemCell.ItemType, fixedTypeItemCell) && fixedTypeItemCell.ItemType != 0)
			{
				Debug.LogError($"ToolBeltInventoryWidget: itemType {fixedTypeItemCell.ItemType} was already added");
			}
		}
	}

	protected override void ClearCallbacks()
	{
		UnsubscribeFromInventoryEvents();
		base.ClearCallbacks();
		foreach (UIFixedTypeItemCell fixedTypeItemCell in fixedTypeItemCells)
		{
			fixedTypeItemCell.UIItemCell.ClearCallbacks();
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		SubscribeToInventoryEvents();
		foreach (UIFixedTypeItemCell fixedTypeItemCell in fixedTypeItemCells)
		{
			fixedTypeItemCell.DrawEmpty();
			fixedTypeItemCell.UIItemCell.OnItemCellOver = onItemCellOver;
			fixedTypeItemCell.UIItemCell.OnItemCellOut = onItemCellOut;
			fixedTypeItemCell.UIItemCell.OnItemCellPress = onItemCellPress;
			fixedTypeItemCell.UIItemCell.OnItemCellPress2 = onItemCellPress2;
		}
		foreach (Item item in data.Inventory.Data.Inventory)
		{
			if (itemCellsByType.TryGetValue(item.Definition.type, out var value))
			{
				bool drawAsInteractable = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(item);
				value.Draw(item, data.ItemRelatedWidgetState, drawAsInteractable);
			}
		}
		Item itemByType = data.Inventory.GetItemByType(ItemType.BodyArmor);
		Item itemByType2 = data.Inventory.GetItemByType(ItemType.Sword);
		Item itemByType3 = data.Inventory.GetItemByType(ItemType.Bow);
		int num = ((!itemByType.IsEmpty) ? itemByType.Definition.quality : 0);
		int num2 = ((!itemByType2.IsEmpty) ? itemByType2.Definition.damage.EvaluateInt(MainGame.PlayerController?.PhysicalBody) : 0);
		int num3 = ((!itemByType3.IsEmpty) ? itemByType3.Definition.damage.EvaluateInt(MainGame.PlayerController?.PhysicalBody) : 0);
		armorLabel.text = string.Format("{0}{1}", "equip_icon_armor".FontIcon(), num);
		swordLabel.text = string.Format("{0}{1}", "equip_icon_sword".FontIcon(), num2);
		bowLabel.text = string.Format("{0}{1}", "equip_icon_arrow".FontIcon(), num3);
	}

	public override void UpdateItemRelatedWidgetStateForCells()
	{
		for (int i = 0; i < fixedTypeItemCells.Count; i++)
		{
			UIFixedTypeItemCell uIFixedTypeItemCell = fixedTypeItemCells[i];
			bool flag = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(uIFixedTypeItemCell.UIItemCell.DisplayingItem);
			uIFixedTypeItemCell.UIItemCell.SetWidgetState((!flag) ? ItemRelatedWidgetState.Disabled : data.ItemRelatedWidgetState);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new InventoryWidgetDataBase(MainGame.PlayerData.toolBeltInventory, null, null, new PlayerInventoryUIItemOpHandler(MainGame.PlayerData).TryUnEquipItem, null, null));
	}
}
