using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Flip Flop (Multi)", 0)]
[Category("Flow Controllers/Togglers")]
[Description("Each time input is signaled, the next output in order is called. After the last output, the order loops from the start.\nReset, resets the current index to zero.")]
[ContextDefinedOutputs(new Type[] { typeof(int) })]
public class Sequence : FlowControlNode, IMultiPortNode
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
			current = (int)Mathf.Repeat(current + 1, portCount);
		});
		AddFlowInput("Reset", delegate
		{
			current = 0;
		});
		AddValueOutput("Current", () => current);
	}
}
