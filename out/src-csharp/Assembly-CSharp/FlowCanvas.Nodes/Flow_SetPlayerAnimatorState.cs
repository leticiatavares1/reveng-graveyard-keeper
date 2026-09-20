using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Be careful and return state to idle/walk")]
[Name("Set Player Animator State", 0)]
[Category("Game Actions")]
public class Flow_SetPlayerAnimatorState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<int> state = AddValueInput<int>("State");
		FlowOutput flow_out = AddFlowOutput("Out", "Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player_char.TryDropOverheadItem();
			MainGame.me.player.components.character.SetGlobalState(state.value);
			flow_out.Call(f);
		});
	}
}
