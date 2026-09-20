using System;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Sleep Fade", 0)]
[Category("Game/Cutscenes")]
public class Flow_SleepFade : GKCustomFlowNode
{
	public enum FadeType
	{
		In,
		Out
	}

	[GatherPortsCallback]
	public FadeType fadeType;

	[GatherPortsCallback]
	[ShowIf("fadeType", 0)]
	public SleepAnimType animType;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private ValueInput<bool> instant;

	public override string name => fadeType switch
	{
		FadeType.In => "Sleep Fade In", 
		FadeType.Out => "Sleep Fade Out", 
		_ => base.name, 
	};

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoSleepFade);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		instant = AddValueInput<bool>("instant?");
	}

	private void DoSleepFade(Flow flow)
	{
		UISleepFade uISleepFade = LazyUI.Get<UISleepFade>();
		if (instant.value)
		{
			switch (fadeType)
			{
			case FadeType.In:
				uISleepFade.FadeInInstant();
				break;
			case FadeType.Out:
				uISleepFade.FadeOutInstant();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			onFinished.Call(flow);
		}
		else
		{
			switch (fadeType)
			{
			case FadeType.In:
				uISleepFade.FadeIn(delegate
				{
					onFinished.Call(flow);
				}, FadeFlag.Common, blockInterceptions: true, animType);
				break;
			case FadeType.Out:
				uISleepFade.FadeOut(delegate
				{
					onFinished.Call(flow);
				});
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		@out.Call(flow);
	}
}
