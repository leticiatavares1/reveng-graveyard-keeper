using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

[Name("Shake Camera", 0)]
[Category("Game/Cutscenes")]
public class Flow_PlayCameraNoise : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	public CameraNoiseType type;

	public bool useAdditionalOptions;

	[ShowIf("useAdditionalOptions", 1)]
	public float fadeDuration;

	[ShowIf("useAdditionalOptions", 1)]
	public float amplitude = 1f;

	private ValueInput<float> duration;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ShakeCamera);
		@out = AddFlowOutput("out".CapitalizeFirst());
		duration = AddValueInput<float>("duration".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
	}

	private void ShakeCamera(Flow flow)
	{
		if (duration.value == 0f)
		{
			Debug.LogError("Flow_PlayCameraNoise: Noise duration can't be 0.");
			onFinished.Call(flow);
		}
		else if (useAdditionalOptions)
		{
			CameraSystem.Instance.ActiveCameraController.ShakeCamera(type, duration.value, fadeDuration, amplitude, delegate
			{
				onFinished.Call(flow);
			});
		}
		else
		{
			CameraSystem.Instance.ActiveCameraController.ShakeCamera(type, duration.value, 0f, 1f, delegate
			{
				onFinished.Call(flow);
			});
		}
		@out.Call(flow);
	}
}
