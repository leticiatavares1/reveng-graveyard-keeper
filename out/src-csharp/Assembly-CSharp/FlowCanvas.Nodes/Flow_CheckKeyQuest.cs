using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Check Key Quest", 0)]
[Category("Game Actions")]
public class Flow_CheckKeyQuest : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_quest_key = AddValueInput<string>("Quest Key");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.quests.CheckKeyQuests(in_quest_key.value);
			flow_out.Call(f);
		});
	}
}
