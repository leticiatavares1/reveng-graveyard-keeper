using System;
using UnityEngine;
using UnityEngine.UI;

public class UIConveyorChestSlot : MonoBehaviour
{
	public enum State
	{
		EmptyIn,
		EmptyOut,
		NotConnected,
		FilledOut,
		OutDuringSelection
	}

	[SerializeField]
	private int slotIndex = -1;

	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private Sprite itemCellBackEmptyIn;

	[SerializeField]
	private Sprite itemCellBackNotConnected;

	[SerializeField]
	private Sprite itemCellBackOut;

	[SerializeField]
	private Image directionImage;

	[SerializeField]
	private GameObject questionMark;

	[SerializeField]
	private GameObject backDirection;

	[SerializeField]
	private Sprite directionInSprite;

	[SerializeField]
	private Sprite directionOutSprite;

	[SerializeField]
	private Direction chestPosDirection;

	private State previousState;

	private State state;

	private Action<UIConveyorChestSlot> onPress;

	private Action<UIConveyorChestSlot> onPress2;

	private ConveyorChestSlotData slotData;

	private bool isParent;

	private bool isChild;

	private bool hasCanvases;

	public UIItemCell ItemCell => itemCell;

	public Direction ChestPosDirection => chestPosDirection;

	public int SlotIndex => slotIndex;

	public State PreviousState => previousState;

	public ConveyorChestSlotData SlotData => slotData;

	public void Draw(ConveyorChestSlotData slotData, ConveyorChestComponent chestComponent, Action<UIConveyorChestSlot> onPress, Action<UIConveyorChestSlot> onPress2)
	{
		this.onPress = onPress;
		this.onPress2 = onPress2;
		this.slotData = slotData;
		isParent = slotData.ConveyorWgoData != null && chestComponent.ParentsData.ContainsValue(slotData.ConveyorWgoData);
		isChild = slotData.ConveyorWgoData != null && chestComponent.ConnectedWgoData.Contains(slotData.ConveyorWgoData);
		previousState = state;
		if (!isParent && !isChild)
		{
			state = State.NotConnected;
		}
		else if (isParent)
		{
			state = State.EmptyIn;
		}
		else if (string.IsNullOrEmpty(slotData.slotItemId))
		{
			state = State.EmptyOut;
		}
		else
		{
			state = State.FilledOut;
		}
		DrawCurrentState();
		ItemCell.OnItemCellPress = OnCellPressed;
		ItemCell.OnItemCellPress2 = OnCellPressed2;
	}

	public void Draw(ConveyorChestSlotData slotData, ConveyorChestOutComponent chestOutComponent, Action<UIConveyorChestSlot> onPress, Action<UIConveyorChestSlot> onPress2)
	{
		this.onPress = onPress;
		this.onPress2 = onPress2;
		this.slotData = slotData;
		isChild = slotData.ConveyorWgoData != null && chestOutComponent.ConnectedWgoData.Contains(slotData.ConveyorWgoData);
		previousState = state;
		if (!isChild)
		{
			state = State.NotConnected;
		}
		else if (string.IsNullOrEmpty(slotData.slotItemId))
		{
			state = State.EmptyOut;
		}
		else
		{
			state = State.FilledOut;
		}
		DrawCurrentState();
		ItemCell.OnItemCellPress = OnCellPressed;
		ItemCell.OnItemCellPress2 = OnCellPressed2;
	}

	public void SetState(State state)
	{
		previousState = this.state;
		this.state = state;
		DrawCurrentState();
	}

