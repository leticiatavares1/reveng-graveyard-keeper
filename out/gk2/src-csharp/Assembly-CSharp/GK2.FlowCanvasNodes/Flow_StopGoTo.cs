using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Stop GoTo", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
[Icon("Halt", false, "")]
public class Flow_StopGoTo : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool stopPlayer;

	[GatherPortsCallback]
	[ShowIf("stopPlayer", 1)]
	public bool goTo = true;

	protected FlowInput @in;

	protected FlowOutput @out;

	public override int MinWidth => 200;

	public override string name
	{
		get
		{
			if (!stopPlayer)
			{
				return "Stop GoTo Wgo";
			}
			if (!goTo)
			{
				return "Stop Player";
			}
			return "Stop GoTo Player";
		}
	}

	protected override void RegisterPorts()
	{
		if (!stopPlayer)
		{
			base.RegisterPorts();
		}
		@in = AddFlowInput("in".CapitalizeFirst(), Stop);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	protected virtual void Stop(Flow flow)
	{
		WgoData wgoData = ((!stopPlayer) ? GetWgoData() : null);
		MovementComponent movementComponent = (stopPlayer ? MainGame.PlayerController.MovementComponent : wgoData?.MovementComponent);
		if (stopPlayer && !goTo)
		{
			MainGame.PlayerController.PhysicalBody.StopMoving();
		}
		else if (movementComponent != null && movementComponent.IsMoving)
		{
			movementComponent.ForceStop();
		}
		@out.Call(flow);
	}
}
