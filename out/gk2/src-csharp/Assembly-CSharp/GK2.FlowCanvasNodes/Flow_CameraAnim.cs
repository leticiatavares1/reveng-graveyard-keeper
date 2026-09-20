using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Camera Animation", 0)]
[Category("Game/Cutscenes")]
public class Flow_CameraAnim : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private ValueInput<string> animationName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoAnimation);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		animationName = AddValueInput<string>("animationName");
	}

	private void DoAnimation(Flow flow)
	{
		Common();
		void Common()
		{
			CameraSystem.Instance.MainCamera.PlayAnimation(animationName.value, delegate
			{
				onFinished.Call(flow);
			});
			@out.Call(flow);
		}
	}
}
