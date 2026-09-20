using System;
using System.Collections.Generic;

[Serializable]
public class SmartConditionsList
{
	public List<SmartCondition> conditions = new List<SmartCondition>();

	private bool _cached;

	private bool _single;

	private SmartCondition _single_cnd;

	public bool CheckCondition()
	{
		if (!_cached)
		{
			_cached = true;
			_single = conditions.Count == 1;
			if (_single)
			{
				_single_cnd = conditions[0];
			}
		}
		if (_single)
		{
			return _single_cnd.CheckCondition();
		}
		for (int i = 0; i < conditions.Count; i++)
		{
			if (!conditions[i].CheckCondition())
			{
				return false;
			}
		}
		return true;
	}
}
