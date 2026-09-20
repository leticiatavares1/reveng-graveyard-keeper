using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Player Wisp Target", 0)]
[Category("Game/Environment")]
public class Flow_PlayerWisp : GKCustomFlowNode
{
	[GatherPortsCallback]
	public PlayerWispControlType targetType;

	public bool teleport;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GDPointData> gdPoint;

	private ValueInput<Transform> customTarget;

	private ValueInput<Vector3> customVector;

	private ValueInput<WgoData> wgoDataInput;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetTarget);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (targetType == PlayerWispControlType.GDPoint)
		{
			gdPoint = AddValueInput<GDPointData>("gdPoint");
		}
		if (targetType == PlayerWispControlType.Transform)
		{
			customTarget = AddValueInput<Transform>("customTarget");
		}
		if (targetType == PlayerWispControlType.Vector)
		{
			customVector = AddValueInput<Vector3>("customVector");
		}
		if (targetType == PlayerWispControlType.WgoData)
		{
			wgoDataInput = AddValueInput<WgoData>("wgoDataInput");
		}
	}

	private void SetTarget(Flow flow)
	{
		switch (targetType)
		{
		case PlayerWispControlType.Player:
			MainGame.PlayerController.EnableWispControl();
			MainGame.PlayerController.WispController.ChangeTargetType(WispTargetType.Transform);
			MainGame.PlayerController.View.UpdateWispDirection();
			break;
		case PlayerWispControlType.GDPoint:
			MainGame.PlayerController.DisableWispControl();
			MainGame.PlayerController.WispController.ChangeTargetType(WispTargetType.GDPoint);
			MainGame.PlayerController.WispController.SetTargetGDPoint(gdPoint.value);
			break;
		case PlayerWispControlType.Transform:
			MainGame.PlayerController.DisableWispControl();
			MainGame.PlayerController.WispController.ChangeTargetType(WispTargetType.Transform);
			MainGame.PlayerController.WispController.SetTargetTransform(customTarget.value);
			break;
		case PlayerWispControlType.Vector:
			MainGame.PlayerController.DisableWispControl();
			MainGame.PlayerController.WispController.ChangeTargetType(WispTargetType.Vector);
			MainGame.PlayerController.WispController.SetTargetVector(customVector.value);
			break;
		case PlayerWispControlType.WgoData:
			MainGame.PlayerController.DisableWispControl();
			MainGame.PlayerController.WispController.ChangeTargetType(WispTargetType.WgoData);
			MainGame.PlayerController.WispController.SetTargetWgoData(wgoDataInput.value);
			break;
		}
		if (teleport)
		{
			MainGame.PlayerController.WispController.TeleportToTarget();
		}
		@out.Call(flow);
	}
}
