using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Finish Sermon", 0)]
[Category("Game/Cutscenes")]
[Color("8a8a8a")]
public class Flow_FinishSermon : GKCustomFlowNode
{
	protected FlowInput @in;

	protected FlowOutput @out;

	protected FlowOutput onFinish;

	protected ValueOutput<bool> result;

	private bool GetCurrentSermonResult()
	{
		return MainGame.PlayerData.currentSermon.success;
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), FinishSermon);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinish = AddFlowOutput("onFinish".CapitalizeFirst());
		result = AddValueOutput("result", GetCurrentSermonResult);
	}

	private void FinishSermon(Flow flow)
	{
		MainGame.PlayerController.View.FinishSermon(GetCurrentSermonResult(), delegate
		{
			onFinish.Call(flow);
		});
		@out.Call(flow);
	}
}
