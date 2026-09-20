using System;
using System.Collections.Generic;

public static class MimicAnimationController
{
	private static Action _event = null;

	private static long _last_id = 0L;

	private static Dictionary<long, Action> _listeners_in_progress = new Dictionary<long, Action>();

	private static List<long> _dont_timer = new List<long>();

	public static void Init()
	{
		_event = null;
		_listeners_in_progress.Clear();
		_dont_timer.Clear();
	}
}
