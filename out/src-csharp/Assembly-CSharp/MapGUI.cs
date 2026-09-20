using System.Collections.Generic;
using UnityEngine;

public class MapGUI : BaseGameGUI
{
	private UIScrollView _scroll;

	public float gamepad_scroll_sensitivity = 0.1f;

	public float momentum_decay = 0.1f;

	private Vector2 _momentum = Vector2.zero;

	public Transform left;

	public Transform right;

	public Transform up;

	public Transform down;

	public VirtualCursor virtual_cursor;

	private Dictionary<string, MapZoneGUI> _zones = new Dictionary<string, MapZoneGUI>();

	public override void Init()
	{
		_scroll = GetComponentInChildren<UIScrollView>(includeInactive: true);
		base.Init();
		_zones.Clear();
		MapZoneGUI[] componentsInChildren = GetComponentsInChildren<MapZoneGUI>(includeInactive: true);
		foreach (MapZoneGUI mapZoneGUI in componentsInChildren)
		{
			_zones.Add(mapZoneGUI.name, mapZoneGUI);
		}
	}

	public override void Open()
	{
		base.Open();
		WorldZone myWorldZone = MainGame.me.player.GetMyWorldZone();
		foreach (KeyValuePair<string, MapZoneGUI> zone in _zones)
		{
			zone.Value.label.text = GJL.L(string.IsNullOrEmpty(zone.Value.override_name) ? ("zone_" + zone.Key) : zone.Value.override_name);
			bool flag = MainGame.me.save.known_world_zones.Contains(zone.Key);
			zone.Value.gameObject.SetActive(flag);
			if (zone.Value.hidden != null)
			{
				zone.Value.hidden.SetActive(!flag);
			}
			if (myWorldZone != null)
			{
				_ = zone.Key == myWorldZone.id;
			}
		}
		base.button_tips.gameObject.SetActive(MainGame.me.save.has_global_craft_control);
		base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.LeftStick(), GameKeyTip.RightStick());
		virtual_cursor.gameObject.SetActive(BaseGUI.for_gamepad && MainGame.me.save.has_global_craft_control);
		if (BaseGUI.for_gamepad && MainGame.me.save.has_global_craft_control)
		{
			virtual_cursor.EnableAsMapCursor(left, right, up, down);
		}
	}

	private void OnDisable()
	{
		virtual_cursor.gameObject.SetActive(value: false);
		ZoneControlItem.current_selected = null;
		base.button_tips.Clear();
	}

	protected override bool OnPressedBack()
	{
		GUIElements.me.game_gui.Hide();
		return true;
	}

	public override void Update()
	{
		Vector2 direction = LazyInput.GetDirection();
		if (!direction.magnitude.EqualsTo(0f))
		{
			_momentum = -direction;
		}
		else
		{
			_momentum *= Mathf.Pow(momentum_decay, Time.deltaTime);
		}
		if ((double)_momentum.magnitude < 0.001)
		{
			_momentum = Vector2.zero;
		}
		else
		{
			Vector2 vector = _momentum * Time.deltaTime * gamepad_scroll_sensitivity;
			vector = new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
			_scroll.MoveRelative(vector);
			_scroll.RestrictWithinBounds(instant: true);
		}
		base.Update();
	}
}
