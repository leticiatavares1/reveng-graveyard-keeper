using System;
using System.Collections.Generic;
using System.Reflection;
using ParadoxNotion;

namespace FlowCanvas.Nodes.Legacy;

public sealed class PureReflectedMethodNode : ReflectedMethodNode
{
	private MethodInfo method;

	private ValueInput instanceInput;

	private List<ValueInput> inputs;

	private List<ValueInput> paramsInputs;

	private Type paramsArrayType;

	private object[] args;

	private object instance;

	private object returnValue;

	public override void RegisterPorts(FlowNode node, MethodInfo method, ReflectedMethodRegistrationOptions options)
	{
		this.method = method;
		ParameterInfo[] parameters = method.GetParameters();
		if (options.callable)
		{
			FlowOutput o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", delegate(Flow f)
			{
				CallMethod();
				o.Call(f);
			});
		}
		if (!method.IsStatic)
		{
			instanceInput = node.AddValueInput(method.DeclaringType.FriendlyName(), method.DeclaringType);
			if (options.callable)
			{
				node.AddValueOutput(method.DeclaringType.FriendlyName(), method.DeclaringType, () => instance);
			}
		}
		if (method.ReturnType != typeof(void))
		{
			node.AddValueOutput("Value", method.ReturnType, () => (!options.callable) ? CallMethod() : returnValue);
		}
		inputs = new List<ValueInput>();
		for (int j = 0; j < parameters.Length; j++)
		{
			int i = j;
			ParameterInfo parameterInfo = parameters[i];
			string text = parameterInfo.Name;
			if (instanceInput != null && text == instanceInput.name)
			{
				text += " ";
			}
			if (parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef)
			{
				node.AddValueOutput(text, parameterInfo.ParameterType.GetElementType(), delegate
				{
					if (options.callable)
					{
						return args[i];
					}
					CallMethod();
					return args[i];
				});
				inputs.Add(new ValueInput<object>(null, null, null));
			}
			else if (options.exposeParams && parameterInfo.IsParams(parameters))
			{
				paramsInputs = new List<ValueInput>();
				paramsArrayType = parameterInfo.ParameterType;
				for (int k = 0; k < options.exposedParamsCount; k++)
				{
					ValueInput item = node.AddValueInput(text + " #" + k, parameterInfo.ParameterType.GetEnumerableElementType(), text + k);
					paramsInputs.Add(item);
				}
			}
			else
			{
				ValueInput valueInput = node.AddValueInput(text, parameterInfo.ParameterType);
				if (parameterInfo.IsOptional && valueInput != null)
				{
					valueInput.serializedValue = parameterInfo.DefaultValue;
				}
				inputs.Add(valueInput);
			}
		}
	}

	private object CallMethod()
	{
		if (args == null)
		{
			args = new object[inputs.Count + ((paramsInputs != null) ? 1 : 0)];
		}
		for (int i = 0; i < inputs.Count; i++)
		{
			args[i] = inputs[i].value;
		}
		if (paramsInputs != null)
		{
			Array array = Array.CreateInstance(paramsArrayType.GetElementType(), paramsInputs.Count);
			for (int j = 0; j < paramsInputs.Count; j++)
			{
				array.SetValue(paramsInputs[j].value, j);
			}
			args[args.Length - 1] = array;
		}
		if (method.IsStatic)
		{
			return returnValue = method.Invoke(null, args);
		}
		instance = instanceInput.value;
		if (instance == null || instance.Equals(null))
		{
			return returnValue = null;
		}
		return returnValue = method.Invoke(instance, args);
	}
}
