using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Set Camera Target", 0)]
[Category("Game/Camera")]
[Color("8a8a8a")]
public class Flow_SetCameraTarget : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private ValueInput<float> duration;

	private ValueInput<Transform> target;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetCameraTarget);
		@out = AddFlowOutput("out".CapitalizeFirst());
		duration = AddValueInput<float>("duration".CapitalizeFirst());
		target = AddValueInput<Transform>("target".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
	}

	private void SetCameraTarget(Flow flow)
	{
		if (target.value == null)
		{
			Debug.LogError("Flow_SetCameraTarget: No camera target was set.");
			onFinished.Call(flow);
		}
		else if (duration.value == 0f)
		{
			CameraController.SetFollowTargetInstant(target.value);
			onFinished.Call(flow);
		}
		else
		{
			CameraController.SetFollowTarget(target.value, duration.value, delegate
			{
				onFinished.Call(flow);
			});
		}
		@out.Call(flow);
	}
}
