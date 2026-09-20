using System;
using System.Collections.Generic;
using System.Reflection;
using LinqTools;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Name("Implemented Action (mp)", 0)]
[Category("✫ Script Control/Multiplatform")]
[Description("Calls a function that has signature of 'public Status NAME()' or 'public Status NAME(T)'. You should return Status.Success, Failure or Running within that function.")]
public class ImplementedAction_Multiplatform : ActionTask
{
	[SerializeField]
	private SerializedMethodInfo method;

	[SerializeField]
	private List<BBObjectParameter> parameters = new List<BBObjectParameter>();

	private Status actionStatus = Status.Resting;

	private MethodInfo targetMethod
	{
		get
		{
			if (method == null)
			{
				return null;
			}
			return method.Get();
		}
	}

	public override Type agentType
	{
		get
		{
			if (!(targetMethod != null))
			{
				return typeof(Transform);
			}
			return targetMethod.RTReflectedType();
		}
	}

	protected override string info
	{
		get
		{
			if (method == null)
			{
				return "No Action Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {method.GetMethodString()} *</color>";
			}
			return string.Format("[ {0}.{1}({2}) ]", base.agentInfo, targetMethod.Name, (parameters.Count == 1) ? parameters[0].ToString() : "");
		}
	}

	public override void OnValidate(ITaskSystem ownerSystem)
	{
		if (method != null && method.HasChanged())
		{
			SetMethod(method.Get());
		}
		if (method != null && method.Get() == null)
		{
			Error($"Missing Method '{method.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (method == null)
		{
			return "No method selected";
		}
		if (targetMethod == null)
		{
			return $"Missing method '{method.GetMethodString()}'";
		}
		return null;
	}

	protected override void OnExecute()
	{
		Forward();
	}

	protected override void OnUpdate()
	{
		Forward();
	}

	private void Forward()
	{
		object[] array = parameters.Select((BBObjectParameter p) => p.value).ToArray();
		actionStatus = (Status)targetMethod.Invoke(base.agent, array);
		if (actionStatus == Status.Success)
		{
			EndAction(success: true);
		}
		else if (actionStatus == Status.Failure)
		{
			EndAction(success: false);
		}
	}

	protected override void OnStop()
	{
		actionStatus = Status.Resting;
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			this.method = new SerializedMethodInfo(method);
			parameters.Clear();
			ParameterInfo[] array = method.GetParameters();
			for (int i = 0; i < array.Length; i++)
			{
				BBObjectParameter item = new BBObjectParameter(array[i].ParameterType)
				{
					bb = base.blackboard
				};
				parameters.Add(item);
			}
		}
	}
}
