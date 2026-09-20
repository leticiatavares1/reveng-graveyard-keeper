using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIFixedTypeItemCell : MonoBehaviour
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private ItemType itemType;

	[SerializeField]
	private Image emptySlotImage;

	[SerializeField]
	private Image unknownImage;

	[SerializeField]
	private Image background;

	private bool isEmpty;

	public UIItemCell UIItemCell => itemCell;

	public ItemType ItemType => itemType;

	public GamepadNavigationItem GamepadNavigationItem => itemCell.GamepadNavigationItem;

	public void Draw(Item item, ItemRelatedWidgetState customState = ItemRelatedWidgetState.NotSet, bool drawAsInteractable = true)
	{
		itemCell.Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, !drawAsInteractable, 0, drawCounter: true, forceNonEmpty: false, forceDrawCounter: false, customState);
		isEmpty = false;
		emptySlotImage.gameObject.SetActive(value: false);
		TryUpdateUnknownOrgan();
	}

	public void DrawEmpty()
	{
		itemCell.DrawEmpty();
		isEmpty = true;
		emptySlotImage.gameObject.SetActive(value: true);
		TryUpdateUnknownOrgan();
	}

	public void DrawEmptyInteractable()
	{
		itemCell.DrawEmptyInteractable();
		isEmpty = true;
		emptySlotImage.gameObject.SetActive(value: true);
		TryUpdateUnknownOrgan();
	}

	public void TryUpdateUnknownOrgan()
	{
		if (LazyConsts.MAIN_ORGANS_TYPES.Contains(ItemType))
		{
			if (MainGame.Instance.GameSave.knowledgeSystem.unlockedOrgans.Contains(ItemType))
			{
				unknownImage.gameObject.SetActive(value: false);
				itemCell.LazyButton.interactable = true;
				return;
			}
			unknownImage.gameObject.SetActive(value: true);
			unknownImage.color = (isEmpty ? new Color(1f, 1f, 1f, 0.5f) : Color.white);
			itemCell.LazyButton.interactable = false;
			emptySlotImage.gameObject.SetActive(value: false);
			itemCell.DrawEmpty(drawAsNonInteractable: true);
		}
		else
		{
			unknownImage.gameObject.SetActive(value: false);
			itemCell.LazyButton.interactable = true;
		}
	}

	public void UpdateWidgetBackgroundSprite(Sprite sprite)
	{
		background.sprite = sprite;
	}
}
