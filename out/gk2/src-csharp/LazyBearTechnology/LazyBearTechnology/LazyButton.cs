using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LazyBearTechnology;

[DefaultExecutionOrder(-10000)]
public class LazyButton : Button, ILazyUIElementWithId, ISerializationCallbackReceiver
{
	[SerializeField]
	private string id;

	[NonSerialized]
	private bool eventsSanitized;

	[NonSerialized]
	private bool keepPressed;

	[NonSerialized]
	private bool pointerHeld;

	[SerializeField]
	private List<IconTransition> iconTransitions = new List<IconTransition>();

	[SerializeField]
	private List<TextTransition> textTransitions = new List<TextTransition>();

	public LazyUIEvent onDown = new LazyUIEvent();

	public LazyUIEvent onNotInteractableDown = new LazyUIEvent();

	public LazyUIEvent onUp = new LazyUIEvent();

	public LazyUIEvent onNotInteractableUp = new LazyUIEvent();

	public LazyUIEvent onEnter = new LazyUIEvent();

	public LazyUIEvent onNotInteractableEnter = new LazyUIEvent();

	public LazyUIEvent onExit = new LazyUIEvent();

	public LazyUIEvent onNotInteractableExit = new LazyUIEvent();

	public LazyUIEvent onNotInteractableClick = new LazyUIEvent();

	public string onDownSound;

	public string onNotInteractableDownSound;

	public string onUpSound;

	public string onNotInteractableUpSound;

	public string onEnterSound;

	public string onNotInteractableEnterSound;

	public string onExitSound;

	public string onNotInteractableExitSound;

	public string onClickSound;

	public string onNotInteractableClickSound;

	public string LazyUIElementId
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public MonoBehaviour MonoBehaviour => this;

