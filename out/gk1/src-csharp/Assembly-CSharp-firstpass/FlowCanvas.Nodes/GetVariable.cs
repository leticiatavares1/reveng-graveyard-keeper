using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Variables")]
[Description("Returns a constant or linked variable value.\nYou can alter between constant or linked at any time using the radio button.")]
[ContextDefinedOutputs(new Type[] { typeof(Wild) })]
[Name("Graph Variable", 99)]
public class GetVariable<T> : VariableNode
{
	public BBParameter<T> value;

	protected override void RegisterPorts()
	{
		AddValueOutput("Value", () => value.value);
	}

	public void SetTargetVariableName(string name)
	{
		value.name = name;
	}

	public override void SetVariable(object o)
	{
		if (o is Variable<T>)
		{
			value.name = (o as Variable<T>).name;
		}
		else if (o is T)
		{
			value.value = (T)o;
		}
		else
		{
			Debug.LogError("Set Variable Error");
		}
	}
}
