using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Disable GDPoint", 0)]
[Category("Game Actions")]
[Icon("Cube", false, "")]
[Description("If Character is null, then Player")]
public class Flow_DisableGDPoint : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("enable").value)
			{
				return "<color=#FFFF50>Enable GDPoint</color>";
			}
			return "<color=#30FF30>Disable GDPoint</color>";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<GDPoint> in_gd_point = AddValueInput<GDPoint>("GDPoint");
		ValueInput<bool> in_enable = AddValueInput<bool>("enable");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_gd_point.value != null)
			{
				in_gd_point.value.gameObject.SetActive(in_enable.value);
				if (in_enable.value)
				{
					RoundAndSortComponent[] componentsInChildren = in_gd_point.value.GetComponentsInChildren<RoundAndSortComponent>(includeInactive: true);
					if (componentsInChildren != null && componentsInChildren.Length != 0)
					{
						RoundAndSortComponent[] array = componentsInChildren;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].DoUpdateStuff(force: true);
						}
					}
				}
			}
			else
			{
				Debug.LogError("GDPoint is null!");
			}
			flow_out.Call(f);
		});
	}
}
