using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelGrades
{
	[SerializeField]
	private List<int> _values = new List<int>();

	public string grade_id;

	public int GetLevel(float value)
	{
		return GetLevel(Mathf.Floor(value));
	}

	public int GetLevel(int value)
	{
		for (int i = 0; i < _values.Count; i++)
		{
			if (value < _values[i])
			{
				return i;
			}
		}
		return _values.Count - 1;
	}

	public int GetValueOfLevel(int level)
	{
		if (level >= _values.Count)
		{
			level = _values.Count - 1;
		}
		if (level < 0)
		{
			return 0;
		}
		return _values[level];
	}

	public void AddLevel(int value)
	{
		_values.Add(value);
	}

	public int GetValueToNextLevel(int value)
	{
		int level = GetLevel(value);
		return GetValueOfLevel(level) - value;
	}
}
