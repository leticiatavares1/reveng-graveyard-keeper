using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Quest Custom Trigger", 0)]
[Category("Game/Quest")]
[Color("FFBE3B")]
public class Flow_QuestCustomTrigger : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinish;

	private ValueInput<string> triggerId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Process);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinish = AddFlowOutput("onFinish".CapitalizeFirst());
		triggerId = AddValueInput<string>("triggerId");
	}

	private void Process(Flow flow)
	{
		if (!MainGame.Instance.GameSave.questSystemData.RaiseCustomQuestTrigger(triggerId.value, delegate
		{
			onFinish.Call(flow);
		}) && onFinish.isConnected)
		{
			Error("No callback for quest trigger " + triggerId.value + ".");
			onFinish.Call(flow);
		}
		@out.Call(flow);
	}
}
