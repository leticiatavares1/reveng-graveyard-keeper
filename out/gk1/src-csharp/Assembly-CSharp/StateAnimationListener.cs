using System;
using UnityEngine;

public class StateAnimationListener : MonoBehaviour
{
	public Action on_entered;

	public Action on_exit;

	public bool destroy_after_on_exit = true;

	public bool destroyed;

	private GJTimer _workaround_timer;

	private string _error_message = "";

	public void OnEnteredState()
	{
		Debug.Log("OnEnteredState " + base.gameObject.name, this);
		on_entered.TryInvoke();
		if (_workaround_timer != null)
		{
			_workaround_timer.AddTime(4f);
		}
	}

	public void OnExitedState()
	{
		try
		{
			if (base.gameObject != null && destroyed)
			{
				Debug.LogWarning("Already destroyed, skipping. Obj = " + base.gameObject.name, this);
				return;
			}
		}
		catch (Exception)
		{
			Debug.Log("OnExitedState gameObject is null");
			if (_workaround_timer != null)
			{
				_workaround_timer.Stop();
				_workaround_timer = null;
			}
			destroyed = true;
			UnityEngine.Object.Destroy(this);
			return;
		}
		if (destroyed)
		{
			Debug.LogWarning("Already destroyed, skipping. Obj = " + base.gameObject.name, this);
			return;
		}
		Debug.Log("OnExitedState " + base.gameObject.name, this);
		if (_workaround_timer != null)
		{
			_workaround_timer.Stop();
			_workaround_timer = null;
		}
		on_exit.TryInvoke();
		if (destroy_after_on_exit)
		{
			destroyed = true;
			UnityEngine.Object.Destroy(this);
		}
	}

	public void AddWorkaroundTimer(float time, string error_message = "")
	{
		if (!time.EqualsTo(0f))
		{
			_error_message = error_message;
			_workaround_timer = GJTimer.AddTimer(time, delegate
			{
				Debug.Log(_error_message, this);
				OnExitedState();
			});
		}
	}
}
