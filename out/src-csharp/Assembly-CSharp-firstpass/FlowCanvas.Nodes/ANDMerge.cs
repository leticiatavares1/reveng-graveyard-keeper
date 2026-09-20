using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Flow Controllers/Flow Merge")]
[Description("Calls Out when all inputs are called together in the same frame")]
[Name("AND", 0)]
public class ANDMerge : FlowControlNode, IMultiPortNode
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
			if (calls[i] != calls[index])
			{
				return;
			}
		}
		if (Time.frameCount != lastFrameCall)
		{
			lastFrameCall = Time.frameCount;
			fOut.Call(f);
		}
	}
}
