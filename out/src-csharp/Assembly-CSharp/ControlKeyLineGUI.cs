using System;
using UnityEngine;

public class ControlKeyLineGUI : MonoBehaviour
{
	public UILabel key_value;

	public UILabel key_description;

	public GameKey key;

	private bool _is_rebinding;

	private bool _blink_vis;

	private float _t;

	[NonSerialized]
	public bool changed;

	private const float BLINK_TIME = 0.05f;

	public void OnEnable()
	{
		_is_rebinding = false;
	}

	public void Redraw()
	{
		key_value.text = GameKeyTip.GetIcon(key).Replace("[", "").Replace("]", "");
		SetKeyAlpha(1f);
		RedrawDescription();
	}

	private void RedrawDescription()
	{
		string text = "";
		text = GJL.L(key switch
		{
			GameKey.Attack => "control_atk", 
			GameKey.Dash => "control_dash", 
			GameKey.Interaction => "control_interact", 
			GameKey.Work => "control_work", 
			GameKey.GameGUI => "control_gmenu", 
			GameKey.IngameMenu => "control_pause", 
			GameKey.Toolbar1 => GJL.L("control_qslot") + " 1", 
			GameKey.Toolbar2 => GJL.L("control_qslot") + " 2", 
			GameKey.Toolbar3 => GJL.L("control_qslot") + " 3", 
			GameKey.Toolbar4 => GJL.L("control_qslot") + " 4", 
			GameKey.Left => "key_left", 
			GameKey.Right => "key_right", 
			GameKey.Up => "key_up", 
			GameKey.Down => "key_down", 
			GameKey.Map => "map", 
			GameKey.Inventory => "tab_Inventory", 
			GameKey.KnownNPCs => "tab_NPCs", 
			GameKey.Techs => "tab_Techs", 
			_ => "control_" + key.ToString().ToLower(), 
		});
		text = text.ToLower();
		key_description.text = text;
	}

	public void OnRebind()
	{
		_blink_vis = true;
		_is_rebinding = true;
		_t = 0.05f;
		key_value.text = "?";
	}

	private void SetKeyAlpha(float a)
	{
		Color color = key_value.color;
		color.a = a;
		key_value.color = color;
	}

	public void Update()
	{
		if (!_is_rebinding)
		{
			return;
		}
		_t -= Time.deltaTime;
		if (_t <= 0f)
		{
			_t = 0.05f;
			_blink_vis = !_blink_vis;
			SetKeyAlpha(_blink_vis ? 1 : 0);
		}
		if (!Input.anyKeyDown)
		{
			return;
		}
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (value != KeyCode.Mouse0 && value != KeyCode.Mouse1 && value != KeyCode.Mouse2 && !value.ToString().Contains("Mouse") && Input.GetKeyDown(value))
			{
				SetKeyAlpha(1f);
				_is_rebinding = false;
				if (value != KeyCode.Escape)
				{
					KeyBindings.RedefineKey(key, value);
					changed = true;
					Redraw();
				}
				break;
			}
		}
	}
}
