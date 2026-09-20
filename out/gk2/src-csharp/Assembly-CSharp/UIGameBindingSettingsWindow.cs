using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIGameBindingSettingsWindow : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private LazyButton restoreDefaultBindings;

	[SerializeField]
	private LazyButton okBtn;

	[SerializeField]
	private List<GameKey> keysToBind = new List<GameKey>();

	[SerializeField]
	private List<GameKey> gamepadBindings = new List<GameKey>();

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private ScrollRect scrollRectGamepad;

	private Dictionary<GameKey, UIGameBindingElement> cachedBindingElements = new Dictionary<GameKey, UIGameBindingElement>();

	private List<UIGameBindingElement> gamepadBindingsList = new List<UIGameBindingElement>();

	private GameBindings bindings;

	private List<KeyBinding> keyBindings;

	private bool isLocked;

	public Action onClosed;

	public static event Action OnBtnUpdated;

	private void Awake()
	{
		okBtn.onClick.AddListener(Close);
		restoreDefaultBindings.onClick.AddListener(OnReset);
	}

	public override void Init()
	{
		bindings = LazyInput.GameBindings;
		keyBindings = bindings.keyBindings;
		base.Init();
	}

	public override void Redraw()
	{
		base.Redraw();
		isLocked = false;
		okBtn.interactable = true;
		restoreDefaultBindings.interactable = true;
		foreach (KeyValuePair<GameKey, UIGameBindingElement> cachedBindingElement in cachedBindingElements)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(cachedBindingElement.Value);
		}
		cachedBindingElements.Clear();
		foreach (UIGameBindingElement gamepadBindings in gamepadBindingsList)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(gamepadBindings);
		}
		gamepadBindingsList.Clear();
		foreach (GameKey gameKey in keysToBind)
		{
			UIGameBindingElement elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIGameBindingElement>(scrollRect.content);
			elementFromPool.gameObject.SetActive(value: true);
			KeyBinding binding = keyBindings.Find((KeyBinding k) => k.gameKey.value == gameKey.value);
			elementFromPool.Initialize(binding, UpdateBinding, delegate
			{
				SetLockOnBindingButtons(state: true);
			});
			cachedBindingElements.Add(gameKey, elementFromPool);
		}
		foreach (GameKey gamepadBinding in this.gamepadBindings)
		{
			UIGameBindingElement elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIGameBindingElement>(scrollRectGamepad.content);
			elementFromPool.gameObject.SetActive(value: true);
			elementFromPool.InitializedForGamepad(bindings.gamepadBindings.Find((GamepadBinding b) => b.gameKey.value == gamepadBinding.value));
			gamepadBindingsList.Add(elementFromPool);
		}
		((RectTransform)base.transform).RefreshContentFitter();
		scrollRect.ResetPosition();
		scrollRectGamepad.ResetPosition();
	}

	public override void Close()
	{
		if (!isLocked)
		{
			base.Close();
			onClosed?.Invoke();
			onClosed = null;
		}
	}

	private void OnReset()
	{
		GameSettings.Instance.ApplyDefaultGameBindings();
		foreach (UIGameBindingElement value in cachedBindingElements.Values)
		{
			value.UpdateKeyLabel();
		}
	}

	protected override void PrintTips()
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		list.Add(new LazyGameKeyTip(GameKey.Select, "btn_ok"));
		list.Add(LazyGameKeyTip.Back());
		lazyButtonTips.Print(list);
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, delegate
		{
			Close();
			return true;
		});
		return gameKeyDelegates;
	}

	private void SetLockOnBindingButtons(bool state)
	{
		isLocked = state;
		okBtn.interactable = !isLocked;
		restoreDefaultBindings.interactable = !isLocked;
		foreach (KeyValuePair<GameKey, UIGameBindingElement> cachedBindingElement in cachedBindingElements)
		{
			cachedBindingElement.Value.ChangeIsAvailable(!state);
		}
	}

	private void UpdateBinding(UIGameBindingElement bindingElement)
	{
		KeyBinding keyBinding = null;
		foreach (KeyBinding keyBinding2 in keyBindings)
		{
			if (keyBinding2.gameKey.value == GameKey.Interaction.value)
			{
				keyBinding = keyBinding2;
			}
			if (keyBinding2.gameKey.value != bindingElement.keyBinding.gameKey.value && keyBinding2.keyCode == bindingElement.keyBinding.keyCode)
			{
				keyBinding2.keyCode = KeyCode.None;
			}
		}
		if (keyBinding != null)
		{
			foreach (KeyBinding keyBinding3 in keyBindings)
			{
				if (keyBinding3.gameKey.value == GameKey.SpeechSkip2.value)
				{
					keyBinding3.keyCode = keyBinding.keyCode;
					break;
				}
			}
		}
		SetLockOnBindingButtons(state: false);
		foreach (KeyValuePair<GameKey, UIGameBindingElement> cachedBindingElement in cachedBindingElements)
		{
			cachedBindingElement.Value.UpdateKeyLabel();
		}
		GameSettings.Instance.SaveCurrentGameBindings();
		ControllerIconLibrary.UpdateStandaloneIcons();
		UIGameBindingSettingsWindow.OnBtnUpdated?.Invoke();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		LazyUI.GetWindow<UIGameBindingSettingsWindow>().Open(null);
	}
}
