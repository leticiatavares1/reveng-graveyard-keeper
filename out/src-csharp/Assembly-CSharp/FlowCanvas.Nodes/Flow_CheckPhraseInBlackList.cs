using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Check Phrase In BlackList", 0)]
[Category("Game Actions")]
public class Flow_CheckPhraseInBlackList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		bool phrase_contains = false;
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> phrase = AddValueInput<string>("Phrase");
		AddValueOutput("Contains", () => phrase_contains);
		AddFlowInput("In", delegate(Flow f)
		{
			string value = phrase.value;
			phrase_contains = MainGame.me.save.black_list_of_phrases.Contains(value);
			flow_out.Call(f);
		});
	}
}
