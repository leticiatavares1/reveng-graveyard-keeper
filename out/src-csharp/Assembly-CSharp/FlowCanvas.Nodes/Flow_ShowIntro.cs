using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Show Intro", 0)]
[Category("Game Actions")]
[Description("Shows game's intro playing from the start of game")]
[Color("79aa3d")]
public class Flow_ShowIntro : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<bool> in_no_words = AddValueInput<bool>("no words");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_on_finished = AddFlowOutput("On finished");
		AddFlowInput("In", delegate(Flow f)
		{
			Intro.need_show_first_intro = true;
			Intro.ShowIntro(delegate
			{
				Intro.need_show_first_intro = false;
				flow_on_finished.Call(f);
			}, in_no_words.value, stop_all_playlist: false);
			flow_out.Call(f);
		});
	}
}
