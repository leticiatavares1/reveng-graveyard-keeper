using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Is GDPoint Enable", 0)]
[Category("Game Actions")]
[Icon("Cube", false, "")]
public class Flow_IsGDPointEnable : MyFlowNode
{
	private bool _enable_flag;

	protected override void RegisterPorts()
	{
		ValueInput<GDPoint> in_gd_point = AddValueInput<GDPoint>("GDPoint");
		AddValueOutput("is_enable", () => _enable_flag);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_gd_point.value != null)
			{
				_enable_flag = in_gd_point.value.gameObject.activeSelf;
			}
			else
			{
				Debug.LogError("GDPoint is null!");
			}
			flow_out.Call(f);
		});
	}
}
