using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Wait For Flow", 0)]
[Description("This block is like DoOnce one, but waits flow as many as set")]
public class Flow_WaitForFlow : MyFlowNode
{
	[SerializeField]
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
		reset = AddFlowInput("Reset", Reset);
		waitEnded = AddFlowOutput("Wait End");
		whileWaiting = AddFlowOutput("While Waiting");
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

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		if (GUILayout.Button("Refresh"))
		{
			GatherPorts();
		}
	}
}
