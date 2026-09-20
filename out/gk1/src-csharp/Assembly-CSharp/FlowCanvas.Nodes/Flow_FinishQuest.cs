using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Finish Quest", 0)]
public class Flow_FinishQuest : MyFlowNode
{
	public override string name
	{
		get
		{
			return base.name + "\n" + GetInputValuePort<string>("Quest ID").value;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> in_quest_id = AddValueInput<string>("Quest ID");
		ValueInput<bool> in_success = AddValueInput<bool>("T-Success\nF-Fail");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.quests.ForceQuestEnd(in_quest_id.value, in_success.value);
			flow_out.Call(f);
		});
	}
}
