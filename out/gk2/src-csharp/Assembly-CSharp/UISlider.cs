using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlider : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Button increaseButton;

	[SerializeField]
	private Button decreaseButton;

	[SerializeField]
	private TextMeshProUGUI amountLabel;

	[SerializeField]
	private List<GameKey> decreaseKeys = new List<GameKey> { GameKey.DecSlider };

	[SerializeField]
	private List<GameKey> increaseKeys = new List<GameKey> { GameKey.IncSlider };

	private static readonly GameKey[] defaultDecreaseKeys = new GameKey[1] { GameKey.DecSlider };

	private static readonly GameKey[] defaultIncreaseKeys = new GameKey[1] { GameKey.IncSlider };

	private Action<float> onChangedCallback;

	private float step = 10f;

	private bool suppressClickSound;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	private void Awake()
	{
		increaseButton.onClick.AddListener(delegate
		{
			Step(1, suppressClickSound: true);
		});
		decreaseButton.onClick.AddListener(delegate
		{
			Step(-1, suppressClickSound: true);
		});
		slider.onValueChanged.AddListener(UpdateValue);
	}

	public void Initialize(Action<float> onChangedCallback, float currentValue, float step = 10f)
	{
		this.onChangedCallback = onChangedCallback;
		this.step = step;
		suppressClickSound = true;
		slider.maxValue = 100f / step;
		slider.value = currentValue / step;
		suppressClickSound = false;
		amountLabel.text = currentValue.ToString();
	}

	private void UpdateValue(float value)
	{
		float obj = slider.value * step;
		amountLabel.text = obj.ToString();
		onChangedCallback?.Invoke(obj);
		if (!suppressClickSound)
		{
			UISliderClickSound.Play();
		}
	}

	private void IncreaseSlider()
	{
		Step(1, suppressClickSound: false);
	}

	private void DecreaseSlider()
	{
		Step(-1, suppressClickSound: false);
	}

	private void Step(int delta, bool suppressClickSound)
	{
		this.suppressClickSound = suppressClickSound;
		slider.value += delta;
		this.suppressClickSound = false;
	}

	private void Update()
	{
		if (LazyInput.IsGamepadActive && gamepadNavigationItem.IsFocused)
		{
			if (IsAnyKeyDown(decreaseKeys, defaultDecreaseKeys))
			{
				DecreaseSlider();
			}
			if (IsAnyKeyDown(increaseKeys, defaultIncreaseKeys))
			{
				IncreaseSlider();
			}
		}
	}

	private static bool IsAnyKeyDown(IReadOnlyList<GameKey> keys, GameKey[] fallbackKeys)
	{
		object obj;
		if (keys == null || keys.Count <= 0)
		{
			obj = fallbackKeys;
		}
		else
		{
			obj = keys;
		}
		IReadOnlyList<GameKey> readOnlyList = (IReadOnlyList<GameKey>)obj;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			if (LazyInput.GetKeyDown(readOnlyList[i]))
			{
				return true;
			}
		}
		return false;
	}
}
