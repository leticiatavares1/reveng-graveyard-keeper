using System.Collections.Generic;

public class BuildModeGUI : BaseGUI
{
	public override void Open()
	{
		base.Open();
		InteractionBubbleGUI.ShowAllRemoveBubbles();
		MainGame.me.build_mode_logics.cur_build_zone.RedrawQualities(true, separate_k: true);
	}

	public void RedrawPlacing(bool can_place, bool can_be_rotated)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		List<GameKeyTip> list = new List<GameKeyTip>();
		if (can_be_rotated)
		{
			if (BaseGUI.for_gamepad)
			{
				list.Add(new GameKeyTip(GameKey.RotateLeft, "rotate left"));
				list.Add(new GameKeyTip(GameKey.RotateRight, "rotate right"));
			}
			else
			{
				list.Add(new GameKeyTip(GameKey.RotateRight, "rotate", active: true, gamepad_only: false));
			}
		}
		list.Add(GameKeyTip.Select("place", can_place));
		list.Add(GameKeyTip.Back(active: true, gamepad_only: false));
		base.button_tips.Print(list);
	}

	public void RedrawRemoving(bool waiting_for_removing, bool can_be_removed)
	{
		if (base.gameObject.activeSelf)
		{
			base.button_tips.Print(waiting_for_removing ? GameKeyTip.Select("cancel removing") : GameKeyTip.Select("remove", can_be_removed), GameKeyTip.Back(active: true, gamepad_only: false));
		}
	}

	public void RedrawScriptMode(bool has_variation)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		List<GameKeyTip> list = new List<GameKeyTip>();
		if (has_variation)
		{
			if (BaseGUI.for_gamepad)
			{
				list.Add(new GameKeyTip(GameKey.RotateLeft, "next"));
				list.Add(new GameKeyTip(GameKey.RotateRight, "previous"));
			}
			else
			{
				list.Add(new GameKeyTip(GameKey.RotateRight, "next", active: true, gamepad_only: false));
			}
		}
		if (BaseGUI.for_gamepad)
		{
			list.Add(GameKeyTip.Select("apply"));
		}
		else
		{
			list.Add(new GameKeyTip(GameKey.LeftClick, "apply", active: true, gamepad_only: false));
		}
		list.Add(GameKeyTip.Back(active: true, gamepad_only: false));
		base.button_tips.Print(list);
	}
}
