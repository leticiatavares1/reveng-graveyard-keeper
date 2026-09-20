using System.Collections.Generic;
using UnityEngine;

public class RoundAndSortComponent : SnapToGridComponent
{
	public const int GROUND_Z_SHIFT = 2000;

	private const float default_y_k = 20f;

	private const float default_x_k = 0.02f;

	private float _force_z;

	private bool _force_z_mode;

	private bool _tried_to_find_zero_level_spr;

	private SpriteRenderer _zero_level_spr;

	private SpriteRenderer _spr_renderer;

	private bool _spr_renderer_set;

	public bool do_round = true;

	private WorldObjectPart _world_part;

	public float floor_line;

	private Vector3 _prev_pos = new Vector3(97899f, 97899f, 97899f);

	private Vector3 _prev_pos_editor = new Vector3(97899f, 97899f, 97899f);

	private static Dictionary<int, bool> _sprite_layers_are_ground = new Dictionary<int, bool>();

	private ChunkedGameObject _chnk;

	private bool _pos_dirty = true;

	public bool use_late_update = true;

	public bool never_disable;

	private float _delta_pos;

	public override void Update()
	{
		base.Update();
		if (!use_late_update)
		{
			DoUpdateStuff();
		}
	}

	public void LateUpdate()
	{
		if (use_late_update)
		{
			DoUpdateStuff();
		}
	}

	public void DoUpdateStuff(bool force = false)
	{
		Transform transform = base.transform;
		Vector3 vector;
		if (!Application.isPlaying)
		{
			vector = transform.position;
			_delta_pos = (_prev_pos_editor - vector).sqrMagnitude;
			_prev_pos_editor = vector;
			_pos_dirty = false;
		}
		else
		{
			vector = transform.localPosition;
			_delta_pos = (_prev_pos - vector).sqrMagnitude;
			_prev_pos = vector;
		}
		if (_delta_pos.EqualsTo(0f, 0.001f) && !force && !_pos_dirty)
		{
			return;
		}
		_pos_dirty = false;
		float num = 20f;
		float num2 = 0f;
		bool flag = false;
		SpriteRenderer zeroLevelSprite = GetZeroLevelSprite();
		if (!_spr_renderer_set)
		{
			_spr_renderer = GetComponent<SpriteRenderer>();
			_spr_renderer_set = true;
		}
		Vector3 vector2 = (Application.isPlaying ? transform.position : vector);
		Vector3 vector3 = vector2 / 96f;
		if (DoesSpriteBelongToGround(zeroLevelSprite))
		{
			vector3.z = GroundObject.GetGroundZ(vector2);
			flag = true;
			if (_spr_renderer != null)
			{
				int spriteOrderN = GetSpriteOrderN(vector2);
				_spr_renderer.sortingOrder = spriteOrderN;
			}
		}
		else
		{
			if (_world_part == null)
			{
				_world_part = GetComponentInChildren<WorldObjectPart>();
			}
			float num3 = floor_line;
			if ((bool)_world_part)
			{
				num3 += (float)_world_part.floor_line;
			}
			float num4 = num3 / 96f;
			if (_spr_renderer != null)
			{
				_spr_renderer.sortingOrder = 0;
			}
			vector3.z = (_force_z_mode ? _force_z : ((vector3.y + num4) * num + vector3.x * 0.02f + num2));
		}
		if (flag)
		{
			vector3.z += (float)fine_tune_z / 100f;
		}
		vector3.z /= 96f;
		transform.position = vector3 * 96f;
	}

	public static bool DoesSpriteBelongToGround(SpriteRenderer zspr)
	{
		if (zspr == null)
		{
			return false;
		}
		int sortingLayerID = zspr.sortingLayerID;
		if (_sprite_layers_are_ground.ContainsKey(sortingLayerID))
		{
			return _sprite_layers_are_ground[sortingLayerID];
		}
		string sortingLayerName = zspr.sortingLayerName;
		bool flag = sortingLayerName.Contains("behind ground") || sortingLayerName.Contains("ground_") || sortingLayerName == "drops" || sortingLayerName.Contains("on_ground");
		_sprite_layers_are_ground.Add(sortingLayerID, flag);
		return flag;
	}

	private void TryApplySortingToParentSprites(int order)
	{
	}

	private SpriteRenderer GetZeroLevelSprite()
	{
		if (!Application.isPlaying)
		{
			return GetComponent<SpriteRenderer>();
		}
		if (_tried_to_find_zero_level_spr)
		{
			return _zero_level_spr;
		}
		_tried_to_find_zero_level_spr = true;
		_zero_level_spr = GetComponent<SpriteRenderer>();
		return _zero_level_spr;
	}

	public void SetZ()
	{
		_force_z_mode = false;
		Update();
	}

	public void SetZ(float z)
	{
		_force_z_mode = true;
		_force_z = z;
		Update();
	}

	public void OnChangedSprite()
	{
		_world_part = null;
	}

	public static int GetSpriteOrderN(Vector2 pos)
	{
		return -Mathf.RoundToInt(pos.y / 96f * 10f);
	}

	public static void DisableComponentOnStaticObjects()
	{
		RoundAndSortComponent[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<RoundAndSortComponent>(includeInactive: true);
		Debug.Log("DisableComponentOnStaticObjects, count = " + componentsInChildren.Length);
		RoundAndSortComponent[] array = componentsInChildren;
		foreach (RoundAndSortComponent roundAndSortComponent in array)
		{
			if (!roundAndSortComponent.never_disable)
			{
				roundAndSortComponent._chnk = roundAndSortComponent.GetComponent<ChunkedGameObject>();
				if (roundAndSortComponent._chnk != null)
				{
					roundAndSortComponent._chnk.RecalculateChunk();
				}
				WorldGameObject component = roundAndSortComponent.GetComponent<WorldGameObject>();
				if (component == null)
				{
					roundAndSortComponent.enabled = false;
				}
				else if (!component.is_player && !component.components.character.enabled)
				{
					roundAndSortComponent.enabled = false;
				}
			}
		}
	}

	public void MarkPositionDirty()
	{
		_pos_dirty = true;
	}
}
