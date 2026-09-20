using UnityEngine;

public class GamepadSelectableButton : MonoBehaviour
{
	public GameObject gamepad_frame;

	[HideInInspector]
	public GamepadNavigationItem navigation_item;

	[HideInInspector]
	public UIButton ui_button;

	private GJCommons.VoidDelegate _on_press;

	private GJCommons.VoidDelegate _on_over;

	private Color _normal_color;

	private bool _inited;

	public void Init()
	{
		ui_button = GetComponentInChildren<UIButton>();
		_normal_color = ui_button.defaultColor;
		navigation_item = GetComponent<GamepadNavigationItem>();
		navigation_item.SetCallbacks(OnOver, OnOut, OnButtonSelect);
		gamepad_frame.Deactivate();
		_inited = true;
	}

	public void SetCallbacks(GJCommons.VoidDelegate on_press, GJCommons.VoidDelegate on_over)
	{
		if (!_inited)
		{
			Init();
		}
		gamepad_frame.Deactivate();
		_on_press = on_press;
		_on_over = on_over;
	}

	public void OnButtonSelect()
	{
		if (ui_button.isEnabled)
		{
			_on_press.TryInvoke();
		}
	}

	private void OnOver()
	{
		gamepad_frame.Activate();
		_on_over.TryInvoke();
	}

	private void OnOut()
	{
		gamepad_frame.Deactivate();
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			ui_button.defaultColor = _normal_color;
		}
		ui_button.tweenTarget.GetComponent<UIWidget>().color = (enabled ? ui_button.defaultColor : ui_button.disabledColor);
		ui_button.isEnabled = enabled;
	}
}
