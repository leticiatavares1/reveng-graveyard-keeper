using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveButtonControl : InteractiveControl
{
	internal long _cooldownExpirationTime;

	public string ButtonText { get; private set; }

	public uint Cost { get; private set; }

	public int RemainingCooldown
	{
		get
		{
			long num = (long)DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
			int num2 = (int)(_cooldownExpirationTime - num);
			if (num2 < 0)
			{
				num2 = 0;
			}
			return num2;
		}
	}

	public float Progress { get; private set; }

	public bool ButtonDown
	{
		get
		{
			bool result = false;
			if (base.ControlID != null && InteractivityManager._buttonStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.CountOfButtonDownEvents != 0;
			}
			return result;
		}
	}

	public bool ButtonPressed
	{
		get
		{
			bool result = false;
			if (base.ControlID != null && InteractivityManager._buttonStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.CountOfButtonPressEvents != 0;
			}
			return result;
		}
	}

	public bool ButtonUp
	{
		get
		{
			bool result = false;
			if (base.ControlID != null && InteractivityManager._buttonStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.CountOfButtonUpEvents != 0;
			}
			return result;
		}
	}

	public uint CountOfButtonDowns
	{
		get
		{
			uint result = 0u;
			if (base.ControlID != null && InteractivityManager._buttonStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.CountOfButtonDownEvents;
			}
			return result;
		}
	}

	public uint CountOfButtonPresses
	{
		get
		{
			uint result = 0u;
			if (base.ControlID != null && InteractivityManager._buttonStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.CountOfButtonPressEvents;
			}
			return result;
		}
	}

	public uint CountOfButtonUps
	{
		get
		{
			uint result = 0u;
			if (base.ControlID != null && InteractivityManager._buttonStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.CountOfButtonUpEvents;
			}
			return result;
		}
	}

	public void SetProgress(float progress)
	{
		InteractivityManager.SingletonInstance._SendSetButtonControlProperties(base.ControlID, "progress", disabled: false, progress, string.Empty, 0u);
	}

	public void SetText(string text)
	{
		InteractivityManager.SingletonInstance._SendSetButtonControlProperties(base.ControlID, "text", disabled: false, 0f, text, 0u);
	}

	public void SetCost(uint cost)
	{
		InteractivityManager.SingletonInstance._SendSetButtonControlProperties(base.ControlID, "cost", disabled: false, 0f, string.Empty, cost);
	}

	public bool GetButtonDown(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetButtonDown(base.ControlID, userID);
	}

	public bool GetButtonPressed(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetButtonPressed(base.ControlID, userID);
	}

	public bool GetButtonUp(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetButtonUp(base.ControlID, userID);
	}

	public uint GetCountOfButtonDowns(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetCountOfButtonDowns(base.ControlID, userID);
	}

	public uint GetCountOfButtonPresses(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetCountOfButtonPresses(base.ControlID, userID);
	}

	public uint GetCountOfButtonUps(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetCountOfButtonUps(base.ControlID, userID);
	}

	public void TriggerCooldown(int cooldown)
	{
		InteractivityManager.SingletonInstance.TriggerCooldown(base.ControlID, cooldown);
	}

	public InteractiveButtonControl(string controlID, InteractiveEventType type, bool disabled, string helpText, uint cost, string eTag, string sceneID, Dictionary<string, object> metaproperties)
		: base(controlID, "button", type, disabled, helpText, eTag, sceneID, metaproperties)
	{
		Cost = cost;
	}
}
