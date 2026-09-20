using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using UnityEngine.Events;

public static class CameraTools
{
	private static ProCamera2D _pro_cam;

	private static ProCamera2DCinematics _cinematics;

	private static List<Transform> _cinematic_targets = new List<Transform>();

	private static ProCamera2DTransitionsFX _transitions_fx;

	private static ProCamera2DLetterbox _letterbox;

	private static ProCamera2DNumericBoundaries _boundaries;

	private static Transform _tf;

	private static bool _cached;

	private static bool _playing_transition;

	private static List<CameraTarget> _stored_camera_targets = new List<CameraTarget>();

	private static bool _camera_flyed_to_target = false;

	public static ProCamera2D pro_cam
	{
		get
		{
			if (!_cached)
			{
				CacheComponents();
			}
			return _pro_cam;
		}
	}

	private static Transform tf
	{
		get
		{
			if (!_cached)
			{
				CacheComponents();
			}
			return _tf;
		}
	}

	public static ProCamera2DCinematics cinematics
	{
		get
		{
			if (!_cached)
			{
				CacheComponents();
			}
			return _cinematics;
		}
	}

	public static ProCamera2DTransitionsFX transitions_fx
	{
		get
		{
			if (!_cached)
			{
				CacheComponents();
			}
			return _transitions_fx;
		}
	}

	public static ProCamera2DLetterbox letterbox
	{
		get
		{
			if (!_cached)
			{
				CacheComponents();
			}
			return _letterbox;
		}
	}

	public static ProCamera2DNumericBoundaries boundaries
	{
		get
		{
			if (!_cached)
			{
				CacheComponents();
			}
			return _boundaries;
		}
	}

	public static void ReCache()
	{
		_cached = false;
		CacheComponents();
	}

	private static void CacheComponents()
	{
		_pro_cam = MainGame.me.GetComponent<ProCamera2D>();
		_tf = _pro_cam.transform;
		_cinematics = _tf.GetComponent<ProCamera2DCinematics>();
		_transitions_fx = _tf.GetComponent<ProCamera2DTransitionsFX>();
		_letterbox = _tf.GetComponent<ProCamera2DLetterbox>() ?? _tf.gameObject.AddComponent<ProCamera2DLetterbox>();
		_boundaries = _tf.GetComponent<ProCamera2DNumericBoundaries>();
		_cached = true;
	}

	public static void PlayCinematics(WorldGameObject obj, float ease_in_duration = 1f, float hold_duration = 1f, GJCommons.VoidDelegate on_ended = null)
	{
		PlayCinematics(obj.tf, ease_in_duration, hold_duration, on_ended);
	}

	public static void PlayCinematics(Transform tf, float ease_in_duration = 1f, float hold_duration = 1f, GJCommons.VoidDelegate on_ended = null)
	{
		if (cinematics.IsPlaying)
		{
			Debug.LogError("Cinematics already playing!");
			on_ended.TryInvoke();
			return;
		}
		if (!_cinematic_targets.Contains(tf))
		{
			_cinematic_targets.Add(tf);
			cinematics.AddCinematicTarget(tf, ease_in_duration, hold_duration);
		}
		cinematics.OnCinematicFinished = new UnityEvent();
		cinematics.OnCinematicFinished.AddListener(delegate
		{
			StopCinematics();
			on_ended.TryInvoke();
		});
		cinematics.Play();
	}

	public static void StopCinematics()
	{
		foreach (Transform cinematic_target in _cinematic_targets)
		{
			cinematics.RemoveCinematicTarget(cinematic_target);
		}
		_cinematic_targets.Clear();
		cinematics.Stop();
	}

	public static void PlayFade(bool fade_in, GJCommons.VoidDelegate on_complete = null, float? time = null, Color? color = null)
	{
		if (_playing_transition)
		{
			Debug.LogError("Can't PlayFade: Cinematics already playing!");
			on_complete.TryInvoke();
			return;
		}
		if (transitions_fx == null)
		{
			Debug.LogError("null transition fx!");
			return;
		}
		_playing_transition = true;
		Color color2 = color ?? Color.black;
		if (fade_in)
		{
			transitions_fx.BackgroundColorExit = color2;
			transitions_fx.UpdateTransitionsColor();
			transitions_fx.TransitionExit(time);
			transitions_fx.OnTransitionExitEnded = delegate
			{
				_playing_transition = false;
				on_complete.TryInvoke();
			};
		}
		else
		{
			transitions_fx.BackgroundColorEnter = color2;
			transitions_fx.UpdateTransitionsColor();
			transitions_fx.TransitionEnter(time);
			transitions_fx.OnTransitionEnterEnded = delegate
			{
				_playing_transition = false;
				on_complete.TryInvoke();
			};
		}
	}

