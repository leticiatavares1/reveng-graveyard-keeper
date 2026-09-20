using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Obsolete]
[Category("Flow Controllers/Flow Merge")]
[Name("XOR", 0)]
[Description("Calls Out when either single Input is called, but no other is in the same frame.")]
public class XORMerge : FlowControlNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 2;

	private FlowOutput fOut;

	private int[] calls;

	private int lastFrameCall;

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
		calls = new int[portCount];
		fOut = AddFlowOutput("Out");
		for (int j = 0; j < portCount; j++)
		{
			int i = j;
			AddFlowInput(i.ToString(), delegate(Flow f)
			{
				Check(i, f);
			});
		}
	}

	private void Check(int index, Flow f)
	{
		calls[index] = Time.frameCount;
		for (int i = 0; i < calls.Length; i++)
		{
			if (i != index && calls[i] == lastFrameCall)
			{
				return;
			}
		}
		fOut.Call(f);
		lastFrameCall = Time.frameCount;
	}
}
