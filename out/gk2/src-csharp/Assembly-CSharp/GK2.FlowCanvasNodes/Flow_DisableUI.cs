using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Disable UI (HUD)", 0)]
[Category("Game/UI")]
public class Flow_DisableUI : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool toggleInfoWidget;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<bool> enable;

	public override string name
	{
		get
		{
			if (enable.value)
			{
				return "Enable UI (HUD)";
			}
			return "Disable UI (HUD)";
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoFade);
		@out = AddFlowOutput("out".CapitalizeFirst());
		enable = AddValueInput<bool>("enable?");
	}

	private void DoFade(Flow flow)
	{
		GUIElements.Instance.SetVisibilityState(enable.value);
		if (toggleInfoWidget)
		{
			UIInfoWidget.IsDisabled = !enable.value;
			if (UIInfoWidget.IsDisabled)
			{
				UIInfoWidget[] array = Object.FindObjectsByType<UIInfoWidget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Hide();
				}
			}
		}
		@out.Call(flow);
	}
}
