using DungeonGenerator;
using UnityEngine;

public class DungeonLevelGUIItem : MonoBehaviour
{
	public enum ButtonState
	{
		Active,
		Selected,
		Inactive
	}

	public UILabel label;

	public UI2DSprite bg_sprite;

	public UIButton button;

	public GamepadNavigationItem gamepad_navigation;

	public int dungeon_level;

	private ButtonState state = ButtonState.Inactive;

	public void SetDungeonLevel(int t_level)
	{
		if (t_level < 1)
		{
			Debug.LogError("Wrong dungeon level!");
			return;
		}
		dungeon_level = t_level;
		SavedDungeon savedDungeon = MainGame.me.save.dungeons.GetSavedDungeon(dungeon_level);
		label.text = dungeon_level.ToString();
		if (MainGame.me.dungeon_root.dungeon_is_loaded_now && MainGame.me.dungeon_root.cur_saved_dungeon == savedDungeon)
		{
			SetState(ButtonState.Selected);
		}
		else
		{
			bool flag = false;
			if (dungeon_level != 1 && !savedDungeon.is_completed)
			{
				if (MainGame.me.save.dungeons.GetSavedDungeon(dungeon_level - 1).is_completed)
				{
					for (int i = 0; i < DungeonWindowGUI.NEED_TO_BE_UNLOCKED.Length; i++)
					{
						if (DungeonWindowGUI.NEED_TO_BE_UNLOCKED[i] == dungeon_level)
						{
							if (MainGame.me.player.GetParamInt("dungeon_unlocked_" + dungeon_level) == 0)
							{
								flag = true;
							}
							break;
						}
					}
				}
				else
				{
					flag = true;
				}
			}
			SetState(flag ? ButtonState.Inactive : ButtonState.Active);
		}
		gamepad_navigation.SetCallbacks(null, null, OnPressed);
	}

	public void SetState(ButtonState t_state)
	{
		state = t_state;
		bg_sprite.alpha = ((state == ButtonState.Inactive) ? 0.5f : 1f);
		button.enabled = state == ButtonState.Active;
	}

	public void OnPressed()
	{
		if (state == ButtonState.Active)
		{
			OnEnterToDungeon();
		}
	}

	private void OnEnterToDungeon()
	{
		Debug.Log("Enter to dungeon #" + dungeon_level);
		GUIElements.ChangeBubblesVisibility(show: false);
		MainGame.me.player.components.character.control_enabled = false;
		GUIElements.me.dungeon_window.OnClosePressed();
		CameraTools.Fade(delegate
		{
			MainGame.me.TeleportToDungeonLevel(dungeon_level);
			CameraTools.UnFade(delegate
			{
				MainGame.me.player.components.character.control_enabled = true;
				GUIElements.ChangeBubblesVisibility(MainGame.me.player_char.control_enabled);
			}, 0.5f);
		}, 0.5f);
	}
}
