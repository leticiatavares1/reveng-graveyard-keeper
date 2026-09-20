using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Corner Talk", 0)]
[Category("Game Actions")]
[Icon("Dialogue", false, "")]
public class Flow_CornerTalk : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_txt = AddValueInput<string>("Text");
		ValueInput<string> par_spr = AddValueInput<string>("Sprite");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_on_finished = AddFlowOutput("On Finished");
		ValueInput<SmartSpeechEngine.VoiceID> par_voice = AddValueInput<SmartSpeechEngine.VoiceID>("Voice");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.corner_talk.Say(par_txt.value, delegate
			{
				flow_on_finished.Call(f);
			}, par_spr.value, par_voice.value);
			flow_out.Call(f);
		});
	}
}
