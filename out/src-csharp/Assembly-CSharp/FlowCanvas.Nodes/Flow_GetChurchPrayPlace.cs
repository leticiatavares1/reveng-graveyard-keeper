using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Church Pray Place", 0)]
[Category("Game Actions")]
public class Flow_GetChurchPrayPlace : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		GameObject _go = null;
		AddValueOutput("GO", () => _go);
		AddFlowInput("In", delegate(Flow f)
		{
			_go = PrayLogics.GetPrayPlace();
			flow_out.Call(f);
		});
	}
}
