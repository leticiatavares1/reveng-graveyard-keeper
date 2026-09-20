using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Wisp Direction", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SetWispDirection : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Direction> direction;

	private ValueInput<WispController> wispInput;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetDirection);
		@out = AddFlowOutput("out".CapitalizeFirst());
		direction = AddValueInput<Direction>("direction");
		wispInput = AddValueInput<WispController>("wispInput".CapitalizeFirst());
	}

	private void SetDirection(Flow flow)
	{
		WispController wispController = wispInput.value;
		if (wispController == null)
		{
			wispController = MainGame.PlayerController.WispController;
		}
		wispController.SetDirection(direction.value);
		@out.Call(flow);
	}
}
