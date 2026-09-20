using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Set Direction", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SetDirection : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool isForPlayer;

	[GatherPortsCallback]
	public bool setDirectionToPos;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Direction> direction;

	private ValueInput<Vector3> lookAtPos;

	public override string name => "Set Direction " + (isForPlayer ? "Player" : "WgoData");

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetDirection);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!isForPlayer)
		{
			base.RegisterPorts();
		}
		if (!setDirectionToPos)
		{
			direction = AddValueInput<Direction>("direction");
		}
		else
		{
			lookAtPos = AddValueInput<Vector3>("lookAtPos");
		}
	}

	private void SetDirection(Flow flow)
	{
		if (isForPlayer)
		{
			if (!setDirectionToPos)
			{
				MainGame.PlayerController.View.PlayerAnimation.SetDirection(direction.value);
			}
			else
			{
				MainGame.PlayerController.View.PlayerAnimation.SetDirection(new Vector2(lookAtPos.value.x, lookAtPos.value.z));
			}
		}
		else
		{
			WgoData wgoData = GetWgoData();
			if (wgoData != null)
			{
				wgoData.direction.Value = ((!setDirectionToPos) ? direction.value.ConvertToVector2XZ() : new Vector2(lookAtPos.value.x, lookAtPos.value.z));
			}
		}
		@out.Call(flow);
	}
}
