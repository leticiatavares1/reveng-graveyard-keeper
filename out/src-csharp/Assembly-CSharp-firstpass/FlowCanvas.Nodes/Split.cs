using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Split the Flow in multiple directions. Calls all outputs in the same frame but in order")]
public class Split : FlowControlNode, IMultiPortNode
{
	[SerializeField]
	private int _portCount = 4;

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
			for (int j = 0; j < portCount; j++)
			{
				if (!base.graph.isRunning)
				{
					break;
				}
				outs[j].Call(f);
			}
		});
	}
}
