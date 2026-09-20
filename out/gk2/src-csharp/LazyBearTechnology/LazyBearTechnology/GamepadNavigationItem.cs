using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LazyBearTechnology;

public class GamepadNavigationItem : MonoBehaviour
{
	public int group;

	public bool ignoreScroll;

	public GameObject focusFrame;

	[SerializeField]
	private RectTransform focusRectTransform;

	[SerializeField]
	private Button configuredForButton;

	private GamepadNavigationController controller;

	private int index;

	private bool isFocused;

	private Vector2 halfSize;

	private bool active = true;

	[Header("Custom Navigation")]
	[SerializeField]
	private GamepadNavigationItem leftItem;

	[SerializeField]
	private GamepadNavigationItem rightItem;

	[SerializeField]
	private GamepadNavigationItem upItem;

	[SerializeField]
	private GamepadNavigationItem downItem;

	[Space]
	[SerializeField]
	private UnityEvent onFocus = new UnityEvent();

	[SerializeField]
	private UnityEvent onUnfocus = new UnityEvent();

	[SerializeField]
	private UnityEvent onSelect = new UnityEvent();

	public RectTransform FocusRectTransform
	{
		get
		{
			if (focusRectTransform == null)
			{
				focusRectTransform = ((focusFrame != null) ? focusFrame.GetComponent<RectTransform>() : GetComponent<RectTransform>());
			}
			return focusRectTransform;
		}
		set
		{
			focusRectTransform = value;
		}
	}

	public UnityEvent OnFocus => onFocus;

	public UnityEvent OnUnfocus => onUnfocus;

	public UnityEvent OnSelect => onSelect;

	public bool IsFocused => isFocused;

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			active = value;
		}
	}

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public Vector2 Pos => base.transform.position;

	public GamepadNavigationController Controller => controller;

	public static event Action<GamepadNavigationItem> OnFocusStatic;

	public static event Action<GamepadNavigationItem> OnUnfocusStatic;

	public void Init(int index, GamepadNavigationController controller, float guiScale)
	{
		this.index = index;
		this.controller = controller;
		halfSize = new Vector2(1f, 1f);
	}

	public void Focus()
	{
		if (!isFocused)
		{
			SetFocus(focused: true);
			onFocus?.Invoke();
			GamepadNavigationItem.OnFocusStatic?.Invoke(this);
		}
	}

	public void Unfocus()
	{
		if (isFocused)
		{
			SetFocus(focused: false);
			onUnfocus?.Invoke();
			GamepadNavigationItem.OnUnfocusStatic?.Invoke(this);
		}
	}

	public void Select()
	{
		if (!(configuredForButton != null) || configuredForButton.interactable)
		{
			onSelect?.Invoke();
		}
	}

	public void SetFocus(bool focused)
	{
		isFocused = focused;
		if (focusFrame != null)
		{
			focusFrame.SetActive(focused);
		}
	}

	public float CalcDistToCurrentPos(Vector2 currentPos, GUIDirection direction)
	{
		Vector2 pos = Pos;
		switch (direction)
		{
		case GUIDirection.Left:
			pos.x += halfSize.x;
			break;
		case GUIDirection.Right:
			pos.x -= halfSize.x;
			break;
		case GUIDirection.Up:
			pos.y -= halfSize.y;
			break;
		case GUIDirection.Down:
			pos.y += halfSize.y;
			break;
		}
		return (pos - currentPos).magnitude;
	}

	public bool CorrectDirection(Vector2 otherPos, GUIDirection direction)
	{
		Vector2 vector = Pos - otherPos;
		if (vector.magnitude.EqualsTo(0f))
		{
			return false;
		}
		return direction switch
		{
			GUIDirection.Left => vector.x < 0f - halfSize.x, 
			GUIDirection.Right => vector.x > halfSize.x, 
			GUIDirection.Up => vector.y > halfSize.y, 
			GUIDirection.Down => vector.y < 0f - halfSize.y, 
			_ => false, 
		};
	}

	public bool CorrectGrid(Vector2 otherPos, GUIDirection direction)
	{
		Vector2 vector = Pos - otherPos;
		switch (direction)
		{
		case GUIDirection.Left:
		case GUIDirection.Right:
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
			{
				return Mathf.Abs(vector.y) <= halfSize.y;
			}
			return false;
		case GUIDirection.Up:
		case GUIDirection.Down:
			if (Mathf.Abs(vector.y) > Mathf.Abs(vector.x))
			{
				return Mathf.Abs(vector.x) <= halfSize.x;
			}
			return false;
		default:
			return false;
		}
	}

	public GamepadNavigationItem GetCustomDirectionItem(GUIDirection dir)
	{
		GamepadNavigationItem gamepadNavigationItem = null;
		switch (dir)
		{
		case GUIDirection.Left:
			gamepadNavigationItem = leftItem;
			break;
		case GUIDirection.Right:
			gamepadNavigationItem = rightItem;
			break;
		case GUIDirection.Up:
			gamepadNavigationItem = upItem;
			break;
		case GUIDirection.Down:
			gamepadNavigationItem = downItem;
			break;
		}
		if (!(gamepadNavigationItem == null) && gamepadNavigationItem.isActiveAndEnabled && gamepadNavigationItem.Active)
		{
			return gamepadNavigationItem;
		}
		return null;
	}

	public void SetCustomDirectionItem(GUIDirection dir, GamepadNavigationItem customItem, bool setAlsoBackwardsCustomDirection = false)
	{
		switch (dir)
		{
		case GUIDirection.Left:
			leftItem = customItem;
			if (setAlsoBackwardsCustomDirection && customItem != null)
			{
				customItem.SetCustomDirectionItem(GUIDirection.Right, this);
			}
			break;
		case GUIDirection.Right:
			rightItem = customItem;
			if (setAlsoBackwardsCustomDirection && customItem != null)
			{
				customItem.SetCustomDirectionItem(GUIDirection.Left, this);
			}
			break;
		case GUIDirection.Up:
			upItem = customItem;
			if (setAlsoBackwardsCustomDirection && customItem != null)
			{
				customItem.SetCustomDirectionItem(GUIDirection.Down, this);
			}
			break;
		case GUIDirection.Down:
			downItem = customItem;
			if (setAlsoBackwardsCustomDirection && customItem != null)
			{
				customItem.SetCustomDirectionItem(GUIDirection.Up, this);
			}
			break;
		}
	}

	public void ResetCustomDirections()
	{
		leftItem = null;
		rightItem = null;
		upItem = null;
		downItem = null;
	}

	public void SyncOnSelectWithButton()
	{
		Button component = GetComponent<Button>();
		if (component == null)
		{
			Debug.LogError("SyncOnSelectWithButton() couldn't find a button [GameObject name: " + base.name + "]", this);
		}
		else
		{
			onSelect = component.onClick;
		}
	}

	public void SetCallbacks(UnityAction onFocus, UnityAction onUnfocus, UnityAction onSelect)
	{
		this.onFocus.RemoveAllListeners();
		this.onUnfocus.RemoveAllListeners();
		this.onSelect.RemoveAllListeners();
		if (onFocus != null)
		{
			this.onFocus.AddListener(onFocus);
		}
		if (onUnfocus != null)
		{
			this.onUnfocus.AddListener(onUnfocus);
		}
		if (onSelect != null)
		{
			this.onSelect.AddListener(onSelect);
		}
	}
}
