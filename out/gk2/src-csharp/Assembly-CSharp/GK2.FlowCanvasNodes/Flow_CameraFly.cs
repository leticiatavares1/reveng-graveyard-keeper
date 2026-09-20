using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Game/Camera Fly", 0)]
[Category("Game/Camera")]
[Color("8a8a8a")]
public class Flow_CameraFly : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool returnToPlayer;

	[GatherPortsCallback]
	public bool setPosition;

	[GatherPortsCallback]
	public bool useGdPointAsTarget;

	[GatherPortsCallback]
	public bool useWgoDataAsTarget;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFlyComplete;

	private ValueInput<Transform> targetTransform;

	private ValueInput<Vector3> targetPosition;

	private ValueInput<GDPointData> targetGdPoint;

	private ValueInput<WgoData> targetWgoData;

	private ValueInput<float> duration;

	public override string name => "Camera Fly" + (returnToPlayer ? " to Player" : (useWgoDataAsTarget ? " to WgoData" : (useGdPointAsTarget ? " to GDPoint" : "")));

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), CameraFly);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFlyComplete = AddFlowOutput("onFlyComplete".CapitalizeFirst());
		if (!returnToPlayer)
		{
			if (useWgoDataAsTarget)
			{
				targetWgoData = AddValueInput<WgoData>("targetWgoData");
			}
			else if (useGdPointAsTarget)
			{
				targetGdPoint = AddValueInput<GDPointData>("targetGdPoint");
			}
			else if (setPosition)
			{
				targetPosition = AddValueInput<Vector3>("targetPosition");
			}
			else
			{
				targetTransform = AddValueInput<Transform>("targetTransform");
			}
		}
		duration = AddValueInput<float>("duration");
		duration.SetDefaultAndSerializedValue(1f);
	}

	private void CameraFly(Flow flow)
	{
		CameraController cameraController = CameraSystem.Instance.GetCameraController(CameraType.Main);
		if (returnToPlayer || (!setPosition && !useGdPointAsTarget && !useWgoDataAsTarget))
		{
			cameraController.SetTarget((!returnToPlayer) ? targetTransform.value : MainGame.PlayerController.View.transform, duration.value, delegate
			{
				onFlyComplete.Call(flow);
			});
		}
		else if (useWgoDataAsTarget)
		{
			WgoData value = targetWgoData.value;
			Wgo wgo = ((value != null) ? GameScene.GetWgoViewGlobal(value.UniqueId) : null);
			if (wgo != null)
			{
				cameraController.SetTarget(wgo.transform, duration.value, delegate
				{
					onFlyComplete.Call(flow);
				});
			}
			else
			{
				Vector3 pos = value?.Position ?? Vector3.zero;
				cameraController.SetPosition(pos, duration.value, delegate
				{
					onFlyComplete.Call(flow);
				});
			}
		}
		else
		{
			Vector3 pos2 = ((!useGdPointAsTarget) ? targetPosition.value : ((targetGdPoint.value != null) ? targetGdPoint.value.Position : Vector3.zero));
			cameraController.SetPosition(pos2, duration.value, delegate
			{
				onFlyComplete.Call(flow);
			});
		}
		@out.Call(flow);
	}
}
