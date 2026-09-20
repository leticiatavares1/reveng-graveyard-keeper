using System.Collections.Generic;

public static class WorkbenchAdditionWorldIconPresenter
{
	public readonly struct State
	{
		public readonly string IconId;

		public readonly bool IsInRange;

		public State(string iconId, bool isInRange)
		{
			IconId = iconId;
			IsInRange = isInRange;
		}

		public bool Equals(State other)
		{
			if (IsInRange == other.IsInRange)
			{
				return IconId == other.IconId;
			}
			return false;
		}
	}

	private static readonly Dictionary<SGuid, State> states = new Dictionary<SGuid, State>();

	private static readonly Dictionary<SGuid, Wgo> displayedWgos = new Dictionary<SGuid, Wgo>();

	private static readonly List<Wgo> toRedraw = new List<Wgo>();

	public static bool TryGet(SGuid wgoUniqueId, out State state)
	{
		return states.TryGetValue(wgoUniqueId, out state);
	}

	public static void Replace(Dictionary<Wgo, State> newStates)
	{
		toRedraw.Clear();
		foreach (KeyValuePair<Wgo, State> newState in newStates)
		{
			Wgo key = newState.Key;
			if ((bool)key && key.Data != null)
			{
				SGuid uniqueId = key.Data.UniqueId;
				if (!states.TryGetValue(uniqueId, out var value) || !value.Equals(newState.Value))
				{
					toRedraw.Add(key);
				}
			}
		}
		foreach (KeyValuePair<SGuid, Wgo> displayedWgo in displayedWgos)
		{
			Wgo value2 = displayedWgo.Value;
			if ((bool)value2 && value2.Data != null && !newStates.ContainsKey(value2))
			{
				toRedraw.Add(value2);
			}
		}
		states.Clear();
		displayedWgos.Clear();
		foreach (KeyValuePair<Wgo, State> newState2 in newStates)
		{
			Wgo key2 = newState2.Key;
			if ((bool)key2 && key2.Data != null)
			{
				states[key2.Data.UniqueId] = newState2.Value;
				displayedWgos[key2.Data.UniqueId] = key2;
			}
		}
		for (int i = 0; i < toRedraw.Count; i++)
		{
			Wgo wgo = toRedraw[i];
			if ((bool)wgo && wgo.Data != null)
			{
				wgo.DrawWidgets();
			}
		}
		toRedraw.Clear();
	}

	public static void Clear()
	{
		if (states.Count == 0)
		{
			return;
		}
		toRedraw.Clear();
		foreach (Wgo value in displayedWgos.Values)
		{
			if ((bool)value && value.Data != null)
			{
				toRedraw.Add(value);
			}
		}
		states.Clear();
		displayedWgos.Clear();
		for (int i = 0; i < toRedraw.Count; i++)
		{
			Wgo wgo = toRedraw[i];
			if ((bool)wgo && wgo.Data != null)
			{
				wgo.DrawWidgets();
			}
		}
		toRedraw.Clear();
	}
}
