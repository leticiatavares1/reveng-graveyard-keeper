using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class NeedItemsWidget : LazyWidget<NeedItemsWidgetData>
{
	[SerializeField]
	private List<UIItemCell> itemCells = new List<UIItemCell>();

	[SerializeField]
	private Sprite activeFilledBackSprite;

	[SerializeField]
	private Sprite activeEmptyCellBackSprite;

	[SerializeField]
	private Sprite inactiveCellBackSprite;

	public override void Redraw()
	{
		base.Redraw();
		DrawNeeds();
	}

	private void DrawNeeds()
	{
		if (data.NeedItems == null)
		{
			return;
		}
		for (int i = 0; i < data.NeedItems.Count; i++)
		{
			NeedItemData needItemData = data.NeedItems[i];
			UIItemCell uIItemCell = itemCells[i];
			uIItemCell.Draw(new Item(needItemData.Id, needItemData.GetCount(data.WgoData)), isNeedItem: true, data.MultiInventory.GetTotalCount(needItemData.Id), isCraftResult: false, 1, !data.IsActive);
			if (data.IsActive)
			{
				uIItemCell.Background.sprite = activeFilledBackSprite;
			}
			else
			{
				uIItemCell.Background.sprite = inactiveCellBackSprite;
			}
			uIItemCell.gameObject.SetActive(value: true);
		}
		for (int j = data.NeedItems.Count; j < 3; j++)
		{
			itemCells[j].gameObject.SetActive(value: false);
		}
	}

	protected override void TestDraw()
	{
	}
}
