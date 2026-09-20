using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartSlider : MonoBehaviour
{
	public const int GAMEPAD_FAST_SLIDER_CHANGE_VALUE = 10;

	[SerializeField]
	private TextMeshProUGUI minCounter;

	[SerializeField]
	private TextMeshProUGUI maxCounter;

	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private LazyButton minusBtn;

	[SerializeField]
	private LazyButton plusBtn;

	[SerializeField]
	[Space]
	private float changeValueTime = 0.2f;

	[SerializeField]
	private float holdLowChangeValueTime = 0.5f;

	[SerializeField]
	private float holdMediumChangeValueTime = 2f;

	[SerializeField]
	private float holdHighChangeValueTime = 4f;

	[SerializeField]
	[Space]
	private int lowSpeedChangeValue = 1;

	[SerializeField]
	private int mediumSpeedChangeValue = 5;

	[SerializeField]
	private int highSpeedChangeValue = 10;

	private int min;

	private int max;

	private int stepForGameKeys = 1;

	private int snapStep = 1;

	private int prevValue;

	private bool inputFieldEnabled;

	private bool gameKeysEnabled;

	private bool skipSnap;

	private readonly HoldRepeatValueChanger holdRepeat = new HoldRepeatValueChanger();

	private Action<int> onValueChanged;

	public int Value => min + Mathf.RoundToInt(slider.value);

	public void Init()
	{
		slider.onValueChanged.AddListener(delegate
		{
			OnSliderChanged();
		});
		inputField.onValueChanged.AddListener(delegate
		{
			OnInputFieldChanged();
		});
		inputField.onSubmit.AddListener(delegate
		{
			OnInputFieldSubmit();
		});
		inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
	}

	public void Open(int value, int min, int max, Action<int> onValueChanged, bool inputFieldEnabled = false, bool gameKeysEnabled = false, int stepForGameKeys = 1, int snapStep = 1)
	{
		this.min = min;
		this.max = max;
		this.snapStep = ((snapStep <= 1 || max < snapStep) ? 1 : snapStep);
		prevValue = -9999;
		holdRepeat.Reset();
		skipSnap = false;
		slider.minValue = 0f;
		slider.maxValue = max - min;
		slider.wholeNumbers = true;
		if (minCounter != null)
		{
			minCounter.text = this.min.ToString();
		}
		if (maxCounter != null)
		{
			maxCounter.text = this.max.ToString();
		}
		this.onValueChanged = onValueChanged;
		inputField.interactable = (this.inputFieldEnabled = inputFieldEnabled);
		this.gameKeysEnabled = gameKeysEnabled;
		this.stepForGameKeys = ((this.snapStep > 1) ? this.snapStep : ((stepForGameKeys == 0) ? 1 : stepForGameKeys));
		if (this.snapStep > 1)
		{
			value = SnapToNearest(value);
		}
		SetValue(value, invokeCallback: false);
		inputField.SetTextWithoutNotify(Value.ToString());
		inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
	}

	private void SetValue(int newValue, bool invokeCallback = true)
	{
		newValue = Math.Clamp(newValue, min, max);
		if (snapStep > 1 && !skipSnap)
		{
			newValue = SnapToNearest(newValue);
		}
		slider.SetValueWithoutNotify(newValue - min);
		UpdateInputFieldFromValue();
		prevValue = Value;
		if (invokeCallback && onValueChanged != null)
		{
			onValueChanged(Value);
		}
	}

	public void OnSliderChanged()
	{
		skipSnap = false;
		if (snapStep > 1)
		{
			int num = min + Mathf.RoundToInt(slider.value);
			int num2 = SnapToNearest(num);
			if (num2 != num)
			{
				slider.SetValueWithoutNotify(num2 - min);
			}
		}
		int value = Value;
		if (prevValue != value)
		{
			prevValue = value;
			UpdateInputFieldFromValue();
			UISliderClickSound.Play();
			if (onValueChanged != null)
			{
				onValueChanged(value);
			}
		}
	}

	public void OnInputFieldChanged()
	{
		if (!inputFieldEnabled)
		{
			return;
		}
		int result = 0;
		if (int.TryParse(inputField.text, out result))
		{
			if (result < min)
			{
				result = min;
			}
			if (result > max)
			{
				result = max;
			}
			skipSnap = true;
			if (result != Value)
			{
				SetValue(result);
			}
			if (!inputField.isFocused)
			{
				inputField.SetTextWithoutNotify(result.ToString());
				inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
			}
		}
	}

	public void OnInputFieldSubmit()
	{
		if (inputFieldEnabled)
		{
			inputField.DeactivateInputField();
		}
	}

	private void OnInputFieldEndEdit(string _)
	{
		ApplyInputFieldValue();
	}

	private void ApplyInputFieldValue()
	{
		if (inputFieldEnabled && !inputField.isFocused)
		{
			if (!int.TryParse(inputField.text, out var result))
			{
				result = min;
			}
			result = Math.Clamp(result, min, max);
			skipSnap = true;
			SetValue(result);
			inputField.SetTextWithoutNotify(Value.ToString());
			inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
		}
	}

	private void Update()
	{
		if (inputField.isFocused)
		{
			holdRepeat.Reset();
			return;
		}
		int num = HoldRepeatValueChanger.GetPointerHoldDirection(plusBtn, minusBtn);
		if (num == 0 && gameKeysEnabled)
		{
			num = GetInputDirection();
		}
		ApplyHoldSettings();
		holdRepeat.Tick(num, delegate(int delta)
		{
			skipSnap = false;
			ChangeValue(delta, !IsButtonHoldActive());
		});
		if (num == 0 && gameKeysEnabled)
		{
			if (LazyInput.GetKeyDown(GameKey.ItemCountWindow_Max) || LazyInput.GetKeyDown(GameKey.MaxSlider))
			{
				skipSnap = false;
				SetValue((snapStep > 1) ? GetLastSnappedValue() : max);
			}
			else if (LazyInput.GetKeyDown(GameKey.ItemCountWindow_Min) || LazyInput.GetKeyDown(GameKey.MinSlider))
			{
				skipSnap = false;
				SetValue((snapStep > 1) ? GetFirstSnappedValue() : min);
			}
		}
	}

	private bool IsButtonHoldActive()
	{
		return HoldRepeatValueChanger.GetPointerHoldDirection(plusBtn, minusBtn) != 0;
	}

	private void ApplyHoldSettings()
	{
		holdRepeat.ChangeValueTime = changeValueTime;
		holdRepeat.HoldLowChangeValueTime = holdLowChangeValueTime;
		holdRepeat.HoldMediumChangeValueTime = holdMediumChangeValueTime;
		holdRepeat.HoldHighChangeValueTime = holdHighChangeValueTime;
		holdRepeat.InitialStep = stepForGameKeys;
		holdRepeat.LowSpeedChangeValue = lowSpeedChangeValue;
		holdRepeat.MediumSpeedChangeValue = mediumSpeedChangeValue;
		holdRepeat.HighSpeedChangeValue = highSpeedChangeValue;
	}

	private int GetInputDirection()
	{
		bool flag = LazyInput.GetKey(GameKey.ItemCountWindow_Increase) || LazyInput.GetKeyDown(GameKey.ItemCountWindow_Increase) || LazyInput.GetKey(GameKey.IncSlider) || LazyInput.GetKeyDown(GameKey.IncSlider) || LazyInput.GetKey(GameKey.Right) || LazyInput.GetKeyDown(GameKey.Right) || LazyInput.GetKey(GameKey.DpadRight) || LazyInput.GetKeyDown(GameKey.DpadRight);
		bool flag2 = LazyInput.GetKey(GameKey.ItemCountWindow_Decrease) || LazyInput.GetKeyDown(GameKey.ItemCountWindow_Decrease) || LazyInput.GetKey(GameKey.DecSlider) || LazyInput.GetKeyDown(GameKey.DecSlider) || LazyInput.GetKey(GameKey.Left) || LazyInput.GetKeyDown(GameKey.Left) || LazyInput.GetKey(GameKey.DpadLeft) || LazyInput.GetKeyDown(GameKey.DpadLeft);
		float x = LazyInput.GetDirection().x;
		if (x > 0.4f)
		{
			flag = true;
		}
		else if (x < -0.4f)
		{
			flag2 = true;
		}
		if (flag == flag2)
		{
			return 0;
		}
		if (!flag)
		{
			return -1;
		}
		return 1;
	}

	private void ChangeValue(int delta, bool playClickSound = true)
	{
		int value = Value;
		int newValue = ((snapStep > 1) ? GetSteppedValue(Value, delta) : (Value + delta));
		SetValue(newValue);
		if (playClickSound && Value != value)
		{
			UISliderClickSound.Play();
		}
	}

	private void UpdateInputFieldFromValue()
	{
		if (!inputField.isFocused)
		{
			string text = Value.ToString();
			if (inputField.text != text)
			{
				inputField.SetTextWithoutNotify(text);
				inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
			}
		}
	}

	private int GetSteppedValue(int current, int delta)
	{
		if (delta == 0)
		{
			return current;
		}
		int num;
		if (delta > 0)
		{
			num = ((current % snapStep == 0) ? (current + snapStep) : (FloorToStep(current) + snapStep));
			if (Math.Abs(delta) > snapStep)
			{
				num = CeilToStep(current + delta);
			}
		}
		else
		{
			num = ((current % snapStep == 0) ? (current - snapStep) : FloorToStep(current));
			if (Math.Abs(delta) > snapStep)
			{
				num = FloorToStep(current + delta);
			}
		}
		int lastSnappedValue = GetLastSnappedValue();
		int firstSnappedValue = GetFirstSnappedValue();
		if (num > max)
		{
			if (lastSnappedValue <= current)
			{
				return current;
			}
			return lastSnappedValue;
		}
		if (num < min)
		{
			if (firstSnappedValue >= current)
			{
				return current;
			}
			return firstSnappedValue;
		}
		return num;
	}

	private int SnapToNearest(int value)
	{
		int num = FloorToStep(value);
		int num2 = num + snapStep;
		int num3 = ((value - num < num2 - value) ? num : num2);
		int lastSnappedValue = GetLastSnappedValue();
		int firstSnappedValue = GetFirstSnappedValue();
		if (num3 > max)
		{
			return lastSnappedValue;
		}
		if (num3 < min)
		{
			return firstSnappedValue;
		}
		if (num3 > lastSnappedValue)
		{
			return lastSnappedValue;
		}
		if (num3 < firstSnappedValue)
		{
			return firstSnappedValue;
		}
		return num3;
	}

	private int GetFirstSnappedValue()
	{
		int num = CeilToStep(min);
		if (num > max)
		{
			return min;
		}
		return num;
	}

	private int GetLastSnappedValue()
	{
		int num = FloorToStep(max);
		if (num < min)
		{
			return max;
		}
		return num;
	}

	private int FloorToStep(int value)
	{
		return value / snapStep * snapStep;
	}

	private int CeilToStep(int value)
	{
		int num = FloorToStep(value);
		if (num != value)
		{
			return num + snapStep;
		}
		return value;
	}
}
