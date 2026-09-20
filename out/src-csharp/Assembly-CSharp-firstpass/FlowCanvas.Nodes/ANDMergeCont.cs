using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("AND Cont", 0)]
[Description("Calls Out when all inputs are called (no matter at what frame)")]
[Category("Flow Controllers/Flow Merge")]
public class ANDMergeCont : FlowControlNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 2;

	private FlowOutput fOut;

	private bool[] calls;

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
		calls = new bool[portCount];
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
		calls[index] = true;
		bool flag = true;
		for (int i = 0; i < calls.Length; i++)
		{
			flag &= calls[i];
		}
		if (flag && Time.frameCount != lastFrameCall)
		{
			lastFrameCall = Time.frameCount;
			fOut.Call(f);
		}
	}
}
