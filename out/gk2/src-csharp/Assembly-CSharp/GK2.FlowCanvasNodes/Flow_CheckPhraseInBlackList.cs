using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Check Phrase In BlackList", 0)]
[Category("Game/Dialogue")]
[Color("70f1ff")]
public class Flow_CheckPhraseInBlackList : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool checkInUnlockedList = true;

	private FlowInput @in;

	private FlowOutput @true;

	private FlowOutput @false;

	private ValueInput<string> phrase;

	private ValueOutput<bool> condition;

	private bool innerCondition;

	public override string name => "<color=010101>" + (checkInUnlockedList ? "Check Phrase In UnlockedList" : "Check Phrase In BlackList") + "</color>";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Check);
		@true = AddFlowOutput("true".CapitalizeFirst());
		@false = AddFlowOutput("false".CapitalizeFirst());
		phrase = AddValueInput<string>("phrase".CapitalizeFirst());
		condition = AddValueOutput("condition".CapitalizeFirst(), () => innerCondition);
	}

	private void Check(Flow flow)
	{
		if (checkInUnlockedList)
		{
			innerCondition = MainGame.Instance.GameSave.knowledgeSystem.unlockedPhrases.Contains(phrase.value);
		}
		else
		{
			innerCondition = MainGame.Instance.GameSave.knowledgeSystem.blackListPhrases.Contains(phrase.value);
		}
		if (innerCondition)
		{
			@true.Call(flow);
		}
		else
		{
			@false.Call(flow);
		}
	}
}
