using UnityEngine;

[ExecuteInEditMode]
public class GroundObject : MonoBehaviour
{
	private SpriteRenderer _spr_renderer;

	private bool _need_reset_haschanged;

	private bool _can_move = true;

	public bool can_move
	{
		set
		{
			if (!value && _can_move)
			{
				Update();
			}
			_can_move = value;
		}
	}

	public void Update()
	{
		if (!_can_move || !base.transform.hasChanged)
		{
			return;
		}
		Vector3 position = base.transform.position;
		position.z = GetGroundZ(position);
		base.transform.position = position;
		if (_spr_renderer == null)
		{
			_spr_renderer = GetComponent<SpriteRenderer>();
		}
		if (_spr_renderer != null)
		{
			if (!RoundAndSortComponent.DoesSpriteBelongToGround(_spr_renderer))
			{
				_spr_renderer.sortingLayerName = "on_ground";
			}
			_spr_renderer.sortingOrder = RoundAndSortComponent.GetSpriteOrderN(position);
		}
		_need_reset_haschanged = true;
	}

	public void LateUpdate()
	{
		if (_can_move && Application.isPlaying && _need_reset_haschanged)
		{
			_need_reset_haschanged = false;
			base.transform.hasChanged = false;
		}
	}

	public static float GetGroundZ(Vector2 pos)
	{
		return 2000f + pos.x / 96f * 0.001f;
	}
}
