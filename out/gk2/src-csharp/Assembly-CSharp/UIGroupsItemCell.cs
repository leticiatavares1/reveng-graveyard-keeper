using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupsItemCell : MonoBehaviour
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private List<string> itemGroups;

	[SerializeField]
	private Image emptySlotImage;

	public List<string> ItemGroups => itemGroups;

	public UIItemCell UIItemCell => itemCell;

	public void Draw(Item item, ItemRelatedWidgetState customState = ItemRelatedWidgetState.NotSet)
	{
		itemCell.Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: false, 0, drawCounter: true, forceNonEmpty: false, forceDrawCounter: false, customState);
		emptySlotImage.gameObject.SetActive(value: false);
	}

	public void DrawEmpty()
	{
		itemCell.DrawEmpty();
		emptySlotImage.gameObject.SetActive(value: true);
	}

	public void DrawEmptyInteractable()
	{
		itemCell.DrawEmptyInteractable();
		emptySlotImage.gameObject.SetActive(value: true);
	}
}
