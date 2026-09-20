using UnityEngine;

public static class CustomDebugInfoPanel
{
	public static void Update()
	{
		if (!(GUIElements.me == null) && Input.GetKeyDown(KeyCode.F6) && Input.GetKey(KeyCode.RightShift))
		{
			GUIElements.me.dialog.OpenOK($"Res: {Screen.width}x{Screen.height}, {Screen.fullScreenMode}\n" + $"Cam sizes: W={MainGame.me.world_cam.orthographicSize}; G={MainGame.me.gui_cam.orthographicSize}; GH={MainGame.me.ui_root.manualHeight}");
		}
	}
}
