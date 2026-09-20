using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LogicData
{
	public string id;

	[SerializeField]
	private float _next_execution_time;

	[SerializeField]
	private bool _started;

	private bool _executing;

	private List<string> _running_scripts = new List<string>();

	private List<string> _waiting_finish_scripts = new List<string>();

	private LogicDefinition _definition;

	public LogicDefinition definition
	{
		get
		{
			if (_definition == null)
			{
				_definition = GameBalance.me.GetData<LogicDefinition>(id);
			}
			return _definition;
		}
	}

	public LogicData()
	{
	}

	public LogicData(string id)
	{
		this.id = id;
		_next_execution_time = GetNextExecutionTime();
	}

	private float GetNextExecutionTime()
	{
		if (string.IsNullOrEmpty(id))
		{
			return 0f;
		}
		if (definition == null)
		{
			Debug.LogError("Definition for logic \"" + id + "\" is NULL!");
			return 0f;
		}
		float num = (Mathf.Floor(Mathf.Max(0f - definition.period_time, MainGame.game_time - definition.start_time) / definition.period_time) + 1f) * definition.period_time + definition.start_time;
		if (_started && Mathf.Abs(num - _next_execution_time) < definition.period_time - 0.01f)
		{
			Debug.LogWarning("Something wrong with GameLogics \"" + id + "\".[" + num + " <=> " + _next_execution_time + " ]");
			num += definition.period_time;
		}
		return num;
	}

	public void Update(float delta_time)
	{
		if (!CheckPeriod(delta_time))
		{
			return;
		}
		foreach (SmartExpression expression in definition.expressions)
		{
			expression.Evaluate();
		}
		if (!definition.condition.has_expression || definition.condition.EvaluateChance())
		{
			Execute();
		}
	}

	public bool ForceExecute(bool check_condition)
	{
		if (_executing)
		{
			return false;
		}
		foreach (SmartExpression expression in definition.expressions)
		{
			expression.Evaluate();
		}
		if (check_condition && !definition.condition.EvaluateChance())
		{
			return false;
		}
		_next_execution_time = GetNextExecutionTime();
		Execute();
		return true;
	}

	public void Execute()
	{
		_started = true;
		if (_executing)
		{
			Debug.LogError("<color=red>Execute logic error:</color> " + definition.id + " is now already executing");
			return;
		}
		Debug.Log("<color=yellow>Execute logic:</color> \"" + definition.id + "\" at " + MainGame.game_time);
		foreach (SmartExpression execute_expression in definition.execute_expressions)
		{
			execute_expression.Evaluate();
		}
		RunScripts(definition.execute_scripts);
		if (definition.execute_events != null && definition.execute_events.Count > 0)
		{
			foreach (LogicDefinition.ExecutableEvent execute_event in definition.execute_events)
			{
				List<WorldGameObject> worldGameObjectsByCustomTag = WorldMap.GetWorldGameObjectsByCustomTag(execute_event.custom_tag);
				if (worldGameObjectsByCustomTag == null || worldGameObjectsByCustomTag.Count == 0)
				{
					continue;
				}
				foreach (WorldGameObject item in worldGameObjectsByCustomTag)
				{
					WorldGameObject t_wgo = item;
					LogicDefinition.ExecutableEvent t_ev = execute_event;
					GJTimer.AddTimer(0.01f, delegate
					{
						t_wgo.FireEvent(t_ev.event_name);
					});
				}
			}
		}
		if (definition.condition_1.has_expression && definition.condition_1.EvaluateChance())
		{
			RunScripts(definition.scripts_1);
			if (definition.condition_2.has_expression && definition.condition_2.EvaluateChance())
			{
				RunScripts(definition.scripts_2);
			}
		}
	}

	private bool CheckPeriod(float delta_time)
	{
		if (_next_execution_time.EqualsTo(0f))
		{
			return false;
		}
		float game_time = MainGame.game_time;
		if (_next_execution_time > game_time)
		{
			return false;
		}
		if (definition.period_time > 0.01f)
		{
			_next_execution_time = GetNextExecutionTime();
			return true;
		}
		return !_started;
	}

	private void RunScripts(List<string> scripts)
	{
		if (scripts.Count == 1 && scripts[0][0] == ':')
		{
			FlowScriptEngine.SendEvent(scripts[0].Substring(1));
			return;
		}
		if (scripts.Count > 0)
		{
			_executing = true;
			foreach (string script in scripts)
			{
				if (script != ">")
				{
					_waiting_finish_scripts.Add(script);
				}
			}
		}
		foreach (string script2 in scripts)
		{
			if (script2 == ">")
			{
				break;
			}
			RunScript(script2, scripts);
		}
	}

	private void OnScriptFinished(List<string> scripts_list, string script)
	{
		_running_scripts.Remove(script);
		_waiting_finish_scripts.Remove(script);
		if (_waiting_finish_scripts.Count == 0)
		{
			_executing = false;
		}
		int num = scripts_list.IndexOf(script) + 1;
		if (num < scripts_list.Count - 1 && !(scripts_list[num] != ">"))
		{
			for (int i = num + 1; i < scripts_list.Count && !(scripts_list[i] == ">"); i++)
			{
				RunScript(scripts_list[i], scripts_list);
			}
		}
	}

	private void RunScript(string script, List<string> scripts)
	{
		if (GS.RunFlowScript(script, delegate(string finished_script)
		{
			OnScriptFinished(scripts, finished_script);
		}) != null)
		{
			_running_scripts.Add(script);
			return;
		}
		_waiting_finish_scripts.Remove(script);
		if (_waiting_finish_scripts.Count == 0)
		{
			_executing = false;
		}
	}
}
