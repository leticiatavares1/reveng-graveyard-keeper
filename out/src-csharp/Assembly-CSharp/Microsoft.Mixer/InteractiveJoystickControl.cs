using System;
using System.Collections.Generic;

namespace Microsoft.Mixer;

[Serializable]
public class InteractiveJoystickControl : InteractiveControl
{
	internal uint _userID;

	public double X
	{
		get
		{
			double result = 0.0;
			if (base.ControlID != null && InteractivityManager._joystickStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.X;
			}
			return result;
		}
	}

	public double Y
	{
		get
		{
			double result = 0.0;
			if (base.ControlID != null && InteractivityManager._joystickStates.TryGetValue(base.ControlID, out var value))
			{
				result = value.Y;
			}
			return result;
		}
	}

	public double Intensity { get; private set; }

	public double GetX(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetJoystickX(base.ControlID, userID);
	}

	public double GetY(uint userID)
	{
		return InteractivityManager.SingletonInstance._GetJoystickY(base.ControlID, userID);
	}

	public InteractiveJoystickControl(string controlID, InteractiveEventType type, bool enabled, string helpText, string eTag, string sceneID, Dictionary<string, object> metaproperties)
		: base(controlID, "joystick", type, enabled, helpText, eTag, sceneID, metaproperties)
	{
	}

	private bool TryGetJoystickStateByParticipant(uint userID, string controlID, out _InternalJoystickState joystickState)
	{
		joystickState = default(_InternalJoystickState);
		bool result = false;
		if (InteractivityManager._joystickStatesByParticipant.TryGetValue(userID, out var value) && value.TryGetValue(controlID, out joystickState))
		{
			result = true;
		}
		return result;
	}
}
