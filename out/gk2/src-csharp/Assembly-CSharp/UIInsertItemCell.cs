using UnityEngine;
using UnityEngine.UI;

public class UIInsertItemCell : MonoBehaviour
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private Image emptySlotImage;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Sprite backgroundEmpty;

	[SerializeField]
	private Sprite backgroundFilled;

	[SerializeField]
	private Image requiredToolImage;

	public UIItemCell UIItemCell => itemCell;

	public void Draw(Item item, bool hasRequiredTool, ItemType requiredTool)
	{
		itemCell.Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: false, 0, drawCounter: true, forceNonEmpty: false, forceDrawCounter: false, (!hasRequiredTool) ? ItemRelatedWidgetState.Disabled : ItemRelatedWidgetState.NotSet);
		emptySlotImage.gameObject.SetActive(value: false);
		requiredToolImage.gameObject.SetActive(value: false);
		backgroundImage.sprite = backgroundFilled;
		backgroundImage.color = Color.white;
		UpdateRequiredToolIcon(hasRequiredTool, requiredTool);
	}

	public void DrawEmpty(bool hasRequiredTool, ItemType requiredTool)
	{
		if (hasRequiredTool)
		{
			itemCell.DrawEmptyInteractable();
		}
		else
		{
			itemCell.DrawEmpty(drawAsNonInteractable: true);
		}
		itemCell.SetWidgetState((!hasRequiredTool) ? ItemRelatedWidgetState.Disabled : ItemRelatedWidgetState.NotSet);
		emptySlotImage.gameObject.SetActive(value: true);
		backgroundImage.sprite = backgroundEmpty;
		UpdateRequiredToolIcon(hasRequiredTool, requiredTool);
	}

	private void UpdateRequiredToolIcon(bool hasRequiredTool, ItemType requiredTool)
	{
		if (hasRequiredTool)
		{
			requiredToolImage.gameObject.SetActive(value: false);
			return;
		}
		requiredToolImage.gameObject.SetActive(value: true);
		requiredToolImage.sprite = CraftStatusIconHelper.GetCraftStatusIcon(CraftStatus.DoesntHaveRequiredTool, requiredTool);
	}
}
