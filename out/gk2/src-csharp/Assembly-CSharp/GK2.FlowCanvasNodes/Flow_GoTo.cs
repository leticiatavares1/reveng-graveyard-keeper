using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("GoTo", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
[ParadoxNotion.Design.Icon("Sequencer", false, "")]
public class Flow_GoTo : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool movePlayer;

	[GatherPortsCallback]
	public bool useGdDataInsteadId;

	[GatherPortsCallback]
	[InspectorName("Return path length")]
	[ShowIf("movePlayer", 0)]
	public bool returnPathLength;

	protected FlowInput @in;

	protected FlowOutput @out;

	protected FlowOutput onFinish;

	protected ValueOutput<GDPointData> gdPointDataOutput;

	protected ValueInput<float> speed;

	protected ValueInput<string> gdPointId;

	protected ValueInput<GDPointData> gdPointData;

	protected ValueInput<MovementType> navigation;

	protected ValueInput<string> fireEventOnFinish;

	public override int MinWidth => 200;

	public override string name
	{
		get
		{
			if (!movePlayer)
			{
				return "GoTo Wgo";
			}
			return "GoTo Player";
		}
	}

	protected override void RegisterPorts()
	{
		if (!movePlayer)
		{
			base.RegisterPorts();
		}
		@in = AddFlowInput("in".CapitalizeFirst(), GoTo);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinish = AddFlowOutput("onFinish".CapitalizeFirst());
		gdPointDataOutput = AddValueOutput("GDPointData", () => gdPointData.value);
		speed = AddValueInput<float>("speed".CapitalizeFirst());
		if (!useGdDataInsteadId)
		{
			gdPointId = AddValueInput<string>("gdPointId");
		}
		else
		{
			gdPointData = AddValueInput<GDPointData>("gdPointData");
		}
		navigation = AddValueInput<MovementType>("navigation");
		navigation.serializedValue = MovementType.GDGraph;
		fireEventOnFinish = AddValueInput<string>("fireEventOnFinish");
		speed.SetDefaultAndSerializedValue(1.5f);
	}

	protected virtual void GoTo(Flow flow)
	{
		GDPointData gDPointData = ((!useGdDataInsteadId) ? MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(gdPointId.value) : gdPointData.value);
		WgoData wgoData = ((!movePlayer) ? GetWgoData() : null);
		MovementComponent movementComponent = ((!movePlayer) ? wgoData.MovementComponent : MainGame.PlayerController.MovementComponent);
		Seeker customSeeker = ((!movePlayer) ? null : MainGame.PlayerController.PlayerLocalAreaMovement.Seeker);
		string startWorldId = ((!movePlayer) ? wgoData.WorldId : MainGame.PlayerData.currentGameSceneId);
		BindPathLengthCallback(wgoData, movementComponent);
		MovementComponent.StartPathResult result = ((navigation.value != MovementType.WorldZone) ? movementComponent.StartPath(gDPointData.Position, startWorldId, gDPointData.GameSceneDataId, navigation.value, speed.value, fireEventOnFinish.value ?? string.Empty, delegate
		{
			onFinish.Call(flow);
		}, customSeeker) : movementComponent.StartPath(gDPointData.Position, movePlayer ? MainGame.PlayerData.CurrentWorldZoneData.navigationGraph : wgoData.WorldZoneData.navigationGraph, startWorldId, speed.value, fireEventOnFinish.value ?? string.Empty, delegate
		{
			onFinish.Call(flow);
		}));
		HandlePathLengthStartResult(wgoData, movementComponent, result);
		@out.Call(flow);
	}

	private void BindPathLengthCallback(WgoData wgoData, MovementComponent movementComponent)
	{
		if (returnPathLength && !movePlayer && wgoData != null)
		{
			movementComponent.SetOnPathLengthReady(delegate(float length)
			{
				wgoData.SetGameRes("lastGoToPathLength", length);
			});
		}
	}

	private void HandlePathLengthStartResult(WgoData wgoData, MovementComponent movementComponent, MovementComponent.StartPathResult result)
	{
		if (returnPathLength && !movePlayer && wgoData != null)
		{
			switch (result)
			{
			case MovementComponent.StartPathResult.AlreadyAtDestinationPoint:
				wgoData.SetGameRes("lastGoToPathLength", 0f);
				movementComponent.SetOnPathLengthReady(null);
				break;
			default:
				movementComponent.SetOnPathLengthReady(null);
				break;
			case MovementComponent.StartPathResult.Started:
				break;
			}
		}
	}
}
