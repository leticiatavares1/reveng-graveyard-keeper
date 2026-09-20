using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Show Illustration", 0)]
public class Flow_ShowIllustration : MyFlowNode
{
	private float _hold_time;

	protected override void RegisterPorts()
	{
		ValueInput<string> in_illustration = AddValueInput<string>("illustration name");
		ValueInput<string> in_text = AddValueInput<string>("text");
		AddValueOutput("hold time", () => _hold_time);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.illustrations_gui.ShowIllustration(in_illustration.value);
			GUIElements.me.illustrations_gui.SetText(in_text.value, out _hold_time);
			flow_out.Call(f);
		});
	}
}
