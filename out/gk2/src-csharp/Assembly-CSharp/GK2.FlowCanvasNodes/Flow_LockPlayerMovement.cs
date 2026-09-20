using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Lock Player Movement", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_LockPlayerMovement : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool isLock;

	private FlowInput @in;

	private FlowOutput @out;

	public override string name => (isLock ? "Lock" : "Unlock") + " Player Movement";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), LockMovement);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void LockMovement(Flow flow)
	{
		MainGame.PlayerController.PhysicalBody.LockMovement(isLock);
		@out.Call(flow);
	}
}