	public static void TweenLetterbox(bool show)
	{
		Debug.Log("Camera letterbox = " + show);
		letterbox.TweenTo(show ? cinematics.LetterboxAmount : 0f, cinematics.LetterboxAnimDuration);
	}

	public static void Fade(GJCommons.VoidDelegate on_complete = null, float? time = null, Color? color = null)
	{
		PlayFade(fade_in: true, on_complete, time, color);
	}

	public static void UnFade(GJCommons.VoidDelegate on_complete = null, float? time = null, Color? color = null)
	{
		PlayFade(fade_in: false, on_complete, time, color);
	}

	public static void MoveToPos(Vector2 pos)
	{
		ProCamera2D.Instance.MoveCameraInstantlyToPosition(pos);
	}

	public static void StoreCameraTargets()
	{
		_stored_camera_targets = new List<CameraTarget>(pro_cam.CameraTargets);
		Debug.Log("Store camera targets: Count = " + _stored_camera_targets.Count);
	}

	public static void RestoreCameraTargets(float duration = 0.7f)
	{
		if (_stored_camera_targets == null || _stored_camera_targets.Count == 0)
		{
			AddToCameraTargets(MainGame.me.player.transform, remove_others: true, duration);
		}
		else
		{
			pro_cam.CameraTargets = _stored_camera_targets;
			if (pro_cam.CameraTargets.Count > 0)
			{
				pro_cam.CameraTargets[0].TargetInfluence = 1f;
			}
		}
		_stored_camera_targets = new List<CameraTarget>();
		Debug.Log("Restore camera targets.Count = " + pro_cam.CameraTargets.Count);
	}

	public static void AddToCameraTargets(Transform transform, bool remove_others = true, float duration = 0.7f)
	{
		if (remove_others)
		{
			while (pro_cam.CameraTargets.Count > 0)
			{
				pro_cam.CameraTargets.RemoveAt(0);
			}
		}
		if ((double)Mathf.Abs(duration) < 0.01)
		{
			MoveToPos(transform.position);
		}
		pro_cam.AddCameraTarget(transform, 10000f, 10000f, duration);
	}

	public static void CameraCinematicFly(Transform transform, GJCommons.VoidDelegate on_finished, float duration = 0.7f)
	{
		if (!_camera_flyed_to_target)
		{
			_camera_flyed_to_target = true;
			StoreCameraTargets();
		}
		pro_cam.CameraTargets[0].TargetInfluence = 1f;
		CameraTarget cameraTarget = new CameraTarget
		{
			TargetTransform = transform,
			TargetInfluenceH = 0f,
			TargetInfluenceV = 0f,
			TargetOffset = Vector2.zero
		};
		pro_cam.CameraTargets.Add(cameraTarget);
		pro_cam.StartCoroutine(pro_cam.AdjustTargetInfluenceRoutine(cameraTarget, 1f, 1f, duration));
		pro_cam.StartCoroutine(pro_cam.AdjustTargetInfluenceRoutine(pro_cam.CameraTargets[0], 0f, 0f, duration, removeIfZeroInfluence: true));
		GJTimer.AddTimer(duration + 1f, on_finished.TryInvoke);
	}

	public static void RemoveFromCameraTargets(Transform transform, float duration = 0.7f)
	{
		pro_cam.RemoveCameraTarget(transform, duration);
	}

	public static void CameraFlyTo(Transform target, GJCommons.VoidDelegate on_finished = null, float duration = 0.7f)
	{
		if (!_camera_flyed_to_target)
		{
			_camera_flyed_to_target = true;
			StoreCameraTargets();
		}
		else
		{
			Debug.Log("Camera change target object second time!");
		}
		if ((double)Mathf.Abs(duration) < 0.01)
		{
			AddToCameraTargets(target, remove_others: true, duration);
			on_finished.TryInvoke();
		}
		else
		{
			AddToCameraTargets(target, remove_others: true, duration);
			GJTimer.AddTimer(1f, on_finished.TryInvoke);
		}
	}

	public static void CameraFlyBack(GJCommons.VoidDelegate on_finished = null, float duration = 0.7f)
	{
		if (_camera_flyed_to_target)
		{
			_camera_flyed_to_target = false;
			RestoreCameraTargets();
		}
		else
		{
			Debug.LogError("Call CameraFlyBack when camera not changed target!");
		}
		GJTimer.AddTimer(1f, on_finished.TryInvoke);
	}

	public static void Init()
	{
		_camera_flyed_to_target = false;
	}
}
