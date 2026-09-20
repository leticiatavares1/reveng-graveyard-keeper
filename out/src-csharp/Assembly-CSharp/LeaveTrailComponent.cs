using System.Collections.Generic;
using DarkTonic.MasterAudio;
using UnityEngine;

public class LeaveTrailComponent
{
	private BaseCharacterComponent _ch;

	private Vector2 _prev_pos = Vector2.zero;

	private Vector2 _prev_pos_sound = Vector2.zero;

	private Ground.GroudType _ground_under;

	private Ground.GroudType _trail_type;

	private Ground.GroudType _dirty_type;

	private float _dirty_amount;

	private Vector2 _dir = Vector2.zero;

	private string _preset_id;

	private TrailDefinition _trail_definition;

	private bool _is_left_foot = true;

	private bool _update_trails = true;

	private const float LEAVE_TRAIL_DISTANCE = 370f;

	private const float SOUND_TRAIL_DISTANCE = 7000f;

	private static List<TrailObject> _all_trails = new List<TrailObject>();

	public bool UpdateTrails
	{
		get
		{
			return _update_trails;
		}
		set
		{
			_update_trails = value;
		}
	}

	public LeaveTrailComponent(BaseCharacterComponent ch, string preset_id)
	{
		_ch = ch;
		_preset_id = preset_id;
		_trail_definition = Resources.Load<TrailDefinition>("Trails/" + preset_id);
		if (_trail_definition == null)
		{
			Debug.LogError("Trail preset not found: " + preset_id);
		}
	}

	public void CustomUpdate()
	{
		if (!_update_trails)
		{
			return;
		}
		Vector2 vector = _ch.wgo.transform.position;
		_dir = _prev_pos - vector;
		if ((_prev_pos_sound - vector).sqrMagnitude > 7000f)
		{
			TrailTypeDefinition byType = _trail_definition.GetByType(_ground_under);
			_prev_pos_sound = vector;
			if (byType != null && byType.sound != "[None]")
			{
				MasterAudio.PlaySound(byType.sound);
			}
		}
		TrailTypeDefinition byType2 = _trail_definition.GetByType(_trail_type);
		float num = 370f;
		if (byType2 != null && byType2.custom_trail_dist)
		{
			num = byType2.leave_trail_dist;
		}
		if (_dir.sqrMagnitude < num)
		{
			return;
		}
		_prev_pos = vector;
		_ground_under = _ch.GetGroundTypeUnderCharacter();
		if (_ground_under != _dirty_type && SteppedOnANewSurface(_ground_under))
		{
			return;
		}
		if (_trail_type == Ground.GroudType.None)
		{
			_trail_type = _ground_under;
			_dirty_amount = 1f;
			if (_trail_type == Ground.GroudType.None)
			{
				return;
			}
		}
		LeaveTrail();
	}

	public bool SteppedOnANewSurface(Ground.GroudType ground_type)
	{
		_trail_type = _dirty_type;
		_dirty_amount = 1f;
		_dirty_type = ground_type;
		if (ground_type == Ground.GroudType.Rug)
		{
			_dirty_amount = 0f;
			return true;
		}
		return false;
	}

	private void LeaveTrail()
	{
		float num = 0.9f;
		TrailTypeDefinition byType = _trail_definition.GetByType(_trail_type);
		if (byType != null && byType.custom_trail_decrease)
		{
			num = byType.trail_decrease;
		}
		if (_trail_type != _ground_under)
		{
			_dirty_amount *= num;
		}
		if (_dirty_amount < 0.1f)
		{
			_dirty_amount = 1f;
			_trail_type = _ground_under;
		}
		else
		{
			if (_trail_type == Ground.GroudType.None)
			{
				return;
			}
			if (byType == null)
			{
				_dirty_amount = 0f;
				return;
			}
			bool flip;
			Sprite byDirection = byType.GetByDirection(_dir, _is_left_foot, out flip);
			if (!(byDirection == null))
			{
				TrailObject trailObject = TrailObject.Spawn(_prev_pos, byDirection, flip, _ch.cur_environment == BaseCharacterComponent.Environment.Outside);
				_all_trails.Add(trailObject);
				if (!(trailObject == null))
				{
					_is_left_foot = !_is_left_foot;
					trailObject.SetColor(byType.color, _dirty_amount);
				}
			}
		}
	}

	public string GetDescriptionString()
	{
		return "Trail type: " + _trail_type.ToString() + "\nAmount: " + _dirty_amount + "\nCur dirt: " + _dirty_type.ToString() + "\nUnder: " + _ground_under;
	}

	public static void RemoveAllTrailsFromTheScene()
	{
		for (int i = 0; i < _all_trails.Count; i++)
		{
			Object.Destroy(_all_trails[i].gameObject);
		}
		_all_trails.Clear();
	}

	public static void OnTrailObjectDestroyed(TrailObject o)
	{
		_all_trails.Remove(o);
	}
}
