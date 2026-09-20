using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[]
{
	typeof(Flow),
	typeof(int)
})]
[Description("Calls one random output each time In is called")]
public class Random : FlowControlNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 4;

	private int current;

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
		List<FlowOutput> outs = new List<FlowOutput>();
		for (int i = 0; i < portCount; i++)
		{
			outs.Add(AddFlowOutput(i.ToString()));
		}
		AddFlowInput("In", delegate(Flow f)
		{
			current = UnityEngine.Random.Range(0, portCount);
			outs[current].Call(f);
		});
		AddValueOutput("Current", () => current);
	}
}
