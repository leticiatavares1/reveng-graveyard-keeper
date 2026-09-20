using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Branch the Flow based on an integer value. The Default output is called when the Index value is out of range.")]
[ContextDefinedInputs(new Type[] { typeof(int) })]
[Category("Flow Controllers/Switchers")]
[Name("Switch Integer", 0)]
public class SwitchInt : FlowControlNode, IMultiPortNode
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

	protected override void RegisterPorts()
	{
		ValueInput<int> index = AddValueInput<int>("Index");
		List<FlowOutput> outs = new List<FlowOutput>();
		for (int i = 0; i < portCount; i++)
		{
			outs.Add(AddFlowOutput(i.ToString()));
		}
		FlowOutput def = AddFlowOutput("Default");
		AddFlowInput("In", delegate(Flow f)
		{
			int value = index.value;
			if (value >= 0 && value < outs.Count)
			{
				outs[value].Call(f);
			}
			else
			{
				def.Call(f);
			}
		});
	}
}
