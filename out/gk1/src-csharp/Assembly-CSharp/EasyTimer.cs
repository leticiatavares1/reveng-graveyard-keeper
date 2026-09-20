using UnityEngine;

public class EasyTimer : MonoBehaviour
{
	public delegate void VoidDelegate();

	private float _end_time;

	private VoidDelegate _delegate;

	private bool _active = true;

	private bool _depends_on_scale;

	public static EasyTimer Add(float seconds, VoidDelegate dlgt, bool depends_on_scale = true)
	{
		if (dlgt == null)
		{
			Debug.LogError("Trying to add timer with void delegate!");
			return null;
		}
		EasyTimer easyTimer = new GameObject("Easy timer " + seconds + " s.").AddComponent<EasyTimer>();
		easyTimer.Init(seconds, dlgt, depends_on_scale);
		return easyTimer;
	}

	protected void Init(float time, VoidDelegate dlgt, bool depends_on_scale)
	{
		_end_time = (depends_on_scale ? Time.time : Time.realtimeSinceStartup) + time;
		_delegate = dlgt;
		_active = true;
		_depends_on_scale = depends_on_scale;
	}

	public void Stop()
	{
		if (_active)
		{
			base.enabled = (_active = false);
			base.gameObject.Destroy();
		}
	}

	private void Update()
	{
		if (_active && ((_depends_on_scale && Time.time >= _end_time) || (!_depends_on_scale && Time.realtimeSinceStartup >= _end_time)))
		{
			OnComplete();
		}
	}

	public void OnComplete()
	{
		if (_active)
		{
			if (_delegate != null)
			{
				_delegate();
			}
			Stop();
		}
	}

	public static void StopAllTimers()
	{
		EasyTimer[] array = Object.FindObjectsOfType<EasyTimer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
	}

	public static void ForceAllTimersComplete()
	{
		EasyTimer[] array = Object.FindObjectsOfType<EasyTimer>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnComplete();
		}
	}
}
