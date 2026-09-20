using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set WGO Movement Pause State", 0)]
[Category("Game/Environment")]
[Color("f47dff")]
public class Flow_SetWgoMovementPauseState : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<bool> isPaused;

	public override string name
	{
		get
		{
			if (isPaused == null || !isPaused.value)
			{
				return "<color=#5dff5d>▶</color> Resume WGO Movement";
			}
			return "<color=#ff5d5d>⏸</color> Pause WGO Movement";
		}
	}

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SetMovementPauseState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		isPaused = AddValueInput<bool>("isPaused");
	}

	private void SetMovementPauseState(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			wgoData.MovementComponent.IsPaused = isPaused.value;
		}
		@out.Call(flow);
	}
}
