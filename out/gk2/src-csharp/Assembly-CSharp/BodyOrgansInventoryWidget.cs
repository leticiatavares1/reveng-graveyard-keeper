using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using TMPro;
using UnityEngine;

public class BodyOrgansInventoryWidget : InventoryWidgetBase<BodyOrgansInventoryWidgetData>
{
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

	private List<UIFixedTypeItemCell> mainOrgansFixedTypeItemCells;

	private Dictionary<ItemType, UIFixedTypeItemCell> mainOrgansItemCellsByType = new Dictionary<ItemType, UIFixedTypeItemCell>();

	private BodyOrgansInventoryWidgetData Data => data as BodyOrgansInventoryWidgetData;

	public override void Init()
	{
		base.Init();
		mainOrgansFixedTypeItemCells = GetComponentsInChildren<UIFixedTypeItemCell>(includeInactive: true).ToList();
		foreach (UIFixedTypeItemCell mainOrgansFixedTypeItemCell in mainOrgansFixedTypeItemCells)
		{
			if (!mainOrgansItemCellsByType.TryAdd(mainOrgansFixedTypeItemCell.ItemType, mainOrgansFixedTypeItemCell) && mainOrgansFixedTypeItemCell.ItemType != 0)
			{
				Debug.LogError($"AutopsyInventoryWidget: itemType {mainOrgansFixedTypeItemCell.ItemType} was already added");
			}
		}
	}

	protected override void ClearCallbacks()
	{
		UnsubscribeFromInventoryEvents();
		base.ClearCallbacks();
		foreach (UIFixedTypeItemCell mainOrgansFixedTypeItemCell in mainOrgansFixedTypeItemCells)
		{
			mainOrgansFixedTypeItemCell.UIItemCell.ClearCallbacks();
		}
	}

	public override void UpdateItemRelatedWidgetStateForCells()
	{
		for (int i = 0; i < mainOrgansFixedTypeItemCells.Count; i++)
		{
			UIFixedTypeItemCell uIFixedTypeItemCell = mainOrgansFixedTypeItemCells[i];
			bool flag = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(uIFixedTypeItemCell.UIItemCell.DisplayingItem);
			uIFixedTypeItemCell.UIItemCell.SetWidgetState((!flag) ? ItemRelatedWidgetState.Disabled : data.ItemRelatedWidgetState);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		SubscribeToInventoryEvents();
		if (Data.IsActive)
		{
			for (int i = 0; i < LazyConsts.MAIN_ORGANS_TYPES.Count; i++)
			{
				ItemType itemType = LazyConsts.MAIN_ORGANS_TYPES[i];
				if (mainOrgansItemCellsByType.TryGetValue(itemType, out var value))
				{
					if (data.Inventory.Data.HasItemsByItemType(itemType))
					{
						Item itemByType = data.Inventory.Data.GetItemByType(itemType);
						value.Draw(itemByType, data.ItemRelatedWidgetState);
						value.UpdateWidgetBackgroundSprite(activeFilledBackSprite);
					}
					else
					{
						value.DrawEmpty();
						value.UpdateWidgetBackgroundSprite(activeEmptyCellBackSprite);
					}
					value.UIItemCell.OnItemCellOver = onItemCellOver;
					value.UIItemCell.OnItemCellOut = onItemCellOut;
					value.UIItemCell.OnItemCellPress = onItemCellPress;
					value.UIItemCell.OnItemCellPress2 = onItemCellPress2;
				}
			}
		}
		else
		{
			foreach (UIFixedTypeItemCell mainOrgansFixedTypeItemCell in mainOrgansFixedTypeItemCells)
			{
				mainOrgansFixedTypeItemCell.DrawEmpty();
				mainOrgansFixedTypeItemCell.UpdateWidgetBackgroundSprite(inactiveCellBackSprite);
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

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new BodyOrgansInventoryWidgetData());
	}
}
