using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Black List Phrase", 0)]
public class Flow_BlackListPhrase : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> phrase = AddValueInput<string>("Phrase");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.AddPhraseToBlackList(phrase.value);
			flow_out.Call(f);
		});
	}
}
