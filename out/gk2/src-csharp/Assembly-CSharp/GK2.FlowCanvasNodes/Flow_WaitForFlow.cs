using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Wait For Flow", 0)]
[Description("This block is like DoOnce one, but waits flow as many as set")]
public class Flow_WaitForFlow : GKCustomFlowNode
{
	[SerializeField]
	[ExposeField]
	[MinValue(2)]
	[DelayedField]
	[GatherPortsCallback]
	public int inputFlowsWaitCount = 2;

	private FlowInput reset;

	private FlowOutput waitEnded;

	private FlowOutput whileWaiting;

	private bool[] activated;

	public override string name => $"Wait For {inputFlowsWaitCount} Flow";

	protected override void RegisterPorts()
	{
		activated = new bool[inputFlowsWaitCount];
		for (int j = 0; j < inputFlowsWaitCount; j++)
		{
			int i = j;
			AddFlowInput(i.ToString(), delegate(Flow flow)
			{
				Check(i, flow);
			});
		}
		reset = AddFlowInput("reset".CapitalizeFirst(), Reset);
		waitEnded = AddFlowOutput("waitEnded".CapitalizeFirst());
		whileWaiting = AddFlowOutput("whileWaiting".CapitalizeFirst());
	}

	private void Check(int index, Flow flow)
	{
		activated[index] = true;
		if (CheckFlowsActivated())
		{
			for (int i = 0; i < inputFlowsWaitCount; i++)
			{
				activated[i] = false;
			}
			waitEnded.Call(flow);
		}
		else
		{
			whileWaiting.Call(flow);
		}
	}

	private bool CheckFlowsActivated()
	{
		for (int i = 0; i < inputFlowsWaitCount; i++)
		{
			if (!activated[i])
			{
				return false;
			}
		}
		return true;
	}

	private void Reset(Flow flow)
	{
		for (int i = 0; i < inputFlowsWaitCount; i++)
		{
			activated[i] = false;
		}
		waitEnded.Call(flow);
	}
}
