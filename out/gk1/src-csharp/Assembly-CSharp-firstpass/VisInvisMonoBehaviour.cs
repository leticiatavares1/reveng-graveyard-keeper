using System;
using UnityEngine;

public class VisInvisMonoBehaviour : MonoBehaviour
{
	protected bool visible = true;

	[NonSerialized]
	private Transform _tf;

	[NonSerialized]
	private bool _tf_is_set;

	private bool _outside_camera;

	public bool outside_camera_calculations;

	protected bool ignore_unity_became_visible;

	private static int UPDATE_GROUPS = 3;

	private int _update_group = -1;

	protected Transform tf
	{
		get
		{
			if (!_tf_is_set)
			{
				_tf_is_set = true;
				_tf = base.transform;
			}
			return _tf;
		}
	}

	protected bool is_inside_camera => !_outside_camera;

	public void OnBecameVisible()
	{
		if (Application.isPlaying && !ignore_unity_became_visible)
		{
			visible = true;
		}
	}

	public void OnBecameInvisible()
	{
		if (Application.isPlaying && !ignore_unity_became_visible)
		{
			visible = false;
		}
	}

	public bool IsVisible()
	{
		return visible;
	}

	public void Update()
	{
		VisInvisUpdate();
	}

	protected void VisInvisUpdate()
	{
		if (!outside_camera_calculations)
		{
			return;
		}
		if (_update_group == -1)
		{
			_update_group = UnityEngine.Random.Range(0, UPDATE_GROUPS);
		}
		if (Application.isPlaying && Time.frameCount % UPDATE_GROUPS != _update_group)
		{
			return;
		}
		bool flag = IsOutsideCamera();
		if (flag != _outside_camera)
		{
			_outside_camera = flag;
			if (flag)
			{
				OnMovedOutsideCamera();
			}
			else
			{
				OnMovedInsideCamera();
			}
		}
	}

	protected virtual void OnMovedOutsideCamera()
	{
	}

	protected virtual void OnMovedInsideCamera()
	{
	}

	private bool IsOutsideCamera(float max_coord = 1f)
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		Vector3 position = tf.position;
		Vector2 main_camera_pos_v = GJCommons.main_camera_pos_v2;
		float num = (position.x - main_camera_pos_v.x) / (float)Screen.width;
		float num2 = (position.y - main_camera_pos_v.y) / (float)Screen.height;
		if (!(num > max_coord) && !(num2 > max_coord) && !(num < 0f - max_coord))
		{
			return num2 < 0f - max_coord;
		}
		return true;
	}
}
