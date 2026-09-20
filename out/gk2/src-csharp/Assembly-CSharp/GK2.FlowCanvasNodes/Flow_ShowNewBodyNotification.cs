using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Show New Body Notification", 0)]
[Category("Game/Quests")]
public class Flow_ShowNewBodyNotification : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> bodyId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ShowWindow);
		@out = AddFlowOutput("out".CapitalizeFirst());
		bodyId = AddValueInput<string>("bodyId".CapitalizeFirst());
	}

	private void ShowWindow(Flow flow)
	{
		LazySingleton<UINotificator>.Instance.ShowNewBodyNotification(bodyId.value);
		@out.Call(flow);
	}
}
