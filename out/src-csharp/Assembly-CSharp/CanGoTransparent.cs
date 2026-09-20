using System;
using UnityEngine;

public class CanGoTransparent : MonoBehaviour
{
	private SpriteRenderer[] _sprs;

	private float _a = 1f;

	private float _target_a = 1f;

	private bool _animating;

	private const float ALPHA_CHANGE_SPEED = 10f;

	public const float MIN_X_TRANSPARENT_DIST = 100f;

	public const float MIN_Z_TRANSPARENT_DIST = 50f;

	public void SetAlpha(float a)
	{
		if (!((double)Math.Abs(_target_a - a) < 0.01))
		{
			_target_a = a;
			_animating = true;
		}
	}

	public void Update()
	{
		if (!_animating)
		{
			return;
		}
		float num = _target_a - _a;
		if ((double)Math.Abs(num) < 0.01)
		{
			_animating = false;
			return;
		}
		_a += num * Time.deltaTime * 10f;
		if ((int)Mathf.Sign(_target_a - _a) != (int)Mathf.Sign(num))
		{
			_a = _target_a;
			_animating = false;
		}
		SetSpritesAlpha(_a);
	}

	private void SetSpritesAlpha(float a)
	{
		if (_sprs == null)
		{
			_sprs = base.gameObject.GetComponentsInChildren<SpriteRenderer>();
		}
		SpriteRenderer[] sprs = _sprs;
		foreach (SpriteRenderer obj in sprs)
		{
			Color color = obj.color;
			color.a = a;
			obj.color = color;
		}
	}
}
