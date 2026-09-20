using System;
using System.Collections.Generic;
using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Create a collection of <T> objects")]
[ContextDefinedInputs(new Type[] { typeof(Wild) })]
[ContextDefinedOutputs(new Type[] { typeof(List<>) })]
public class CreateCollection<T> : VariableNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 4;

	public int portCount
	{
		get
		{
			return _portCount;
		}
		set
		{
			_portCount = value;
		}
	}

	public override void SetVariable(object o)
	{
	}

	protected override void RegisterPorts()
	{
		List<ValueInput<T>> ins = new List<ValueInput<T>>();
		for (int i = 0; i < portCount; i++)
		{
			ins.Add(AddValueInput<T>("Element" + i));
		}
		AddValueOutput("Collection", () => ins.Select((ValueInput<T> p) => p.value).ToArray());
	}
}
