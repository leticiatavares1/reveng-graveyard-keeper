using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("GDPoint tag starts with", 0)]
[Icon("Cube", false, "")]
[Description("If WGO is null, then self")]
public class Flow_GDPointTagStartsWith : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<GDPoint> in_gd_point = AddValueInput<GDPoint>("GDPoint");
		ValueInput<string> in_custom_tag = AddValueInput<string>("custom_tag");
		FlowOutput flow_no = AddFlowOutput("No");
		FlowOutput flow_yes = AddFlowOutput("Yes");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_gd_point.value == null)
			{
				Debug.LogError("Flow_GDPointTagStartsWith error: WGO is null");
				flow_no.Call(f);
			}
			else if (in_gd_point.value.gd_tag.StartsWith(in_custom_tag.value))
			{
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
		});
	}
}
