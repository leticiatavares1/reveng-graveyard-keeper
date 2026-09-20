using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Unlock Phrase", 0)]
public class Flow_UnlockPhrase : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> phrase = AddValueInput<string>("Phrase");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.UnlockPhrase(phrase.value);
			flow_out.Call(f);
		});
	}
}