	private void DrawCurrentState()
	{
		Canvas canvas = null;
		bool flag = false;
		UIConveyorChestWindow componentInParent = GetComponentInParent<UIConveyorChestWindow>(includeInactive: true);
		if ((object)componentInParent != null)
		{
			canvas = componentInParent.Canvas;
			flag = componentInParent.IsItemSelectionModActive;
		}
		else
		{
			UIConveyorVegetablesChestWindow componentInParent2 = GetComponentInParent<UIConveyorVegetablesChestWindow>(includeInactive: true);
			if ((object)componentInParent2 != null)
			{
				canvas = componentInParent2.Canvas;
				flag = componentInParent2.IsItemSelectionModActive;
			}
			else
			{
				UIConveyorWineChestWindow componentInParent3 = GetComponentInParent<UIConveyorWineChestWindow>(includeInactive: true);
				if ((object)componentInParent3 != null)
				{
					canvas = componentInParent3.Canvas;
					flag = componentInParent3.IsItemSelectionModActive;
				}
			}
		}
		questionMark.SetActive(value: false);
		backDirection.SetActive(value: false);
		directionImage.gameObject.SetActive(value: false);
		switch (state)
		{
		case State.EmptyIn:
			itemCell.DrawEmpty(drawAsNonInteractable: true);
			itemCell.Background.sprite = itemCellBackEmptyIn;
			itemCell.NoSelectionFrames = true;
			backDirection.SetActive(value: true);
			directionImage.gameObject.SetActive(value: true);
			directionImage.sprite = directionInSprite;
			break;
		case State.EmptyOut:
			itemCell.DrawEmpty();
			itemCell.Background.sprite = itemCellBackOut;
			itemCell.NoSelectionFrames = false;
			questionMark.SetActive(value: true);
			backDirection.SetActive(value: true);
			directionImage.gameObject.SetActive(value: true);
			directionImage.sprite = directionOutSprite;
			break;
		case State.NotConnected:
			itemCell.DrawEmpty(drawAsNonInteractable: true);
			itemCell.NoSelectionFrames = true;
			itemCell.Background.sprite = itemCellBackNotConnected;
			break;
		case State.FilledOut:
			itemCell.Draw(new Item(slotData.slotItemId));
			itemCell.NoSelectionFrames = false;
			itemCell.Background.sprite = itemCellBackOut;
			backDirection.SetActive(value: true);
			directionImage.gameObject.SetActive(value: true);
			directionImage.sprite = directionOutSprite;
			break;
		case State.OutDuringSelection:
			if (string.IsNullOrEmpty(slotData.slotItemId))
			{
				itemCell.DrawEmpty();
			}
			else
			{
				itemCell.Draw(new Item(slotData.slotItemId));
			}
			itemCell.Background.sprite = itemCellBackOut;
			backDirection.SetActive(value: true);
			directionImage.gameObject.SetActive(value: true);
			itemCell.NoSelectionFrames = true;
			directionImage.sprite = directionOutSprite;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (state != State.OutDuringSelection)
		{
			if (hasCanvases)
			{
				UnityEngine.Object.Destroy(itemCell.Background.GetComponent<Canvas>());
				UnityEngine.Object.Destroy(itemCell.Icon.GetComponent<Canvas>());
				hasCanvases = false;
			}
		}
		else if (!hasCanvases)
		{
			Canvas canvas2 = itemCell.Background.gameObject.AddComponent<Canvas>();
			canvas2.overrideSorting = true;
			canvas2.sortingOrder = ((canvas != null) ? (canvas.sortingOrder + 5) : 5);
			Canvas canvas3 = itemCell.Icon.gameObject.AddComponent<Canvas>();
			canvas3.overrideSorting = true;
			canvas3.sortingOrder = ((canvas != null) ? (canvas.sortingOrder + 6) : 6);
			hasCanvases = true;
		}
		itemCell.GamepadNavigationItem.Active = !flag;
		UpdateConnectionTooltip();
	}

	private void UpdateConnectionTooltip()
	{
		bool flag = itemCell.DisplayingItem != null && !itemCell.DisplayingItem.IsEmpty;
		itemCell.CustomTooltipShowAction = (flag ? null : new Action<UIItemCell>(ShowConnectionTooltip));
	}

	private static void ShowConnectionTooltip(UIItemCell cell)
	{
		UIMouseTooltip.TryShow(cell.transform as RectTransform, "tt_conv_chest_conn");
	}

	private void OnCellPressed(UIItemCell cell)
	{
		switch (state)
		{
		case State.EmptyOut:
		case State.FilledOut:
			onPress(this);
			state = State.OutDuringSelection;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case State.EmptyIn:
		case State.NotConnected:
		case State.OutDuringSelection:
			break;
		}
	}

	private void OnCellPressed2(UIItemCell cell)
	{
		switch (state)
		{
		case State.FilledOut:
			onPress2(this);
			state = State.OutDuringSelection;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case State.EmptyIn:
		case State.EmptyOut:
		case State.NotConnected:
		case State.OutDuringSelection:
			break;
		}
	}
}
