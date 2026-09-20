using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Script Control/Common")]
[Description("SendMessage to the agent, optionaly with an argument")]
public class SendMessage : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<string> methodName;

	protected override string info => $"Message {methodName}()";

	protected override void OnExecute()
	{
		base.agent.SendMessage(methodName.value);
		EndAction();
	}
}
[Description("SendMessage to the agent, optionaly with an argument")]
[Category("✫ Script Control/Common")]
public class SendMessage<T> : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<string> methodName;

	public BBParameter<T> argument;

	protected override string info => $"Message {methodName}({argument.ToString()})";

	protected override void OnExecute()
	{
		if (argument.isNull)
		{
			base.agent.SendMessage(methodName.value);
		}
		else
		{
			base.agent.SendMessage(methodName.value, argument.value);
		}
		EndAction();
	}
}
