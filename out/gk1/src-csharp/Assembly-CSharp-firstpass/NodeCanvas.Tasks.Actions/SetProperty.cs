using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Script Control/Standalone Only")]
[Description("Set a property on a script")]
public class SetProperty : ActionTask, ISubParametersContainer
{
	[SerializeField]
	protected ReflectedActionWrapper functionWrapper;

	private MethodInfo targetMethod
	{
		get
		{
			if (functionWrapper == null)
			{
				return null;
			}
			return functionWrapper.GetMethod();
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
			if (functionWrapper == null)
			{
				return "No Property Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {functionWrapper.GetMethodString()} *</color>";
			}
			return $"{base.agentInfo}.{targetMethod.Name} = {functionWrapper.GetVariables()[0]}";
		}
	}

	BBParameter[] ISubParametersContainer.GetSubParameters()
	{
		if (functionWrapper == null)
		{
			return null;
		}
		return functionWrapper.GetVariables();
	}

	public override void OnValidate(ITaskSystem ownerSystem)
	{
		if (functionWrapper != null && functionWrapper.HasChanged())
		{
			SetMethod(functionWrapper.GetMethod());
		}
		if (functionWrapper != null && targetMethod == null)
		{
			Error($"Missing Property '{functionWrapper.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (functionWrapper == null)
		{
			return "No Property selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Property '{functionWrapper.GetMethodString()}'";
		}
		try
		{
			functionWrapper.Init(base.agent);
			return null;
		}
		catch
		{
			return "SetProperty Error";
		}
	}

	protected override void OnExecute()
	{
		if (functionWrapper == null)
		{
			EndAction(success: false);
			return;
		}
		functionWrapper.Call();
		EndAction();
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			functionWrapper = ReflectedActionWrapper.Create(method, base.blackboard);
		}
	}
}
