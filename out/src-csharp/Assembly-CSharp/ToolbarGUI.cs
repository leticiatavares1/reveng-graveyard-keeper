using UnityEngine;

public class ToolbarGUI : MonoBehaviour
{
	public ToolbarSetGUI keyboard;

	public ToolbarSetGUI gamepad;

	public void Init()
	{
		keyboard.Init(interaction_enabled: true);
		gamepad.Init(interaction_enabled: false);
	}

	public void Redraw()
	{
		this.Activate();
		bool gamepad_active = LazyInput.gamepad_active;
		keyboard.SetActive(!gamepad_active);
		gamepad.SetActive(gamepad_active);
		(gamepad_active ? gamepad : keyboard).Redraw();
	}
}
