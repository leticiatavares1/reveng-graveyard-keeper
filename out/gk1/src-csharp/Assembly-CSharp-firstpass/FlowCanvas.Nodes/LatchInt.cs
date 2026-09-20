using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Convert a Flow signal to an integer value")]
[ContextDefinedOutputs(new Type[] { typeof(int) })]
[Category("Flow Controllers/Flow Convert")]
[Name("Latch Integer", 0)]
public class LatchInt : FlowControlNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 4;

	private int latched;

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
		FlowOutput o = AddFlowOutput("Out");
		for (int j = 0; j < portCount; j++)
		{
			int i = j;
			AddFlowInput(i.ToString(), delegate(Flow f)
			{
				latched = i;
				o.Call(f);
			});
		}
		AddValueOutput("Value", () => latched);
	}
}
