using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Show Simple Text With Icon Notification", 0)]
[Category("Game/Quests")]
public class Flow_ShowSimpleTextWithIconNotification : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> locale;

	private ValueInput<string> iconId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ShowWindow);
		@out = AddFlowOutput("out".CapitalizeFirst());
		locale = AddValueInput<string>("locale".CapitalizeFirst());
		iconId = AddValueInput<string>("iconId".CapitalizeFirst());
	}

	private void ShowWindow(Flow flow)
	{
		LazySingleton<UINotificator>.Instance.ShowSimpleTextWithIconNotification(locale.value, iconId.value);
		@out.Call(flow);
	}
}
