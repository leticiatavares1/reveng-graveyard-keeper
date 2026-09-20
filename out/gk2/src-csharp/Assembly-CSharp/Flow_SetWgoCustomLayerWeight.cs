using System;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Set WGO Custom Layer Weight", 0)]
[Category("Game/Animation")]
public class Flow_SetWgoCustomLayerWeight : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public AnimationComponent.Layers layer;

	[GatherPortsCallback]
	public bool enable;

	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), SetCustomLayerWeight);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void SetCustomLayerWeight(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			wgoData.SetLayerWeightToAnimator((int)layer, Convert.ToInt32(enable));
		}
		else
		{
			Debug.LogError(string.Format("{0}: cannot set [{1}] layer on null WGO", "Flow_SetTriggerToAnimator", layer));
		}
		@out.Call(flow);
	}
}
