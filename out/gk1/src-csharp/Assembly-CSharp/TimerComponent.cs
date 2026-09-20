using System.Collections.Generic;
using UnityEngine;

public class TimerComponent : WorldGameObjectComponent
{
	public delegate void OnUpdate(float normalized_time);

	[RuntimeValue(true)]
	[SerializeField]
	private bool _started;

	[RuntimeValue(true)]
	[SerializeField]
	private float _target_time;

	[RuntimeValue(true)]
	[SerializeField]
	private float _played_time;

	[RuntimeValue(true)]
	[SerializeField]
	private float _normalized_time;

	private List<float> _delays;

	private List<GJCommons.VoidDelegate> _delegates;

	public event GJCommons.VoidDelegate on_loop;

	public event OnUpdate on_update;

	public void Play(float time)
	{
		if (_started)
		{
			Debug.LogError("Timer is allready started", base.wgo);
			return;
		}
		if (time.EqualsTo(0f))
		{
			Debug.LogError("Cannot start timer with 0 time", base.wgo);
			return;
		}
		_started = true;
		_target_time = time;
		_played_time = (_normalized_time = 0f);
	}

	public void ClearTimer()
	{
		_started = false;
		_target_time = (_played_time = (_normalized_time = 0f));
	}

	public void SetCallbacks(OnUpdate update, GJCommons.VoidDelegate loop)
	{
		if (!_started)
		{
			Debug.LogWarning("Registering callbacks for not started timer", base.wgo);
		}
		on_update += update;
		on_loop += loop;
	}

	public void RemoveCallbacks(OnUpdate update, GJCommons.VoidDelegate loop)
	{
		on_update -= update;
		on_loop -= loop;
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		if (_delays != null)
		{
			for (int i = 0; i < _delays.Count; i++)
			{
				_delays[i] -= delta_time;
				if (!(_delays[i] > 0f))
				{
					_delegates[i].TryInvoke();
					_delays.RemoveAt(i);
					_delegates.RemoveAt(i);
					i--;
				}
			}
		}
		if (!_started)
		{
			return;
		}
		_played_time += delta_time;
		_normalized_time = _played_time / _target_time;
		if (_normalized_time < 1f)
		{
			if (this.on_update != null)
			{
				this.on_update(_normalized_time);
			}
			return;
		}
		this.on_loop.TryInvoke();
		if (this.on_update != null)
		{
			this.on_update(1f);
		}
		_started = false;
		_target_time = (_played_time = (_normalized_time = 0f));
	}

	protected override int GetExecutionOrder()
	{
		return -1;
	}
}
