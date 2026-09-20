using System;
using System.Collections.Generic;

[Serializable]
public class QuestDefinition : BalanceBaseObject
{
	public SmartExpression start_trigger = new SmartExpression();

	public SmartExpression success_trigger = new SmartExpression();

	public SmartExpression fail_trigger = new SmartExpression();

	public List<string> start_key;

	public GameRes rew_on_success_params = new GameRes();

	public GameRes rew_on_fail_params = new GameRes();

	public bool one_time_quest;

	public string start_script;

	public string success_script;

	public string fail_script;

	public bool quest_visible;

	public string arrow_wgo_custom_tag;

	public string arrow_wgo_obj_id;

	public List<SmartExpression> success_expressions = new List<SmartExpression>();

	public List<SmartExpression> fail_expressions = new List<SmartExpression>();

	public bool IsReadyToStart(QuestSystem quest_system)
	{
		if (one_time_quest && quest_system != null && quest_system.CheckIfQuestWasExecuted(id))
		{
			return false;
		}
		return start_trigger.EvaluateChance();
	}

	public bool IsSucceed()
	{
		return success_trigger.EvaluateChance();
	}

	public bool IsFailed()
	{
		return fail_trigger.EvaluateChance();
	}

	public void InitQuestStartTriggers()
	{
	}

	public void InitQuestEndTriggers()
	{
	}

	public void StartStartingScripts()
	{
		if (!string.IsNullOrEmpty(start_script))
		{
			GS.RunFlowScript(start_script);
		}
	}

	public void StartSucceedScript()
	{
		if (!string.IsNullOrEmpty(success_script))
		{
			GS.RunFlowScript(success_script);
		}
	}

	public void StartFailedScript()
	{
		if (!string.IsNullOrEmpty(fail_script))
		{
			GS.RunFlowScript(fail_script);
		}
	}

	public void ExpressionsSuccess()
	{
		if (success_expressions == null || success_expressions.Count <= 0)
		{
			return;
		}
		foreach (SmartExpression success_expression in success_expressions)
		{
			success_expression?.Evaluate();
		}
	}

	public void ExpressionsFail()
	{
		if (fail_expressions == null || fail_expressions.Count <= 0)
		{
			return;
		}
		foreach (SmartExpression fail_expression in fail_expressions)
		{
			fail_expression?.Evaluate();
		}
	}
}
