using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearGames.Preloader;

public class LBPreloader : MonoBehaviour
{
	private enum State
	{
		Paused,
		FadeIn,
		FadeOut,
		ShowingLogo,
		FinishingCoroutine
	}

	public delegate IEnumerator PreloaderCoroutine();

	public bool autostart = true;

	public float fade_time = 0.35f;

	public SpriteRenderer fading_sprite;

	public List<LBPreloaderLogo> logos = new List<LBPreloaderLogo>();

	private int _cur_logo;

	private State _state;

	private float _time;

	private UnityEngine.Object _cur_obj;

	private PreloaderCoroutine _preloader_coroutine;

	private IEnumerator _enumerator;

	private static LBPreloader _me;

	private bool _coroutine_can_be_left_unfinished;

	private bool _is_animation_in_progress;

	private Action _on_finished_logos;

	public void Start()
	{
		if (autostart)
		{
			StartAnimations();
		}
		_me = this;
	}

	public void Update()
	{
		if (_state == State.Paused)
		{
			return;
		}
		_time += Time.deltaTime;
		float num = Mathf.Min(1f, _time / fade_time);
		Color color = fading_sprite.color;
		switch (_state)
		{
		case State.FadeIn:
			if (_time >= fade_time)
			{
				_state = State.ShowingLogo;
				_time = 0f;
			}
			color.a = 1f - num;
			fading_sprite.color = color;
			break;
		case State.FadeOut:
			if (_time >= fade_time)
			{
				ProceedToNextLogo();
			}
			color.a = num;
			fading_sprite.color = color;
			break;
		case State.ShowingLogo:
			if (!_is_animation_in_progress)
			{
				ProcessCoroutineStep();
			}
			if (_time >= logos[_cur_logo].time)
			{
				_state = State.FadeOut;
				_time = 0f;
			}
			break;
		case State.FinishingCoroutine:
			ProcessCoroutineStep();
			if (_preloader_coroutine == null)
			{
				OnAllLogosShown();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case State.Paused:
			break;
		}
	}

	private void ProcessCoroutineStep()
	{
		if (_preloader_coroutine != null)
		{
			if (_enumerator == null)
			{
				_enumerator = _preloader_coroutine();
			}
			else if (!_enumerator.MoveNext())
			{
				Debug.Log("Finished preloader coroutine");
				_preloader_coroutine = null;
				_enumerator = null;
			}
		}
	}

	public void SetOnFinishedDelegate(Action on_finished)
	{
		_on_finished_logos = on_finished;
	}

	public void SetPreloaderCouroutine(PreloaderCoroutine coroutine, bool coroutine_can_be_left_unfinished = false)
	{
		if (coroutine_can_be_left_unfinished)
		{
			Debug.LogError("coroutine_can_be_left_unfinished==true is not implemented!");
		}
		_preloader_coroutine = coroutine;
		_enumerator = null;
		_coroutine_can_be_left_unfinished = coroutine_can_be_left_unfinished;
	}

	public void StartAnimations()
	{
		_cur_logo = -1;
		_is_animation_in_progress = false;
		ProceedToNextLogo();
	}

	private void ProceedToNextLogo()
	{
		if (_cur_obj != null)
		{
			UnityEngine.Object.Destroy(_cur_obj);
		}
		if (++_cur_logo >= logos.Count)
		{
			OnAllLogosShown();
			return;
		}
		_state = State.FadeIn;
		_time = 0f;
		if (logos[_cur_logo].obj is Texture2D)
		{
			GameObject gameObject = new GameObject("sprite");
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			_cur_obj = gameObject;
			Texture2D texture2D = logos[_cur_logo].obj as Texture2D;
			gameObject.AddComponent<SpriteRenderer>().sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 1f);
		}
		else
		{
			_cur_obj = UnityEngine.Object.Instantiate(logos[_cur_logo].obj, base.transform, instantiateInWorldSpace: false);
		}
		(_cur_obj as GameObject).transform.localScale = Vector3.one * logos[_cur_logo].scale;
	}

	private void OnAllLogosShown()
	{
		if (!_coroutine_can_be_left_unfinished && _preloader_coroutine != null)
		{
			_state = State.FinishingCoroutine;
			return;
		}
		_state = State.Paused;
		if (_on_finished_logos != null)
		{
			_on_finished_logos();
		}
	}

	public static void OnAnimationStarted()
	{
		if (_me == null)
		{
			Debug.LogError("Calling a static LBPreloader.OnAnimationStarted() method without previously running initialize.");
		}
		else
		{
			_me._is_animation_in_progress = true;
		}
	}

	public static void OnAnimationStopped()
	{
		if (_me == null)
		{
			Debug.LogError("Calling a static LBPreloader.OnAnimationStopped() method without previously running initialize.");
		}
		else
		{
			_me._is_animation_in_progress = false;
		}
	}
}
