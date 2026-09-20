using System;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Variables/Blackboard")]
[Description("Set a Blackboard variable value")]
[Name("Set Of Type", 10)]
[ContextDefinedInputs(new Type[] { typeof(Wild) })]
public class SetVariable<T> : FlowNode
{
	[BlackboardOnly]
	public BBParameter<T> targetVariable;

	[HideInInspector]
	public OperationMethod operation;

	[HideInInspector]
	public bool perSecond;

	public override string name => string.Format("{0}{1}{2}", targetVariable.ToString(), OperationTools.GetOperationString(operation), "Value");

	protected override void RegisterPorts()
	{
		FlowOutput o = AddFlowOutput("Out");
		ValueInput<T> v = AddValueInput<T>("Value");
		AddValueOutput("Value", () => targetVariable.value);
		AddFlowInput("In", delegate(Flow f)
		{
			DoSet(v.value);
			o.Call(f);
		});
	}

	private void DoSet(T value)
	{
		if (operation != 0)
		{
			if (typeof(T) == typeof(float))
			{
				targetVariable.value = (T)(object)OperationTools.Operate((float)(object)targetVariable.value, (float)(object)value, operation);
			}
			else if (typeof(T) == typeof(int))
			{
				targetVariable.value = (T)(object)OperationTools.Operate((int)(object)targetVariable.value, (int)(object)value, operation);
			}
			else if (typeof(T) == typeof(Vector3))
			{
				targetVariable.value = (T)(object)OperationTools.Operate((Vector3)(object)targetVariable.value, (Vector3)(object)value, operation);
			}
			else
			{
				targetVariable.value = value;
			}
		}
		else
		{
			targetVariable.value = value;
		}
	}

	public void SetTargetVariableName(string name)
	{
		targetVariable.name = name;
	}
}
