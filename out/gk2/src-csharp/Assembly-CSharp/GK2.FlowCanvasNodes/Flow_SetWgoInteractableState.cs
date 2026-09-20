using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Wgo Interactable State", 0)]
[Category("Game/Environment")]
[Color("f47dff")]
public class Flow_SetWgoInteractableState : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<bool> isInteractable;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SetInteractableState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		isInteractable = AddValueInput<bool>("isInteractable?");
	}

	private void SetInteractableState(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			wgoData.IsInteractable = isInteractable.value;
		}
		@out.Call(flow);
	}
}
