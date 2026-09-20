using UnityEngine;

public class TrailObject : MonoBehaviour
{
	private SpriteRenderer _spr;

	public static Transform trails_root;

	private static TrailObject _prefab;

	private bool _is_outside = true;

	private const float DEGRADE_SPEED_OUTSIDE = 0.1f;

	private const float DEGRADE_SPEED_INSIDE = 2.5f;

	private const float DEGRADE_SPEED_OUTSIDE_RAIN = 10f;

	private float _alpha = 1f;

	private float _real_alpha = 1f;

	private bool _degrading;

	private float _last_time;

	public static TrailObject Spawn(Vector2 pos, Sprite spr, bool flip, bool is_outside)
	{
		if (spr == null)
		{
			Debug.LogError("Trial spr is null");
			return null;
		}
		if (_prefab == null)
		{
			_prefab = Resources.Load<TrailObject>("Trails/trail prefab");
			if (_prefab == null)
			{
				Debug.LogError("Couldn't load trails prefab");
			}
		}
		if (trails_root == null)
		{
			trails_root = new GameObject("Trails").transform;
			trails_root.SetParent(MainGame.me.world_root, worldPositionStays: false);
		}
		TrailObject trailObject = Object.Instantiate(_prefab, trails_root, worldPositionStays: false);
		trailObject.transform.position = pos;
		trailObject.Init(spr, flip, is_outside);
		GroundObject groundObject = trailObject?.GetComponentInChildren<GroundObject>();
		if (groundObject != null)
		{
			groundObject.can_move = false;
		}
		return trailObject;
	}

	protected void Init(Sprite spr, bool flip, bool is_outside)
	{
		_spr = base.gameObject.AddComponentNotDuplicate<SpriteRenderer>();
		_spr.sprite = spr;
		_spr.flipX = flip;
		_is_outside = is_outside;
		_last_time = Time.time;
		_degrading = true;
	}

	public void SetColor(Color c, float alpha = 1f)
	{
		c.a = alpha;
		_spr.color = c;
		_alpha = alpha;
		_real_alpha = alpha;
	}

	public void Update()
	{
		if (_degrading)
		{
			float num = 0.1f;
			if (!_is_outside)
			{
				num = 2.5f;
			}
			else if (EnvironmentEngine.me.is_rainy)
			{
				num = 10f;
			}
			float num2 = (Time.time - _last_time) * num / 100f;
			_alpha -= num2;
			_last_time = Time.time;
			if ((double)Mathf.Abs(_real_alpha - _alpha) > 0.03)
			{
				_real_alpha = _alpha;
				_spr.color.SetAlpha(_real_alpha);
			}
			if ((double)_real_alpha < 0.05)
			{
				_degrading = false;
				LeaveTrailComponent.OnTrailObjectDestroyed(this);
				Object.Destroy(base.gameObject);
			}
		}
	}
}
