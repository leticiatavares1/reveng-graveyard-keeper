using System;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Script Control/Multiplatform")]
[Description("Get a property of a script and save it to the blackboard")]
[Name("Get Property (mp)", 0)]
public class GetProperty_Multiplatform : ActionTask
{
	[SerializeField]
	protected SerializedMethodInfo method;

	[BlackboardOnly]
	[SerializeField]
	protected BBObjectParameter returnValue;

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
				return "No Property Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {method.GetMethodString()} *</color>";
			}
			return $"{returnValue.ToString()} = {base.agentInfo}.{targetMethod.Name}";
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
			Error($"Missing Property '{method.GetMethodString()}'");
		}
	}

	protected override string OnInit()
	{
		if (method == null)
		{
			return "No Property selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Property '{method.GetMethodString()}'";
		}
		return null;
	}

	protected override void OnExecute()
	{
		returnValue.value = targetMethod.Invoke(base.agent, null);
		EndAction();
	}

	private void SetMethod(MethodInfo method)
	{
		if (method != null)
		{
			this.method = new SerializedMethodInfo(method);
			returnValue.SetType(method.ReturnType);
		}
	}
}
