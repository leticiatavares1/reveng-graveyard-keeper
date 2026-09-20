using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Apply WgoData Part State", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_ApplyWgoDataPartState : GKCustomFlowNodeWithWgoData
{
	protected FlowInput @in;

	protected FlowOutput @out;

	protected ValueInput<string> wgoPartId;

	public override string name => "Apply WgoData Part State";

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeWgoViewState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		wgoPartId = AddValueInput<string>("wgoPartId");
	}

	protected virtual void ChangeWgoViewState(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData == null)
		{
			Debug.LogError(string.Format("[{0}]: not found wgoData: {1}", "GKCustomFlowNode", wgoId));
		}
		else
		{
			wgoData.ApplyWgoPartState(wgoPartId.value);
		}
		@out.Call(flow);
	}
}
