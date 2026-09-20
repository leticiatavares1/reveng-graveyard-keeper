using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Set global_state = 999")]
[Category("Game Actions")]
[Name("Disable Player Animation", 0)]
public class Flow_DisablePlayerAnimation : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("disable").value)
			{
				return "Disable Player animations";
			}
			return "Enable Player animations";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> in_disable = AddValueInput<bool>("disable");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_disable.value)
			{
				MainGame.me.player_char.TryDropOverheadItem();
				MainGame.me.player_char.SetAnimationState(CharAnimState.Disabled);
				Debug.Log("Player anim state: Disabled");
			}
			else
			{
				MainGame.me.player_char.SetAnimationState(CharAnimState.Idle);
				Debug.Log("Player anim state: Idle");
			}
			flow_out.Call(f);
		});
	}
}
