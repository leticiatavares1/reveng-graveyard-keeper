using UnityEngine;

public class ObjectDynamicShadowChild : MonoBehaviour
{
	protected Material mat;

	private SpriteRenderer _spr;

	private bool _spr_is_set;

	[HideInInspector]
	public float shadow_alpha = 1f;

	public int shadow_n;

	private float _angle;

	private float _skew;

	protected bool visible = true;

	public const int LIGHT_COLLIDER_LAYER = 12;

	private Transform _tf;

	private bool _tf_set;

	protected bool is_mirrored;

	private const bool SUN_CASTS_SHADOW = true;

	public void SetShadow(Vector2? light_pos, bool is_mirrored_x, Vector3 pos)
	{
		CheckSpriteCachedReference();
		if (!light_pos.HasValue)
		{
			_spr.color = new Color(0f, 0f, 0f, 0f);
			return;
		}
		Vector2 vector = light_pos.Value - (Vector2)pos;
		if (!is_mirrored_x)
		{
			_angle = Mathf.Atan2(vector.y, vector.x) + LazyConsts.PI_DIV_2;
		}
		else
		{
			_angle = Mathf.Atan2(vector.y, vector.x) + LazyConsts.PI_DIV_2;
			if (_angle < 0f)
			{
				_angle += LazyConsts.PI2;
			}
			if (_angle > LazyConsts.PI2)
			{
				_angle -= LazyConsts.PI2;
			}
		}
		SetShadowAngle(_angle, vector.magnitude);
	}

	public void SetShadowAngle(float angle, float distance, float? override_alpha = null, float vert_scale = 1f)
	{
		if (shadow_n == -1)
		{
			Debug.LogError("Couldn't set shadow for n = -1");
			return;
		}
		CheckSpriteCachedReference();
		_angle = angle;
		float num = override_alpha ?? (Mathf.Max(0.04f, Mathf.Min(1f, 150f / distance)) * TimeOfDay.shadow_alpha_k);
		Color color = _spr.color;
		color.a = shadow_alpha * num;
		_spr.color = color;
		if (!_tf_set)
		{
			_tf = base.transform;
			_tf_set = true;
		}
		_tf.eulerAngles = new Vector3(0f, 0f, _angle * 57.29578f);
	}

	private void CheckSpriteCachedReference()
	{
		if (!_spr_is_set)
		{
			_spr = base.gameObject.GetComponent<SpriteRenderer>();
			_spr_is_set = true;
			mat = _spr.sharedMaterial;
		}
	}

	public virtual void SetShadowSprite(Sprite spr)
	{
		GetComponent<SpriteRenderer>().sprite = spr;
	}

	public void SetShadowByNumber(ObjectDynamicShadow getlight_object, int shadow_n, Vector2 pos, bool is_mirrored)
	{
		if (shadow_n == 0 && Application.isPlaying)
		{
			float angle = TimeOfDay.me.GetTimeK() * LazyConsts.PI2;
			SetShadowAngle(angle, 0.1f, (1f - TimeOfDay.shadow_alpha_k) * TimeOfDay.global_shadows_alpha, Mathf.Abs(TimeOfDay.me.time_of_day) / 4f + 0.75f);
		}
		else
		{
			Vector2? light = getlight_object.GetLight(Application.isPlaying ? (shadow_n - 1) : shadow_n);
			SetShadow(light, getlight_object.is_mirrored, pos);
		}
	}
}
