using System.Collections.Generic;
using UnityEngine;

public class ZoneControlItem : MonoBehaviour
{
	public GameObject selection;

	public string zone_group;

	private List<string> zones = new List<string>();

	private bool is_selected;

	public static ZoneControlItem current_selected;

	private UI2DSprite icon;

	private void OnEnable()
	{
		icon.enabled = IsEnabled();
	}

	private bool IsEnabled()
	{
		if (!MainGame.me.save.has_global_craft_control)
		{
			return false;
		}
		for (int i = 0; i < zones.Count; i++)
		{
			WorldZone zoneByID = WorldZone.GetZoneByID(zones[i]);
			if (zoneByID != null && MainGame.me.save.IsWorldZoneKnown(zoneByID.id) && !zoneByID.IsDisabled() && zoneByID.HasBuilder())
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		icon = GetComponent<UI2DSprite>();
		for (int i = 0; i < GameBalance.me.world_zones_data.Count; i++)
		{
			if (GameBalance.me.world_zones_data[i].zone_group == zone_group)
			{
				zones.Add(GameBalance.me.world_zones_data[i].id);
			}
		}
	}

	private void Update()
	{
		if (LazyInput.gamepad_active && is_selected && LazyInput.GetKeyDown(GameKey.Select))
		{
			LazyInput.ClearKeyDown(GameKey.Select);
			OnPress();
		}
	}

	public void OnOver()
	{
		if (IsEnabled())
		{
			if (current_selected != null)
			{
				current_selected.OnOut();
			}
			selection.gameObject.SetActive(value: true);
			is_selected = true;
			current_selected = this;
		}
	}

	public void OnOut()
	{
		if (IsEnabled())
		{
			selection.gameObject.SetActive(value: false);
			is_selected = false;
		}
	}

	public void OnPress()
	{
		if (IsEnabled())
		{
			selection.gameObject.SetActive(value: false);
			GUIElements.me.game_gui.Hide();
			GUIElements.me.global_craft_control_gui.Open(zone_group);
			is_selected = false;
			Sounds.OnGUIClick();
		}
	}

	private void OnDisable()
	{
		OnOut();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("VirtualCursor"))
		{
			OnOver();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("VirtualCursor"))
		{
			OnOut();
		}
	}
}
