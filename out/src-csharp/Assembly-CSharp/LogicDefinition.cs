using System;
using System.Collections.Generic;

[Serializable]
public class LogicDefinition : BalanceBaseObject
{
	[Serializable]
	public struct ExecutableEvent
	{
		public string custom_tag;

		public string event_name;
	}

	public SmartExpression condition = new SmartExpression();

	public SmartExpression condition_1 = new SmartExpression();

	public SmartExpression condition_2 = new SmartExpression();

	public List<SmartExpression> expressions = new List<SmartExpression>();

	public List<SmartExpression> execute_expressions = new List<SmartExpression>();

	public List<string> execute_scripts = new List<string>();

	public List<string> scripts_1 = new List<string>();

	public List<string> scripts_2 = new List<string>();

	public List<ExecutableEvent> execute_events = new List<ExecutableEvent>();

	public float start_time;

	public float period_time;
}
