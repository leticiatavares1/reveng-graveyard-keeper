using System;
using UnityEngine;

public class GJTimer : MonoBehaviour
{
	public delegate void VoidDelegate();

	public delegate bool BoolDelegate();

	private float _end_time;

	private VoidDelegate _delegate;

	private VoidDelegate _on_update;

	private BoolDelegate _condition;

	private bool _active = true;

	private int _id;

	private bool _just_added = true;

	public static GJTimer AddTimer(float seconds, VoidDelegate dlgt)
	{
		if (dlgt == null)
		{
			Debug.LogError("Trying to add timer with void delegate!");
			return null;
		}
		GJTimer gJTimer = new GameObject("GJTimer " + seconds + " s.").AddComponent<GJTimer>();
		gJTimer.Init(seconds, dlgt);
		return gJTimer;
	}

	public static GJTimer AddConditionalChecker(BoolDelegate condition, VoidDelegate on_update, VoidDelegate on_done)
	{
		GJTimer gJTimer = new GameObject("GJTimer condition").AddComponent<GJTimer>();
		gJTimer._condition = condition;
		gJTimer._delegate = on_done;
		gJTimer._on_update = on_update;
		return gJTimer;
	}

	protected void Init(float time, VoidDelegate dlgt)
	{
		_end_time = Time.time + time;
		_delegate = dlgt;
		_active = true;
		_just_added = true;
		_id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}

	public void AddTime(float time)
	{
		_end_time += time;
	}

	public void Stop()
	{
		if (_active)
		{
			base.enabled = false;
			NGUITools.Destroy(base.gameObject);
			_active = false;
		}
	}

	private void Update()
	{
		if (_condition != null)
		{
			if (_on_update != null)
			{
				_on_update();
			}
			if (_condition())
			{
				OnComplete();
			}
		}
		else if (_just_added)
		{
			_just_added = false;
		}
		else if (_active && Time.time >= _end_time)
		{
			OnComplete();
		}
	}

	public void OnComplete()
	{
		if (!_active)
		{
			return;
		}
		try
		{
			if (_delegate != null)
			{
				_delegate();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("FATAL ERROR: Exception while OnComplete GJTimer: " + ex);
		}
		Stop();
	}

	public static void StopAllTimers()
	{
		GJTimer[] array = UnityEngine.Object.FindObjectsOfType<GJTimer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}

	public static void ForceAllTimersComplete()
	{
		GJTimer[] array = UnityEngine.Object.FindObjectsOfType<GJTimer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnComplete();
		}
	}
}
