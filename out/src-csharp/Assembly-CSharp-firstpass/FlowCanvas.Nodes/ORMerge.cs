using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Calls Out when either input is called")]
[Category("Flow Controllers/Flow Merge")]
[Name("OR", 0)]
public class ORMerge : FlowControlNode, IMultiPortNode
{
	private FlowOutput fOut;

	private int lastFrameCall;

	[SerializeField]
	private int _portCount = 2;

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
		if (Time.frameCount != lastFrameCall)
		{
			lastFrameCall = Time.frameCount;
			fOut.Call(f);
		}
	}
}
