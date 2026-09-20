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

[Description("Execute a static function and optionaly save the return value")]
[Category("✫ Script Control/Multiplatform")]
[Name("Execute Static Function (mp)", 0)]
public class ExecuteStaticFunction_Multiplatform : ActionTask
{
	[SerializeField]
	protected SerializedMethodInfo method;

	[SerializeField]
	protected List<BBObjectParameter> parameters = new List<BBObjectParameter>();

	[SerializeField]
	[BlackboardOnly]
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

	protected override string info
	{
		get
		{
			if (method == null)
			{
				return "No Method Selected";
			}
			if (targetMethod == null)
			{
				return $"<color=#ff6457>* {method.GetMethodString()} *</color>";
			}
			string text = ((targetMethod.ReturnType == typeof(void)) ? "" : (returnValue.ToString() + " = "));
			string text2 = "";
			for (int i = 0; i < parameters.Count; i++)
			{
				text2 = text2 + ((i != 0) ? ", " : "") + parameters[i].ToString();
			}
			return $"{text}{targetMethod.DeclaringType.FriendlyName()}.{targetMethod.Name} ({text2})";
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
			return "No methMethodd selected";
		}
		if (targetMethod == null)
		{
			return $"Missing Method '{method.GetMethodString()}'";
		}
		return null;
	}

	protected override void OnExecute()
	{
		object[] array = parameters.Select((BBObjectParameter p) => p.value).ToArray();
		returnValue.value = targetMethod.Invoke(base.agent, array);
		EndAction();
	}

	private void SetMethod(MethodInfo method)
	{
		if (method == null)
		{
			return;
		}
		this.method = new SerializedMethodInfo(method);
		parameters.Clear();
		ParameterInfo[] array = method.GetParameters();
		foreach (ParameterInfo parameterInfo in array)
		{
			BBObjectParameter bBObjectParameter = new BBObjectParameter(parameterInfo.ParameterType)
			{
				bb = base.blackboard
			};
			if (parameterInfo.IsOptional)
			{
				bBObjectParameter.value = parameterInfo.DefaultValue;
			}
			parameters.Add(bBObjectParameter);
		}
		if (method.ReturnType != typeof(void))
		{
			returnValue = new BBObjectParameter(method.ReturnType)
			{
				bb = base.blackboard
			};
		}
		else
		{
			returnValue = null;
		}
	}
}
