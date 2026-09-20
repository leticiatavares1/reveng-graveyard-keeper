using System;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

public class TweenToAMovingTarget : MonoBehaviour
{
	public delegate Transform GetTransformDelegate();

	private GetTransformDelegate _get_target_dlg;

	private float _target_check_period;

	private float _duration;

	private Action _on_complete;

	private float _t;

	private float _check_t;

	private Transform _target;

	private Vector3 _real_target_pos;

	private Vector3 _cur_target_pos;

	private Vector3 _origin_pos;

	public static void DoTweenToAMovingTarget(GameObject obj, GetTransformDelegate get_target, float target_check_period, float duration, Action on_complete)
	{
		TweenToAMovingTarget tweenToAMovingTarget = obj.AddComponent<TweenToAMovingTarget>();
		tweenToAMovingTarget._target = get_target();
		tweenToAMovingTarget._duration = duration;
		tweenToAMovingTarget._target_check_period = target_check_period;
		tweenToAMovingTarget._get_target_dlg = get_target;
		tweenToAMovingTarget._on_complete = on_complete;
		tweenToAMovingTarget._cur_target_pos = (tweenToAMovingTarget._real_target_pos = ((tweenToAMovingTarget._target == null) ? Vector3.zero : tweenToAMovingTarget._target.transform.position));
		tweenToAMovingTarget._origin_pos = obj.transform.position;
	}

	public void Update()
	{
		float deltaTime = Time.deltaTime;
		_t += deltaTime;
		_check_t += deltaTime;
		if (_check_t > _target_check_period)
		{
			_check_t = 0f;
			Transform transform = _get_target_dlg();
			if (transform != null)
			{
				_target = transform;
			}
			if (_target != null)
			{
				_real_target_pos = _target.transform.position;
			}
		}
		Vector3 vector = _real_target_pos - _cur_target_pos;
		_cur_target_pos += vector * deltaTime;
		if (_t > _duration)
		{
			base.transform.position = _real_target_pos;
			UnityEngine.Object.Destroy(this);
			_on_complete.TryInvoke();
		}
		else
		{
			float t = EaseManager.Evaluate(Ease.InOutCubic, null, _t, _duration, 1f, 1f);
			base.transform.position = Vector3.Lerp(_origin_pos, _cur_target_pos, t);
		}
	}
}
