using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Set WGO Custom Animation Trigger", 0)]
[Category("Game/Animation")]
public class Flow_SetWgoCustomAnimationState : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool remove;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> triggerName;

	public override string name => (remove ? "Remove" : "Set") + " WGO Custom Animation Trigger";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SetTrigger);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!remove)
		{
			triggerName = AddValueInput<string>("triggerName".CapitalizeFirst());
		}
	}

	private void SetTrigger(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			string customAnimationTrigger = (remove ? "" : triggerName.value);
			wgoData.SetCustomAnimationTrigger(customAnimationTrigger);
		}
		else
		{
			Debug.LogError("Flow_SetTriggerToAnimator: cannot trigger [" + triggerName.value + "] on null WGO");
		}
		@out.Call(flow);
	}
}
