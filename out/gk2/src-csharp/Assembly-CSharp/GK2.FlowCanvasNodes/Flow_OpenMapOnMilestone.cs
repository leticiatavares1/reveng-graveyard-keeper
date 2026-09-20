using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Open Map On Milestone", 0)]
[Category("Game/UI")]
public class Flow_OpenMapOnMilestone : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Show);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void Show(Flow flow)
	{
		LazyUI.GetWindow<UIMapWindow>().Open(new MapPageWidgetData(MainGame.Instance.GameSave, milestonesInteractable: true, base.SelfWgoData.id));
		@out.Call(flow);
	}
}
