using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("IsMovingByMovementComponent", 0)]
[Category("Game/Environment")]
public class Flow_IsMovingByMovementComponent : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput trueOut;

	private FlowOutput falseOut;

	private bool isMovingByMovementComponent;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), IsMovingByMovementComponent);
		trueOut = AddFlowOutput("<color=green>✔</color>");
		falseOut = AddFlowOutput("<color=red>✘</color>");
	}

	private void IsMovingByMovementComponent(Flow flow)
	{
		WgoData wgoData = GetWgoData();
		if (wgoData != null)
		{
			isMovingByMovementComponent = wgoData.MovementComponent.IsMoving;
		}
		else
		{
			Debug.LogError("Flow_IsMovingByMovementComponent: WGOData is null");
		}
		if (isMovingByMovementComponent)
		{
			trueOut.Call(flow);
		}
		else
		{
			falseOut.Call(flow);
		}
	}
}
