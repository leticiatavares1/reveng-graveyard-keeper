using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VisibilitySector : MonoBehaviour
{
	private const int DELTA_STEP = 20;

	public float size;

	[Range(0f, 1f)]
	public float start_width;

	[Range(0f, 360f)]
	public int angle;

	[Range(0f, 180f)]
	public int delta;

	[Space]
	public bool draw_debug = true;

	[SerializeField]
	private List<Vector2> raw_dots = new List<Vector2>();

	[SerializeField]
	private List<Vector2> dots = new List<Vector2>();

	[SerializeField]
	[HideInInspector]
	private PolygonCollider2D _col;

	private BaseCharacterComponent _ch;

	private float _angle;

	private Transform _tf;

	private bool _tf_cached;

	private bool _ch_initialized;

	public Transform tf
	{
		get
		{
			if (!_tf_cached)
			{
				return this.Cache<Transform>(out _tf, out _tf_cached, deep: false);
			}
			return _tf;
		}
	}

	public Vector2 pos => tf.position;

	private void OnValidate()
	{
		delta = delta / 20 * 20;
		SetRawPositions();
	}

	public void Init(BaseCharacterComponent ch)
	{
		_ch = ch;
		_ch_initialized = _ch != null;
	}

	public bool IsTouching(Vector3 other_pos, bool ignore_obstacles)
	{
		CalcPositions(_ch_initialized ? _ch.anim_dir_angle : 0f);
		if (!_col.OverlapPoint(other_pos))
		{
			return false;
		}
		if (ignore_obstacles)
		{
			return true;
		}
		Vector2 vector = other_pos - base.transform.position;
		return Physics2D.RaycastAll(pos, vector.normalized, vector.magnitude, 1).Length == 0;
	}

	private void SetRawPositions()
	{
		Vector2 vector = Vector2.left * start_width / 2f;
		Vector2 vector2 = Vector2.right * start_width / 2f;
		if (raw_dots == null)
		{
			raw_dots = new List<Vector2>();
		}
		else
		{
			raw_dots.Clear();
		}
		raw_dots.Add(vector);
		List<Vector2> list = new List<Vector2>();
		List<Vector2> list2 = new List<Vector2>();
		Vector2 vec = Vector2.down * size;
		for (float num = 0f; num <= (float)(delta - 20); num += 20f)
		{
			list2.Add(Rotate(vec, Sin(0f - num), Cos(0f - num)) + vector);
			list.Add(Rotate(vec, Sin(num), Cos(num)) + vector2);
		}
		list2.Add(Rotate(vec, Sin(-delta), Cos(-delta)) + vector);
		list.Add(Rotate(vec, Sin(delta), Cos(delta)) + vector2);
		for (int num2 = list2.Count - 1; num2 >= 0; num2--)
		{
			raw_dots.Add(list2[num2]);
		}
		foreach (Vector2 item in list)
		{
			raw_dots.Add(item);
		}
		raw_dots.Add(vector2);
		_angle = float.MinValue;
	}

	private void CalcPositions(float obj_angle)
	{
		if (_angle.EqualsTo((float)angle + obj_angle))
		{
			return;
		}
		_angle = (float)angle + obj_angle;
		if (dots == null)
		{
			dots = new List<Vector2>();
		}
		else
		{
			dots.Clear();
		}
		float sin = Sin(_angle);
		float cos = Cos(_angle);
		foreach (Vector2 raw_dot in raw_dots)
		{
			dots.Add(Rotate(raw_dot, Vector2.zero, sin, cos));
		}
		_col = GetComponent<PolygonCollider2D>();
		if (_col == null)
		{
			_col = base.gameObject.AddComponent<PolygonCollider2D>();
		}
		_col.isTrigger = true;
		_col.points = dots.ToArray();
		for (int i = 0; i < dots.Count; i++)
		{
			dots[i] = ToWorldPos(dots[i]);
		}
	}

	private float Sin(float a)
	{
		return Mathf.Sin(a * ((float)Math.PI / 180f));
	}

	private float Cos(float a)
	{
		return Mathf.Cos(a * ((float)Math.PI / 180f));
	}

	private Vector2 ToWorldPos(Vector2 vec)
	{
		return vec * 96f + pos;
	}

	private Vector2 Rotate(Vector2 vec, float sin, float cos)
	{
		return new Vector2(vec.x * cos - vec.y * sin, vec.x * sin + vec.y * cos);
	}

	private Vector2 Rotate(Vector2 vec, Vector2 axis, float sin, float cos)
	{
		return axis + new Vector2((vec.x - axis.x) * cos - (vec.y - axis.y) * sin, (vec.x - axis.x) * sin + (vec.y - axis.y) * cos);
	}

	private void DrawDebug()
	{
		Color magenta = Color.magenta;
		int count = dots.Count;
		for (int i = 0; i < count - 1; i++)
		{
			Debug.DrawLine(dots[i], dots[i + 1], magenta);
		}
		if (count > 2)
		{
			Debug.DrawLine(dots[count - 1], dots[0], magenta);
		}
	}
}
