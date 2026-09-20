using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UISwitchButton : MonoBehaviour
{
	[SerializeField]
	private LazyButton increaseButton;

	[SerializeField]
	private LazyButton decreaseButton;

	[SerializeField]
	private TextMeshProUGUI amountLabel;

	[SerializeField]
	private TextMeshProUGUI headerLabel;

	[Space]
	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	private static readonly GameKey[] defaultDecreaseKeys = new GameKey[1] { GameKey.DecSlider };

	private static readonly GameKey[] defaultIncreaseKeys = new GameKey[1] { GameKey.IncSlider };

	private IReadOnlyList<GameKey> decreaseKeys = defaultDecreaseKeys;

	private IReadOnlyList<GameKey> increaseKeys = defaultIncreaseKeys;

	private int currentFieldIndex;

	private string[] fields;

	private Action<int> onChangedCallback;

	private bool isInteractable = true;

	private bool loopNavigation = true;

	public bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			isInteractable = value;
			RefreshNavigationButtons();
		}
	}

	public int CurrentFieldIndex => currentFieldIndex;

	public string CurrentFieldData => fields[currentFieldIndex];

	private void Awake()
	{
		increaseButton.onClick.AddListener(IncreaseSlider);
		decreaseButton.onClick.AddListener(DecreaseSlider);
	}

	public void Initialize(Action<int> onChangedCallback, string[] fields, int currentFieldIndex = 0, string header = "", GameKey[] decreaseKeys = null, GameKey[] increaseKeys = null, bool loopNavigation = true)
	{
		this.onChangedCallback = onChangedCallback;
		this.fields = fields;
		this.decreaseKeys = ((decreaseKeys != null && decreaseKeys.Length != 0) ? decreaseKeys : defaultDecreaseKeys);
		this.increaseKeys = ((increaseKeys != null && increaseKeys.Length != 0) ? increaseKeys : defaultIncreaseKeys);
		this.loopNavigation = loopNavigation;
		UpdateField(currentFieldIndex, fireCallback: false);
		if (!string.IsNullOrEmpty(header) && headerLabel != null)
		{
			headerLabel.text = header;
		}
	}

	public void UpdateField(int currentFieldIndex, bool fireCallback = true)
	{
		if (currentFieldIndex >= fields.Length)
		{
			currentFieldIndex = fields.Length - 1;
		}
		else if (currentFieldIndex < 0)
		{
			currentFieldIndex = 0;
		}
		this.currentFieldIndex = currentFieldIndex;
		Apply(fireCallback);
		RefreshNavigationButtons();
	}

	private void IncreaseSlider()
	{
		int num = currentFieldIndex + 1;
		if (num == fields.Length)
		{
			if (!loopNavigation)
			{
				return;
			}
			num = 0;
		}
		currentFieldIndex = num;
		Apply();
		RefreshNavigationButtons();
	}

	private void DecreaseSlider()
	{
		int num = currentFieldIndex - 1;
		if (num < 0)
		{
			if (!loopNavigation)
			{
				return;
			}
			num = fields.Length - 1;
		}
		currentFieldIndex = num;
		Apply();
		RefreshNavigationButtons();
	}

	private void Apply(bool fireOnChanged = true)
	{
		amountLabel.text = fields[currentFieldIndex];
		if (fireOnChanged)
		{
			onChangedCallback?.Invoke(currentFieldIndex);
		}
	}

	private void RefreshNavigationButtons()
	{
		bool flag = fields != null && fields.Length != 0;
		decreaseButton.interactable = isInteractable && flag && (loopNavigation || currentFieldIndex > 0);
		increaseButton.interactable = isInteractable && flag && (loopNavigation || currentFieldIndex < fields.Length - 1);
	}

	public void ReinitLabels(string[] fields)
	{
		this.fields = fields;
		amountLabel.text = fields[currentFieldIndex];
		RefreshNavigationButtons();
	}

	public void SetCustomLabelText(string text)
	{
		amountLabel.text = text;
	}

	private void Update()
	{
		if (IsInteractable && LazyInput.IsGamepadActive && gamepadNavigationItem.IsFocused)
		{
			if (IsAnyKeyDown(decreaseKeys))
			{
				DecreaseSlider();
			}
			if (IsAnyKeyDown(increaseKeys))
			{
				IncreaseSlider();
			}
		}
	}

	private static bool IsAnyKeyDown(IReadOnlyList<GameKey> keys)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			if (LazyInput.GetKeyDown(keys[i]))
			{
				return true;
			}
		}
		return false;
	}
}
