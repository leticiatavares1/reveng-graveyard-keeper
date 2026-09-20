using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Quest Status Equals", 0)]
[Category("Game/Quest")]
[Color("FFBE3B")]
public class Flow_QuestStatusEquals : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<string> questId;

	private ValueInput<QuestStatus> questStatus;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			if (MainGame.Instance.GameSave.questSystemData.IsQuestInStatus(questId.value, questStatus.value))
			{
				yes.Call(flow);
			}
			else
			{
				no.Call(flow);
			}
		});
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
		questId = AddValueInput<string>("questId".CapitalizeFirst());
		questStatus = AddValueInput<QuestStatus>("questStatus".CapitalizeFirst());
	}
}
