using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameBindingElement : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI actionLabel;

	[SerializeField]
	private TextMeshProUGUI keyLabel;

	[SerializeField]
	private GameObject recordObject;

	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private LayoutElement layoutElement;

	public KeyBinding keyBinding;

	private Action<UIGameBindingElement> onSubmitBinding;

	private Action onChangingBinding;

	private bool isChanging;

	private bool isForGamepad;

	private void Update()
	{
		if (isForGamepad || !isChanging)
		{
			return;
		}
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(value))
			{
				if (!IsForbiddenKey(value))
				{
					isChanging = false;
					SubmitChanges(value);
				}
				break;
			}
		}
	}

	private void SubmitChanges(KeyCode newKey)
	{
		if (newKey == KeyCode.None)
		{
			Debug.LogError("Cannot assign keykode!");
		}
		keyBinding.keyCode = newKey;
		keyLabel.text = GetTextForKeyCode(newKey);
		recordObject.gameObject.SetActive(value: false);
		onSubmitBinding?.Invoke(this);
	}

	private bool IsForbiddenKey(KeyCode key)
	{
		if (key >= KeyCode.F1 && key <= KeyCode.F12)
		{
			return true;
		}
		switch (key)
		{
		case KeyCode.LeftWindows:
		case KeyCode.RightWindows:
			return true;
		case KeyCode.RightMeta:
		case KeyCode.LeftMeta:
			return true;
		case KeyCode.Menu:
			return true;
		case KeyCode.Print:
			return true;
		case KeyCode.ScrollLock:
			return true;
		case KeyCode.Break:
			return true;
		case KeyCode.Pause:
			return true;
		case KeyCode.Numlock:
			return true;
		case KeyCode.Tilde:
			return true;
		case KeyCode.BackQuote:
			return true;
		case KeyCode.Mouse0:
			return true;
		case KeyCode.Escape:
			return true;
		default:
			return false;
		}
	}

	public void InitializedForGamepad(GamepadBinding gamepadBinding)
	{
		layoutElement.minHeight = 30f;
		actionLabel.text = LLBase.L(gamepadBinding.localeId);
		keyLabel.text = ControllerIconLibrary.GetIconId(gamepadBinding.gameKey);
		button.gameObject.SetActive(value: false);
		recordObject.gameObject.SetActive(value: false);
		isForGamepad = true;
	}

	public void Initialize(KeyBinding binding, Action<UIGameBindingElement> onSubmitBinding, Action onChangingBinding)
	{
		layoutElement.minHeight = 20f;
		isForGamepad = false;
		keyBinding = binding;
		actionLabel.text = LLBase.L(binding.localeId);
		keyLabel.text = GetTextForKeyCode(keyBinding.keyCode);
		if (binding.keyCode == KeyCode.Escape || binding.keyCode == KeyCode.Mouse0)
		{
			button.gameObject.SetActive(value: false);
			return;
		}
		this.onSubmitBinding = onSubmitBinding;
		this.onChangingBinding = onChangingBinding;
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(ChangeBinding);
		button.gameObject.SetActive(value: true);
		recordObject.gameObject.SetActive(value: false);
	}

	public void UpdateKeyLabel()
	{
		keyLabel.text = GetTextForKeyCode(keyBinding.keyCode);
	}

	public void ChangeIsAvailable(bool value)
	{
		button.interactable = value;
	}

	private void ChangeBinding()
	{
		recordObject.gameObject.SetActive(value: true);
		isChanging = true;
		keyLabel.text = "...";
		onChangingBinding?.Invoke();
	}

	private string GetTextForKeyCode(KeyCode keyCode)
	{
		return LazySingletonSO<ControllerIconLibrary>.Instance.GetKeycodeString(keyCode);
	}
}
