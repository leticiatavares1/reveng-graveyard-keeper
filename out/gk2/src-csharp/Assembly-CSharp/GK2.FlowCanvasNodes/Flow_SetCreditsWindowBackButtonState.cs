using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Credits Window Back Button State", 0)]
[Category("Game/UI")]
public class Flow_SetCreditsWindowBackButtonState : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool hide;

	private FlowInput @in;

	private FlowOutput @out;

	public override string name => (hide ? "Hide" : "Show") + " Credits Window Back Button";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetHiddenState);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void SetHiddenState(Flow flow)
	{
		UICreditsWindow window = LazyUI.GetWindow<UICreditsWindow>();
		if (hide)
		{
			window.HideBackButton();
		}
		else
		{
			window.ShowBackButton();
		}
		@out.Call(flow);
	}
}
