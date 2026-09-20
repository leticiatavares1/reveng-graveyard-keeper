using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZombieEquipmentInventoryWidget : InventoryWidgetBase<ZombieEquipmentInventoryWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI armorLabel;

	[SerializeField]
	private TextMeshProUGUI handLabel;

	[SerializeField]
	private UIFixedTypeItemCell collarCell;

	[SerializeField]
	private UIFixedTypeItemCell armorCell;

	[SerializeField]
	private UIGroupsItemCell handCell;

	private ZombieEquipmentInventoryWidgetData Data => data as ZombieEquipmentInventoryWidgetData;

	public override void Redraw()
	{
		base.Redraw();
		SubscribeToInventoryEvents();
		armorCell.DrawEmptyInteractable();
		collarCell.DrawEmptyInteractable();
		handCell.DrawEmptyInteractable();
		int num = 0;
		int num2 = 0;
		if (!Data.ZombieWgoData.Armor.IsEmpty)
		{
			armorCell.Draw(Data.ZombieWgoData.Armor);
			num = Data.ZombieWgoData.ArmorValue;
		}
		if (!Data.ZombieWgoData.Collar.IsEmpty)
		{
			collarCell.Draw(Data.ZombieWgoData.Collar);
			collarCell.UIItemCell.OnItemCellPress = Data.OnCollarCellPressed;
		}
		if (!Data.ZombieWgoData.Hand.IsEmpty)
		{
			handCell.Draw(Data.ZombieWgoData.Hand);
			num2 = Data.ZombieWgoData.AttackValue;
		}
		handCell.UIItemCell.OnItemCellPress = Data.OnInstrumentCellPressed;
		handCell.UIItemCell.OnItemCellPress2 = Data.OnInstrumentCellPressed2;
		armorCell.UIItemCell.OnItemCellPress = Data.OnArmorCellPressed;
		armorCell.UIItemCell.OnItemCellPress2 = Data.OnArmorCellPressed2;
		armorLabel.text = string.Format("{0}{1}", "equip_icon_armor".FontIcon(), num);
		handLabel.text = string.Format("{0}{1}", "equip_icon_sword".FontIcon(), num2);
	}

	protected override void ClearCallbacks()
	{
		UnsubscribeFromInventoryEvents();
		base.ClearCallbacks();
	}

	public override void UpdateItemRelatedWidgetStateForCells()
	{
		armorCell.UIItemCell.SetWidgetState(data.ItemRelatedWidgetState);
		handCell.UIItemCell.SetWidgetState(data.ItemRelatedWidgetState);
	}

	protected override void TestDraw()
	{
	}
}
