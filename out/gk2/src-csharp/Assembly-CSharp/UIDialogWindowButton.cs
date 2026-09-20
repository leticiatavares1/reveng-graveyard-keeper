using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogWindowButton : MonoBehaviour, IPoolable
{
	[SerializeField]
	private LazyButton lazyButton;

	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private bool replaceForGamepad;

	[SerializeField]
	private GameObject mouseObj;

	[SerializeField]
	private GameObject gamepadObj;

	[SerializeField]
	private TextMeshProUGUI gamepadTip;

	private GameKey keyToReplace;

	private LazyWidgetBase ownerWindow;

	private bool canProcessHotkey;

	private string textGamepad;

	public bool ReplaceForGamepad => replaceForGamepad;

	public GameKey KeyToReplace => keyToReplace;

	public LazyButton LazyButton => lazyButton;

	public void Draw(UIDialogWindowData.ButtonData data)
	{
		replaceForGamepad = data.replaceForGamepad;
		keyToReplace = data.keyToReplace;
		label.text = data.text;
		textGamepad = data.textGamepad;
		lazyButton.onClick.RemoveAllListeners();
		lazyButton.onClick.AddListener(delegate
		{
			data.onPressed?.Invoke();
		});
		lazyButton.interactable = data.buttonAvailableCondition?.Invoke() ?? true;
		lazyButton.RefreshTextTransitions();
		CacheOwnerWindow();
		SyncHotkeyProcessing();
		UpdateGamepadDependentState();
	}

	private void Awake()
	{
		if (replaceForGamepad)
		{
			LazyInput.OnInputChanged += UpdateGamepadDependentState;
			LazyInput.OnActiveGamepadChangedEvent += UpdateGamepadDependentState;
		}
		LazyWindowsStackController.OnWindowBecameVisibleInStack += OnWindowBecameVisibleInStack;
		LazyWindowsStackController.OnWindowBecameHiddenInStack += OnWindowBecameHiddenInStack;
	}

	private void Update()
	{
		if (canProcessHotkey && (LazyInput.IsGamepadActive || keyToReplace.value == GameKey.Back.value) && replaceForGamepad && lazyButton.interactable && LazyInput.GetKeyDown(keyToReplace))
		{
			LazyInput.ClearKeyDown(keyToReplace);
			lazyButton.onClick?.Invoke();
		}
	}

	private void OnWindowBecameVisibleInStack(LazyWidgetBase window)
	{
		if (window == ownerWindow)
		{
			canProcessHotkey = true;
		}
	}

	private void OnWindowBecameHiddenInStack(LazyWidgetBase window)
	{
		if (window == ownerWindow)
		{
			canProcessHotkey = false;
		}
	}

	private void CacheOwnerWindow()
	{
		ownerWindow = null;
		Transform parent = base.transform;
		while (parent != null)
		{
			if (parent.TryGetComponent<LazyWidgetBase>(out var component) && parent.TryGetComponent<Canvas>(out var _))
			{
				ownerWindow = component;
				break;
			}
			parent = parent.parent;
		}
	}

	private void SyncHotkeyProcessing()
	{
		canProcessHotkey = ownerWindow != null && ownerWindow == LazyWindowsStackController.ActiveWindow;
	}

	private void OnDestroy()
	{
		if (replaceForGamepad)
		{
			LazyInput.OnInputChanged -= UpdateGamepadDependentState;
			LazyInput.OnActiveGamepadChangedEvent -= UpdateGamepadDependentState;
		}
		LazyWindowsStackController.OnWindowBecameVisibleInStack -= OnWindowBecameVisibleInStack;
		LazyWindowsStackController.OnWindowBecameHiddenInStack -= OnWindowBecameHiddenInStack;
	}

	private void UpdateGamepadDependentState()
	{
		if (replaceForGamepad && LazyInput.IsGamepadActive && base.gameObject.activeInHierarchy)
		{
			if (mouseObj != null)
			{
				mouseObj.gameObject.SetActive(value: false);
			}
			if (gamepadObj != null)
			{
				gamepadObj.gameObject.SetActive(value: true);
				if (gamepadTip != null && (object)keyToReplace != null)
				{
					string text = (string.IsNullOrEmpty(textGamepad) ? label.text : textGamepad);
					gamepadTip.text = new LazyGameKeyTip(keyToReplace, text, lazyButton.interactable).ToString();
				}
			}
		}
		else
		{
			if (!replaceForGamepad)
			{
				lazyButton.SetCallbacksIntoGamepadNavigationItem();
			}
			if (mouseObj != null)
			{
				mouseObj.gameObject.SetActive(value: true);
			}
			if (gamepadObj != null)
			{
				gamepadObj.gameObject.SetActive(value: false);
			}
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	public void OnPoolableObjReleased()
	{
		textGamepad = string.Empty;
		ownerWindow = null;
		canProcessHotkey = false;
	}
}
