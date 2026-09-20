using System;
using System.Collections;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Script Control/Standalone Only")]
[Description("Execute a function on a script, of up to 6 parameters and save the return if any. If function is an IEnumerator it will execute as a coroutine.")]
public class ExecuteFunction : ActionTask, ISubParametersContainer
{
	[SerializeField]
	protected ReflectedWrapper functionWrapper;

	private bool routineRunning;

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
				return "No Method Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {functionWrapper.GetMethodString()} *</color>";
			}
			BBParameter[] variables = functionWrapper.GetVariables();
			string text = "";
			string text2 = "";
			if (targetMethod.ReturnType == typeof(void))
			{
				for (int i = 0; i < variables.Length; i++)
				{
					text2 = text2 + ((i != 0) ? ", " : "") + variables[i].ToString();
				}
			}
			else
			{
				text = ((targetMethod.ReturnType == typeof(void) || targetMethod.ReturnType == typeof(IEnumerator) || variables[0].isNone) ? "" : (variables[0]?.ToString() + " = "));
				for (int j = 1; j < variables.Length; j++)
				{
					text2 = text2 + ((j != 1) ? ", " : "") + variables[j].ToString();
				}
			}
			return $"{text}{base.agentInfo}.{targetMethod.Name}({text2})";
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
			Error($"Missing Method '{functionWrapper.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (functionWrapper == null)
		{
			return "No Method selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Method '{functionWrapper.GetMethodString()}'";
		}
		try
		{
			functionWrapper.Init(base.agent);
			return null;
		}
		catch
		{
			return "ExecuteFunction Error";
		}
	}

	protected override void OnExecute()
	{
		if (targetMethod == null)
		{
			EndAction(success: false);
			return;
		}
		try
		{
			if (targetMethod.ReturnType == typeof(IEnumerator))
			{
				StartCoroutine(InternalCoroutine((IEnumerator)((ReflectedFunctionWrapper)functionWrapper).Call()));
				return;
			}
			if (targetMethod.ReturnType == typeof(void))
			{
				((ReflectedActionWrapper)functionWrapper).Call();
			}
			else
			{
				((ReflectedFunctionWrapper)functionWrapper).Call();
			}
			EndAction(success: true);
		}
		catch (Exception ex)
		{
			Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
			EndAction(success: false);
		}
	}

	protected override void OnStop()
	{
		routineRunning = false;
	}

	private IEnumerator InternalCoroutine(IEnumerator routine)
	{
		routineRunning = true;
		while (routineRunning && routine.MoveNext())
		{
			if (!routineRunning)
			{
				yield break;
			}
			yield return routine.Current;
		}
		if (routineRunning)
		{
			EndAction();
		}
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			functionWrapper = ReflectedWrapper.Create(method, base.blackboard);
		}
	}
}
