using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

[DefaultExecutionOrder(-3000)]
public class Sounds : MonoBehaviour
{
	public enum ElementType
	{
		Unknown,
		Button,
		ItemCell
	}

	private static HashSet<string> _played_sounds = new HashSet<string>();

	public static bool ignore_window_sounds = false;

	public static void Init()
	{
		SingletonGameObjects.FindOrCreate<Sounds>();
	}

	public void Update()
	{
		_played_sounds.Clear();
	}

	public static bool WasAnySoundPlayedThisFrame()
	{
		return _played_sounds.Count > 0;
	}

	public static float CalcSoundVolume(Vector2? pos, float custom_distance = 0f)
	{
		float value = 1f;
		Vector2 vector = MainGame.me.world_cam.transform.position;
		float num = ((!pos.HasValue) ? 0f : ((vector - pos.Value).magnitude / 96f));
		num -= 3.5f;
		if (num > 0f)
		{
			value = 1f - num / (6f + custom_distance);
		}
		return Mathf.Clamp01(value);
	}

	public static PlaySoundResult PlaySound(string snd, Vector2? pos = null, bool force_play = false, float custom_distance = 0f)
	{
		if (_played_sounds.Contains(snd) || GUIElements.gui_is_initializing)
		{
			return null;
		}
		float num = CalcSoundVolume(pos, custom_distance);
		PlaySoundResult result = null;
		if ((double)num > 0.01 || force_play)
		{
			_played_sounds.Add(snd);
			result = MasterAudio.PlaySound(snd, num);
		}
		return result;
	}

	public static void OnGUIClick()
	{
		PlaySound("gui_click");
	}

	public static void OnGUITabClick()
	{
		PlaySound("tab_click");
	}

	public static void OnGUIHover(ElementType element_type = ElementType.Unknown)
	{
		if (element_type == ElementType.ItemCell)
		{
			PlaySound("gui_hover_light");
		}
		else
		{
			PlaySound("gui_hover");
		}
	}

	public static void OnClosePressed()
	{
		if (!ignore_window_sounds)
		{
			PlaySound("win_close");
		}
	}

	public static void OnWindowOpened()
	{
		if (!ignore_window_sounds)
		{
			PlaySound("win_open");
		}
	}

	public static void OnToolEquip(bool equip)
	{
		PlaySound(equip ? "equip_tool" : "unequip_tool");
	}
}
