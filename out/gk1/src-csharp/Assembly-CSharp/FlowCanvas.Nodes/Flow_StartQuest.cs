using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Start Quest", 0)]
[Category("Game Actions")]
public class Flow_StartQuest : MyFlowNode
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
		ValueInput<string> par_quest_id = AddValueInput<string>("Quest ID");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			QuestDefinition data = GameBalance.me.GetData<QuestDefinition>(par_quest_id.value);
			if (data == null)
			{
				Debug.LogError("Couldn't add quest id = " + par_quest_id.value);
			}
			else
			{
				MainGame.me.save.quests.StartQuest(data);
			}
			flow_out.Call(f);
		});
	}
}
