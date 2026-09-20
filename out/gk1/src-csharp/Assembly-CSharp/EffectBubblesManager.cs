using System.Collections.Generic;
using UnityEngine;

public class EffectBubblesManager : BaseGUI
{
	public enum BubbleColor
	{
		White = -1,
		Red,
		Green,
		Energy,
		Sanity,
		Relation
	}

	public const int FRAMES_DELAY = 3;

	public const float PERIOD = 0.5f;

	public const float DELAY_BUBBLES_IN_ROW = 1f;

	private static EffectBubblesManager _instance;

	private static Vector3 _last_pos;

	private static float _last_bubble_time;

	public EffectBubbleGUI effect_bubble_prefab;

	private static List<StackedWgoBubblesData> _stacked_bubbles = new List<StackedWgoBubblesData>();

	private static List<EffectBubbleGUI> _all_bubbles = new List<EffectBubbleGUI>();

	private static Camera _world_cam;

	private static Camera _gui_cam;

	public Color[] colors;

	public override void Init()
	{
		effect_bubble_prefab.Deactivate();
		_instance = this;
		BaseGUI.on_window_opened += delegate
		{
			ChangeBubblesVisibility();
		};
		BaseGUI.on_window_closed += delegate
		{
			ChangeBubblesVisibility();
		};
		_world_cam = MainGame.me.world_cam;
		_gui_cam = MainGame.me.gui_cam;
	}

	public static void RemoveAllBubbles(bool stacked_too = true)
	{
		while (_all_bubbles.Count > 0)
		{
			RemoveBubble(_all_bubbles[0]);
		}
		if (stacked_too)
		{
			_stacked_bubbles.Clear();
		}
	}

	public static void RemoveBubble(EffectBubbleGUI effect_bubble)
	{
		_all_bubbles.Remove(effect_bubble);
		effect_bubble.DestroyGO();
	}

	public new void Update()
	{
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < _stacked_bubbles.Count; i++)
		{
			StackedWgoBubblesData stackedWgoBubblesData = _stacked_bubbles[i];
			bool flag = false;
			if (stackedWgoBubblesData.wgo == null)
			{
				flag = true;
			}
			else if (stackedWgoBubblesData.frames_delay > 0)
			{
				if (--stackedWgoBubblesData.frames_delay == 0)
				{
					stackedWgoBubblesData.TryToShowBubble();
				}
			}
			else
			{
				stackedWgoBubblesData.period_delay -= deltaTime;
				if (stackedWgoBubblesData.period_delay <= 0f)
				{
					stackedWgoBubblesData.TryToShowBubble();
					if (stackedWgoBubblesData.period_delay < -0.5f)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				_stacked_bubbles.RemoveAt(i);
				i--;
			}
		}
	}

	public new void LateUpdate()
	{
		foreach (EffectBubbleGUI all_bubble in _all_bubbles)
		{
			all_bubble.UpdateBubble();
		}
	}

	public void ChangeBubblesVisibility()
	{
		ChangeBubblesVisibility(BaseGUI.all_guis_closed);
	}

	public static void ChangeBubblesVisibility(bool should_be_active)
	{
		foreach (EffectBubbleGUI all_bubble in _all_bubbles)
		{
			all_bubble.SetActive(should_be_active);
		}
	}

	public static void ShowImmediately(Vector3 position, string text, BubbleColor color = BubbleColor.White, bool ignore_timescale = true, float custom_time = -1f, bool is_gui_pos = false)
	{
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("Can't show effect bubble for empty string");
			return;
		}
		if (_last_pos == position && (double)(Time.time - _last_bubble_time) < 0.25)
		{
			GJTimer.AddTimer(1f, delegate
			{
				ShowImmediately(position, text, color, ignore_timescale, custom_time, is_gui_pos);
			});
			return;
		}
		Debug.Log("ShowEffectBubble: " + text);
		if (is_gui_pos)
		{
			position = _world_cam.ScreenToWorldPoint(_gui_cam.WorldToScreenPoint(position));
		}
		_last_pos = position;
		_last_bubble_time = Time.time;
		EffectBubbleGUI effectBubbleGUI = _instance.effect_bubble_prefab.Copy();
		effectBubbleGUI.InitEffect(position, text, (color == BubbleColor.White) ? Color.white : _instance.colors[(int)color], ignore_timescale, custom_time);
		_all_bubbles.Add(effectBubbleGUI);
		BuffsLogics.CheckBuffsGiveConditions();
	}

	public static void ShowImmediately(Vector3 position, GameRes res, bool ignore_timescale = true, float custom_time = -1f, bool is_gui_pos = false)
	{
		if (!res.IsEmpty())
		{
			ShowImmediately(position, res.ToFormattedString(), BubbleColor.White, ignore_timescale, custom_time, is_gui_pos);
		}
	}

	public static void ShowImmediately(WorldGameObject wgo, GameRes res, bool ignore_timescale = true, float custom_time = -1f)
	{
		ShowImmediately(wgo.bubble_pos, res.ToFormattedString(), BubbleColor.White, ignore_timescale, custom_time);
	}

	public static void ShowStacked(WorldGameObject wgo, GameRes res, Vector3? custom_pos = null)
	{
		if (res.IsEmpty())
		{
			return;
		}
		long unique_id = wgo.unique_id;
		foreach (StackedWgoBubblesData stacked_bubble in _stacked_bubbles)
		{
			if (stacked_bubble.id == unique_id)
			{
				stacked_bubble.AddRes(res);
				if (custom_pos.HasValue)
				{
					stacked_bubble.custom_pos = custom_pos;
				}
				return;
			}
		}
		_stacked_bubbles.Add(new StackedWgoBubblesData(unique_id, wgo, res, custom_pos));
	}

	public static void RemoveStacked(WorldGameObject wgo)
	{
		for (int i = 0; i < _stacked_bubbles.Count; i++)
		{
			if (_stacked_bubbles[i].wgo == wgo)
			{
				_stacked_bubbles.RemoveAt(i);
				break;
			}
		}
	}

	public static void ShowStackedHP(WorldGameObject wgo, float value)
	{
		ShowStacked(wgo, new GameRes
		{
			hp = value
		});
	}

	public static void ShowStackedEnergy(WorldGameObject wgo, float value)
	{
		ShowStacked(wgo, new GameRes("energy", value));
	}

	public static void ShowStackedSanity(WorldGameObject wgo, float value)
	{
		ShowStacked(wgo, new GameRes("sanity", value));
	}
}