	public bool IsPointerHeld => pointerHeld;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		SanitizeEmptyEvents();
	}

	public static void RefreshAll()
	{
		LazyButton[] array = UnityEngine.Object.FindObjectsByType<LazyButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RefreshTextTransitions();
		}
	}

	public void RefreshTextTransitions()
	{
		ApplyTextTransitionsForState(base.currentSelectionState);
	}

	protected override void Awake()
	{
		SanitizeEmptyEvents();
		base.Awake();
		if (Application.isPlaying && !string.IsNullOrEmpty(id))
		{
			LazyUIElementManager.TryRegisterElement(this);
		}
	}

	private void SanitizeEmptyEvents()
	{
		if (!eventsSanitized)
		{
			eventsSanitized = true;
			onDown = RecreateIfEmpty(onDown);
			onNotInteractableDown = RecreateIfEmpty(onNotInteractableDown);
			onUp = RecreateIfEmpty(onUp);
			onNotInteractableUp = RecreateIfEmpty(onNotInteractableUp);
			onEnter = RecreateIfEmpty(onEnter);
			onNotInteractableEnter = RecreateIfEmpty(onNotInteractableEnter);
			onExit = RecreateIfEmpty(onExit);
			onNotInteractableExit = RecreateIfEmpty(onNotInteractableExit);
			onNotInteractableClick = RecreateIfEmpty(onNotInteractableClick);
			if (base.onClick == null || base.onClick.GetPersistentEventCount() == 0)
			{
				base.onClick = new ButtonClickedEvent();
			}
		}
	}

	private static LazyUIEvent RecreateIfEmpty(LazyUIEvent evt)
	{
		if (evt == null || evt.GetPersistentEventCount() == 0)
		{
			return new LazyUIEvent();
		}
		return evt;
	}

	protected override void OnDisable()
	{
		pointerHeld = false;
		base.OnDisable();
	}

	protected override void OnDestroy()
	{
		if (Application.isPlaying && !string.IsNullOrEmpty(id))
		{
			LazyUIElementManager.TryUnregisterElement(this);
		}
		base.OnDestroy();
	}

	public void SetKeepPressed(bool keepPressed)
	{
		this.keepPressed = keepPressed;
		DoStateTransition(keepPressed ? SelectionState.Pressed : base.currentSelectionState, instant: true);
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (keepPressed)
		{
			state = SelectionState.Pressed;
		}
		base.DoStateTransition(state, instant);
		foreach (IconTransition iconTransition in iconTransitions)
		{
			if (!(iconTransition.targetImage == null))
			{
				Sprite sprite = null;
				switch (state)
				{
				case SelectionState.Normal:
					sprite = iconTransition.defaultSprite;
					break;
				case SelectionState.Highlighted:
					sprite = iconTransition.highlightedSprite;
					break;
				case SelectionState.Pressed:
					sprite = iconTransition.pressedSprite;
					break;
				case SelectionState.Selected:
					sprite = iconTransition.selectedSprite;
					break;
				case SelectionState.Disabled:
					sprite = iconTransition.disabledSprite;
					break;
				}
				if (sprite != null)
				{
					iconTransition.targetImage.overrideSprite = sprite;
				}
			}
		}
		ApplyTextTransitionsForState(state);
	}

	private void ApplyTextTransitionsForState(SelectionState state)
	{
		foreach (TextTransition textTransition in textTransitions)
		{
			if (!(textTransition.targetLabel == null))
			{
				TextStyle textStyle = null;
				switch (state)
				{
				case SelectionState.Normal:
					textStyle = textTransition.defaultStyle;
					break;
				case SelectionState.Highlighted:
					textStyle = textTransition.highlightedStyle;
					break;
				case SelectionState.Pressed:
					textStyle = textTransition.pressedStyle;
					break;
				case SelectionState.Selected:
					textStyle = textTransition.selectedStyle;
					break;
				case SelectionState.Disabled:
					textStyle = textTransition.disabledStyle;
					break;
				}
				if (textStyle != null)
				{
					textStyle.ApplyStyle(textTransition.targetLabel);
				}
			}
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (base.interactable)
		{
			pointerHeld = true;
			onDown.Invoke();
			PlaySound(onDownSound);
		}
		else
		{
			onNotInteractableDown.Invoke();
			PlaySound(onNotInteractableDownSound);
		}
		base.OnPointerDown(eventData);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		pointerHeld = false;
		if (base.interactable)
		{
			onUp.Invoke();
			PlaySound(onUpSound);
		}
		else
		{
			onNotInteractableUp.Invoke();
			PlaySound(onNotInteractableUpSound);
		}
		base.OnPointerUp(eventData);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (base.interactable)
		{
			onEnter.Invoke();
			PlaySound(onEnterSound);
		}
		else
		{
			onNotInteractableEnter.Invoke();
			PlaySound(onNotInteractableEnterSound);
		}
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (eventData.fullyExited)
		{
			if (base.interactable)
			{
				onExit.Invoke();
				PlaySound(onExitSound);
			}
			else
			{
				onNotInteractableExit.Invoke();
				PlaySound(onNotInteractableExitSound);
			}
			base.OnPointerExit(eventData);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!base.interactable)
		{
			PlaySound(onNotInteractableClickSound);
			onNotInteractableClick.Invoke();
		}
		else
		{
			PlaySound(onClickSound);
		}
		base.OnPointerClick(eventData);
	}

	public void ForceOnEnter()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			pointerEnter = base.gameObject
		};
		OnPointerEnter(eventData);
	}

	public void ForceOnExit()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			fullyExited = true,
			pointerEnter = base.gameObject
		};
		OnPointerExit(eventData);
	}

	public void ForceOnClick()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			pointerClick = base.gameObject,
			pointerEnter = base.gameObject
		};
		OnPointerClick(eventData);
	}

	public void SetCallbacksIntoGamepadNavigationItem()
	{
		if (TryGetComponent<GamepadNavigationItem>(out var component))
		{
			component.SetCallbacks(ForceOnEnter, ForceOnExit, ForceOnClick);
		}
	}

	private void PlaySound(string sound)
	{
		if (!string.IsNullOrEmpty(sound))
		{
			LazyAudio.PlayAndForget(sound);
		}
	}
}
