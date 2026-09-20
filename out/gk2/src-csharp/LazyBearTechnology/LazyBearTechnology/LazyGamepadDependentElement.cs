using UnityEngine;

namespace LazyBearTechnology;

public class LazyGamepadDependentElement : MonoBehaviour
{
	public bool anyGamepad;

	public bool gamepadXbox;

	public bool gamepadPs;

	public bool gamepadSwitch;

	public bool isDualShock;

	public void UpdateState()
	{
		if (!base.enabled)
		{
			return;
		}
		switch (Platform.Type)
		{
		case PlatformType.PС:
			if (LazyInput.IsGamepadActive)
			{
				if (LazyInput.CurrentGamepadType == GamepadType.Sony_DualShock || LazyInput.CurrentGamepadType == GamepadType.Sony_DualSense)
				{
					if (LazyInput.CurrentGamepadType == GamepadType.Sony_DualShock)
					{
						base.gameObject.SetActive(anyGamepad || (isDualShock && gamepadPs));
					}
					else
					{
						base.gameObject.SetActive(anyGamepad || (!isDualShock && gamepadPs));
					}
				}
				else
				{
					base.gameObject.SetActive(anyGamepad || gamepadXbox);
				}
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		case PlatformType.XBox:
			if (LazyInput.IsGamepadActive)
			{
				base.gameObject.SetActive(anyGamepad || gamepadXbox);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		case PlatformType.PlayStation:
			if (gamepadPs)
			{
				if (anyGamepad)
				{
					base.gameObject.SetActive(value: true);
				}
				else if (LazyInput.CurrentGamepadType == GamepadType.Sony_DualShock)
				{
					base.gameObject.SetActive(isDualShock && gamepadPs);
				}
				else
				{
					base.gameObject.SetActive(!isDualShock && gamepadPs);
				}
			}
			else
			{
				base.gameObject.SetActive(anyGamepad);
			}
			break;
		case PlatformType.Switch:
		case PlatformType.Switch2:
			base.gameObject.SetActive(anyGamepad || gamepadSwitch);
			break;
		default:
			base.gameObject.SetActive(value: false);
			break;
		}
	}
}
