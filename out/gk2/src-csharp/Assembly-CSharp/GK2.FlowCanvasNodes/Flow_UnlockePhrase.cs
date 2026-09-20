using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Unlock Phrase", 0)]
[Category("Game/Dialogue")]
[Color("FFFFFF")]
public class Flow_UnlockePhrase : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> phrase;

	public override string name => "<color=#010101>Unlock Phrase</color>";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ProcessPhrase);
		@out = AddFlowOutput("out".CapitalizeFirst());
		phrase = AddValueInput<string>("phrase".CapitalizeFirst());
	}

	private void ProcessPhrase(Flow flow)
	{
		MainGame.Instance.GameSave.knowledgeSystem.UnlockPhrase(phrase.value);
		@out.Call(flow);
	}
}
