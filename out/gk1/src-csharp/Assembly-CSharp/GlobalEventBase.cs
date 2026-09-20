using System;
using UnityEngine;

public class GlobalEventBase
{
	public enum GlobalEventPeriodType
	{
		Years = 1
	}

	public DateTime event_start_time;

	public TimeSpan event_duration;

	public int period;

	public GlobalEventPeriodType period_type;

	public IHasExecute on_start_script;

	public IHasExecute on_finish_script;

	public string game_res;

	public GlobalEventBase(string game_res, DateTime event_start_time, TimeSpan event_duration, int period = 1, GlobalEventPeriodType period_type = GlobalEventPeriodType.Years)
	{
		this.game_res = game_res;
		this.event_start_time = event_start_time;
		this.event_duration = event_duration;
		this.period_type = period_type;
		this.period = period;
	}

	public GlobalEventBase(string game_res, DateTime event_start_time, int event_duration, int period = 1, GlobalEventPeriodType period_type = GlobalEventPeriodType.Years)
		: this(game_res, event_start_time, new TimeSpan(event_duration), period, period_type)
	{
	}

	public void Process()
	{
		if (!IsProperTimeForEvent())
		{
			if (MainGame.me.player.GetParamInt(game_res) == 1)
			{
				OnFinishEvent();
			}
		}
		else if (MainGame.me.player.GetParamInt(game_res) == 0)
		{
			OnStartEvent();
		}
	}

	private bool IsProperTimeForEvent()
	{
		DateTime now = DateTime.Now;
		DateTime dateTime = event_start_time;
		DateTime dateTime2 = event_start_time.Add(event_duration);
		if (period_type == GlobalEventPeriodType.Years)
		{
			int year = now.Year;
			dateTime = dateTime.AddYears(year - dateTime.Year);
			dateTime2 = dateTime2.AddYears(year - dateTime2.Year);
			if (dateTime < now)
			{
				return now < dateTime2;
			}
			return false;
		}
		throw new ArgumentOutOfRangeException();
	}

	private void OnStartEvent()
	{
		on_start_script?.Execute();
		MainGame.me.player.SetParam(game_res, 1f);
		Debug.Log("#gevent# Started global event \"" + game_res + "\"");
	}

	private void OnFinishEvent()
	{
		on_finish_script?.Execute();
		MainGame.me.player.SetParam(game_res, 0f);
		Debug.Log("#gevent# Finished global event \"" + game_res + "\"");
	}
}
