using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameLogics
{
	[SerializeField]
	private List<LogicData> _logics = new List<LogicData>();

	[SerializeField]
	private List<string> _black_list = new List<string>();

	[SerializeField]
	private bool _initialized;

	public void Init()
	{
		List<LogicData> list = new List<LogicData>();
		foreach (LogicDefinition logics_datum in GameBalance.me.logics_data)
		{
			list.Add(GetLogicByID(logics_datum.id) ?? new LogicData(logics_datum.id));
		}
		_logics = list;
		_initialized = true;
	}

	private LogicData GetLogicByID(string id)
	{
		foreach (LogicData logic in _logics)
		{
			if (logic.id == id)
			{
				return logic;
			}
		}
		return null;
	}

	public void Update()
	{
		if (!_initialized || !MainGame.game_started || MainGame.paused || (EnvironmentEngine.me != null && EnvironmentEngine.me.IsTimeStopped()))
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		LogicData logicData = null;
		foreach (LogicData logic in _logics)
		{
			if (!_black_list.Contains(logic.id))
			{
				if (logic.definition == null)
				{
					Debug.LogError("Logic definition not found for id = " + logic.id + ", removing logics");
					logicData = logic;
					break;
				}
				logic.Update(deltaTime);
			}
		}
		if (logicData != null)
		{
			_logics.Remove(logicData);
		}
	}

	public bool AddToBlackList(string logic_id)
	{
		if (_black_list.Contains(logic_id))
		{
			return false;
		}
		_black_list.Add(logic_id);
		return true;
	}

	public bool ForceExecute(string id)
	{
		return ForceExecute(id, check_condition: false);
	}

	public bool ForceExecuteCond(string id)
	{
		return ForceExecute(id, check_condition: true);
	}

	private bool ForceExecute(string id, bool check_condition)
	{
		if (check_condition && _black_list.Contains(id))
		{
			return false;
		}
		LogicData logicByID = GetLogicByID(id);
		if (logicByID == null)
		{
			Debug.LogError("Force logic execute fail, no logic with id: " + id);
			return false;
		}
		return logicByID.ForceExecute(check_condition);
	}
}
