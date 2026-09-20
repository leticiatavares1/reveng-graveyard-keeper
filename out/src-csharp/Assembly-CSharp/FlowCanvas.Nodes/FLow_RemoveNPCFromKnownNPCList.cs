using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Remove NPC From Known NPC List", 0)]
public class FLow_RemoveNPCFromKnownNPCList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_wgo = AddValueInput<string>("NPC ID");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.known_npcs.RemoveNPC(in_wgo.value);
			flow_out.Call(f);
		});
	}
}
