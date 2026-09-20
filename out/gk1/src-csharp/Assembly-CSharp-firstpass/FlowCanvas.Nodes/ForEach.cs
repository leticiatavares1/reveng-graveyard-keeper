using System;
using System.Collections;
using System.Collections.Generic;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Flow Controllers/Iterators")]
[ContextDefinedInputs(new Type[] { typeof(IEnumerable) })]
[Description("Enumerate a value (usualy a list or array) for each of it's elements")]
[ContextDefinedOutputs(new Type[] { typeof(object) })]
public class ForEach : FlowControlNode
{
	private object current;

	private bool broken;

	private ValueInput<IEnumerable> enumerableInput;

	protected override void RegisterPorts()
	{
		enumerableInput = AddValueInput<IEnumerable>("Value");
		AddValueOutput("Current", () => current);
		FlowOutput fCurrent = AddFlowOutput("Do");
		FlowOutput fFinish = AddFlowOutput("Done");
		AddFlowInput("In", delegate(Flow f)
		{
			IEnumerable value = enumerableInput.value;
			if (value == null)
			{
				fFinish.Call(f);
			}
			else
			{
				broken = false;
				f.Break = delegate
				{
					broken = true;
				};
				foreach (object item in value)
				{
					if (broken)
					{
						break;
					}
					current = item;
					fCurrent.Call(f);
				}
				f.Break = null;
				fFinish.Call(f);
			}
		});
		AddFlowInput("Break", delegate
		{
			broken = true;
		});
	}

	public override Type GetNodeWildDefinitionType()
	{
		return typeof(IEnumerable);
	}

	public override void OnPortConnected(Port port, Port otherPort)
	{
		if (port == enumerableInput)
		{
			Type enumerableElementType = otherPort.type.GetEnumerableElementType();
			if (enumerableElementType != null)
			{
				ReplaceWith(typeof(ForEach<>).RTMakeGenericType(enumerableElementType));
			}
		}
	}
}
[ExposeAsDefinition]
[ContextDefinedOutputs(new Type[] { typeof(Wild) })]
[Description("Enumerate a value (usualy a list or array) for each of it's elements")]
[Category("Flow Controllers/Iterators")]
[ContextDefinedInputs(new Type[] { typeof(IEnumerable<>) })]
public class ForEach<T> : FlowControlNode
{
	private T current;

	private bool broken;

	protected override void RegisterPorts()
	{
		ValueInput<IEnumerable<T>> list = AddValueInput<IEnumerable<T>>("Value");
		AddValueOutput("Current", () => current);
		FlowOutput fCurrent = AddFlowOutput("Do");
		FlowOutput fFinish = AddFlowOutput("Done");
		AddFlowInput("In", delegate(Flow f)
		{
			IEnumerable<T> value = list.value;
			if (value == null)
			{
				fFinish.Call(f);
			}
			else
			{
				broken = false;
				f.Break = delegate
				{
					broken = true;
				};
				foreach (T item in value)
				{
					if (broken)
					{
						break;
					}
					current = item;
					fCurrent.Call(f);
				}
				f.Break = null;
				fFinish.Call(f);
			}
		});
		AddFlowInput("Break", delegate
		{
			broken = true;
		});
	}
}
