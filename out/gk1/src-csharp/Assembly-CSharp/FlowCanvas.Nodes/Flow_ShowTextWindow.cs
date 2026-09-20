using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Icon("TextWindow", false, "")]
[Name("Show Text Window", 0)]
public class Flow_ShowTextWindow : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> txt = AddValueInput<string>("Text");
		FlowOutput o = AddFlowOutput("Out");
		ValueInput<bool> is_cinematic = AddValueInput<bool>("Cinematic");
		FlowOutput on_closed = AddFlowOutput("On Closed");
		AddFlowInput("In", delegate(Flow f)
		{
			if (is_cinematic.value)
			{
				GUIElements.me.cinematic_text.Open(txt.value, delegate
				{
					on_closed.Call(f);
				});
			}
			else
			{
				GUIElements.me.text_window.Open(txt.value, delegate
				{
					on_closed.Call(f);
				});
			}
			o.Call(f);
		});
	}
}
