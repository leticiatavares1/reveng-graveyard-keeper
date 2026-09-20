using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("ffd200")]
[Icon("Task", false, "")]
[Category("Game Actions")]
[Name("Set Task", 0)]
public class Flow_SetTaskState : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!string.IsNullOrEmpty(GetInputValuePort<string>("NPC id").value))
			{
				return "Set [npc] Task";
			}
			return "Set [self] Task";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> par_npc = AddValueInput<string>("NPC id (\"\" if self)", "NPC id");
		ValueInput<string> par_task = AddValueInput<string>("Task");
		ValueInput<KnownNPC.TaskState.State> par_state = AddValueInput<KnownNPC.TaskState.State>("State");
		ValueInput<float> par_story_br = AddValueInput<float>("Story brnz");
		ValueInput<float> par_story_silv = AddValueInput<float>("Story silv");
		ValueInput<float> par_story_gold = AddValueInput<float>("Story gold");
		FlowOutput flow_out = AddFlowOutput("Immediate", "Out");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			if (base.wgo != null && MainGame.me.player.GetParam("p_journalist") > 0f)
			{
				base.wgo.DropStory(par_story_br.value, par_story_silv.value, par_story_gold.value);
			}
			string text = par_npc.value;
			if (string.IsNullOrEmpty(text))
			{
				text = base.wgo.obj_id;
			}
			MainGame.me.save.SetTaskState(text, par_task.value, par_state.value, delegate
			{
				flow_finished.Call(f);
			});
			flow_out.Call(f);
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("NPC id");
	}
}
