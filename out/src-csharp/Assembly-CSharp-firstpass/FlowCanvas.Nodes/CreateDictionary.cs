using System;
using System.Collections.Generic;
using LinqTools;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Create a Dictionary of <string, T> objects")]
[ContextDefinedInputs(new Type[]
{
	typeof(string),
	typeof(Wild)
})]
public class CreateDictionary<T> : VariableNode, IMultiPortNode
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
		List<ValueInput<string>> keys = new List<ValueInput<string>>();
		List<ValueInput<T>> values = new List<ValueInput<T>>();
		for (int i = 0; i < portCount; i++)
		{
			keys.Add(AddValueInput<string>("Key" + i));
			values.Add(AddValueInput<T>("Value" + i));
		}
		AddValueOutput("Dictionary", (ValueHandler<IDictionary<string, T>>)delegate
		{
			List<string> j = keys.Select((ValueInput<string> x) => x.value).ToList();
			List<T> v = values.Select((ValueInput<T> x) => x.value).ToList();
			return j.ToDictionary((string x) => x, (string x) => v[j.IndexOf(x)]);
		}, "");
	}
}
