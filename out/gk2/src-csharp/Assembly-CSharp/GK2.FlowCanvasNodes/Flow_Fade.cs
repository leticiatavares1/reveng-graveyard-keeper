using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Fade", 0)]
[Category("Game/Cutscenes")]
public class Flow_Fade : GKCustomFlowNode
{
	public enum FadeType
	{
		In,
		Out,
		InOut
	}

	[GatherPortsCallback]
	public FadeType fadeType;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onMiddle;

	private FlowOutput onFinished;

	private ValueInput<bool> instant;

	private ValueInput<float> fadeTime;

	public override string name => fadeType switch
	{
		FadeType.In => "Fade In", 
		FadeType.Out => "Fade Out", 
		FadeType.InOut => "Fade In Out", 
		_ => base.name, 
	};

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoFade);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (fadeType == FadeType.InOut)
		{
			onMiddle = AddFlowOutput("onMiddle".CapitalizeFirst());
		}
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		instant = AddValueInput<bool>("instant?");
		fadeTime = AddValueInput<float>("fadeTime");
		fadeTime.SetDefaultAndSerializedValue(0.5f);
	}

	private void DoFade(Flow flow)
	{
		UIFade fadeElement = LazyUI.Get<UIFade>();
		if (!instant.value)
		{
			switch (fadeType)
			{
			case FadeType.In:
				fadeElement.FadeIn(fadeTime.value, delegate
				{
					onFinished.Call(flow);
				}, FadeFlag.FlowScript);
				break;
			case FadeType.Out:
				fadeElement.FadeOut(fadeTime.value, delegate
				{
					onFinished.Call(flow);
				}, FadeFlag.FlowScript);
				break;
			case FadeType.InOut:
				fadeElement.FadeIn(fadeTime.value / 2f, delegate
				{
					onMiddle.Call(flow);
					fadeElement.FadeOut(fadeTime.value / 2f, delegate
					{
						onFinished.Call(flow);
					}, FadeFlag.FlowScript);
				}, FadeFlag.FlowScript);
				break;
			}
		}
		else
		{
			switch (fadeType)
			{
			case FadeType.In:
				fadeElement.FadeInInstant(FadeFlag.FlowScript);
				break;
			case FadeType.Out:
				fadeElement.FadeOutInstant(FadeFlag.FlowScript);
				break;
			case FadeType.InOut:
				fadeElement.FadeInInstant(FadeFlag.FlowScript);
				fadeElement.FadeOutInstant(FadeFlag.FlowScript);
				break;
			}
		}
		@out.Call(flow);
		if (instant.value)
		{
			onFinished.Call(flow);
		}
	}
}
