using System.Collections.Generic;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("Text Screen With Fade", 0)]
[Category("Game/UI")]
public class Flow_TextScreenFade : GKCustomFlowNode
{
	protected const float FADE_DURATION = 0.6f;

	[GatherPortsCallback]
	public bool severalPhrases;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onMiddle;

	private FlowOutput onFinished;

	private ValueInput<string> text;

	private ValueInput<List<string>> ids;

	private ValueInput<float> screenFadeTime;

	private ValueInput<float> textFadeTime;

	private ValueInput<float> textHoldTime;

	private ValueInput<float> pauseTime;

	private ValueInput<TextStyle> textStyle;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoFade);
		onMiddle = AddFlowOutput("onMiddle".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (severalPhrases)
		{
			ids = AddValueInput<List<string>>("ids");
		}
		else
		{
			text = AddValueInput<string>("text");
		}
		screenFadeTime = AddValueInput<float>("screenFadeTime".CapitalizeFirst());
		screenFadeTime.SetDefaultAndSerializedValue(0.6f);
		textFadeTime = AddValueInput<float>("textFadeTime".CapitalizeFirst());
		textFadeTime.SetDefaultAndSerializedValue(0.6f);
		textHoldTime = AddValueInput<float>("textHoldTime".CapitalizeFirst());
		textHoldTime.SetDefaultAndSerializedValue(1f);
		pauseTime = AddValueInput<float>("pauseTime".CapitalizeFirst());
		pauseTime.SetDefaultAndSerializedValue(1f);
		textStyle = AddValueInput<TextStyle>("textStyle".CapitalizeFirst());
	}

	private void DoFade(Flow flow)
	{
		UIFadeWithText uIFadeWithText = LazyUI.Get<UIFadeWithText>();
		if (severalPhrases)
		{
			uIFadeWithText.ScreenTextFade(ids.value, screenFadeTime.value, textFadeTime.value, pauseTime.value, textHoldTime.value, delegate
			{
				onMiddle.Call(flow);
			}, delegate
			{
				onFinished.Call(flow);
			}, textStyle.value);
		}
		else
		{
			uIFadeWithText.ScreenTextFade(text.value, screenFadeTime.value, textFadeTime.value, pauseTime.value, textHoldTime.value, delegate
			{
				onMiddle.Call(flow);
			}, delegate
			{
				onFinished.Call(flow);
			}, textStyle.value);
		}
		@out.Call(flow);
	}
}
