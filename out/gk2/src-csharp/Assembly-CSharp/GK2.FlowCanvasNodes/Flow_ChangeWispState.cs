using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Change Wisp State", 0)]
[Category("Game/Cutscenes")]
[Color("8a8a8a")]
public class Flow_ChangeWispState : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<bool> isEnable;

	public override string name => (isEnable.value ? "Enable Wisp" : "Disable Wisp") ?? "";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		isEnable = AddValueInput<bool>("isEnable".CapitalizeFirst());
	}

	private void ChangeState(Flow flow)
	{
		MainGame.Instance.GameSave.playerData.isWispEnabled = isEnable.value;
		MainGame.PlayerController.WispController.ChangeActiveState(isEnable.value);
		@out.Call(flow);
	}
}
