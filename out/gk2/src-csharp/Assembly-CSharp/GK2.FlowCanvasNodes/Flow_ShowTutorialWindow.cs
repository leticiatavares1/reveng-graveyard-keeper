using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Tutorial Window", 0)]
[Category("Game/Quests")]
public class Flow_ShowTutorialWindow : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onClose;

	private ValueInput<string> pageId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ShowWindow);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onClose = AddFlowOutput("onClose".CapitalizeFirst());
		pageId = AddValueInput<string>("pageId".CapitalizeFirst());
	}

	private void ShowWindow(Flow flow)
	{
		UITutorialWindowData data = new UITutorialWindowData(pageId.value)
		{
			OnCompleteCallback = delegate
			{
				onClose.Call(flow);
			}
		};
		LazyUI.GetWindow<UITutorialWindow>().Open(data);
		@out.Call(flow);
	}
}
