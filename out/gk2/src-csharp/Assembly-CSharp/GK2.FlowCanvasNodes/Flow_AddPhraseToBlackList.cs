using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Add Phrase To BlackList", 0)]
[Category("Game/Dialogue")]
[Color("FFFFFF")]
public class Flow_AddPhraseToBlackList : GKCustomFlowNode
{
	public enum OperationType
	{
		Add,
		Remove
	}

	[GatherPortsCallback]
	public OperationType operationType;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> phrase;

	public override string name => string.Format("{0} Phrase {1} BlackList", operationType, (operationType == OperationType.Add) ? "to" : "from");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ProcessPhrase);
		@out = AddFlowOutput("out".CapitalizeFirst());
		phrase = AddValueInput<string>("phrase".CapitalizeFirst());
	}

	private void ProcessPhrase(Flow flow)
	{
		switch (operationType)
		{
		case OperationType.Add:
			MainGame.Instance.GameSave.knowledgeSystem.AddPhraseToBlackList(phrase.value);
			break;
		case OperationType.Remove:
			MainGame.Instance.GameSave.knowledgeSystem.RemovePhraseFromBlackList(phrase.value);
			break;
		}
		@out.Call(flow);
	}
}
