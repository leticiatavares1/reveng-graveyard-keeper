using System;
using LazyBearTechnology;
using UnityEngine;

public class UIPorterStationItemCell : MonoBehaviour
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private Vector2 defaultLayoutSize;

	[SerializeField]
	private Vector2 bigLayoutSize;

	[SerializeField]
	private GameObject shade;

	[SerializeField]
	private GameObject checkmark;

	private RectTransform rectTransform;

	private bool isBig;

	private Action<UIItemCell> onItemCellPress;

	private bool isListed;

	private Item item;

	public RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = base.transform as RectTransform;
			}
			return rectTransform;
		}
	}

	public void Draw(Item item, bool isListed, Action<UIItemCell> onItemCellPress)
	{
		itemCell.Draw(item);
		this.item = item;
		this.isListed = isListed;
		this.onItemCellPress = onItemCellPress;
		itemCell.OnItemCellPress = OnPressed;
		itemCell.SetNativeSizeForIcon();
		isBig = item.Definition.itemSize == ItemSize.Big;
		UpdateSize(isBig);
		if (isListed)
		{
			checkmark.SetActive(value: true);
			shade.SetActive(value: false);
		}
		else
		{
			checkmark.SetActive(value: false);
			shade.SetActive(value: true);
		}
	}

	private void OnPressed(UIItemCell cell)
	{
		LazyAudio.PlayAndForget("gui_click");
		onItemCellPress?.Invoke(cell);
		isListed = !isListed;
		Draw(item, isListed, onItemCellPress);
	}

	private void UpdateSize(bool big)
	{
		if (big)
		{
			RectTransform.sizeDelta = bigLayoutSize;
		}
		else
		{
			RectTransform.sizeDelta = defaultLayoutSize;
		}
	}
}
