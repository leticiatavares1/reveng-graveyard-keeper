using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Whenever In is called the 'current' output is called as well. Calling '+' or '-' changes the current output respectively up or down.")]
[Category("Flow Controllers/Togglers")]
[ContextDefinedOutputs(new Type[] { typeof(int) })]
[Name("Toggle (Multi)", 0)]
public class MultiToggle : FlowControlNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 4;

	public int current;

	private int original;

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

	public override string name => base.name + " " + $"[{current.ToString()}]";

	public override void OnGraphStarted()
	{
		current = Mathf.Clamp(current, 0, portCount - 1);
		original = current;
	}

	public override void OnGraphStoped()
	{
		current = original;
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
			outs[current].Call(f);
		});
		AddFlowInput("-", delegate
		{
			current = (int)Mathf.Repeat(current - 1, portCount);
		});
		AddFlowInput("+", delegate
		{
			current = (int)Mathf.Repeat(current + 1, portCount);
		});
		AddValueOutput("Current", () => current);
	}
}
