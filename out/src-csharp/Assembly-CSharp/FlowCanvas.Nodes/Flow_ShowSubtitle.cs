using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Show Subtitle", 0)]
[Category("Game Actions")]
public class Flow_ShowSubtitle : MyFlowNode
{
	public override string name
	{
		get
		{
			if (string.IsNullOrEmpty(GetInputValuePort<string>("Subtitle text").value))
			{
				return "Remove subtitle";
			}
			return "Show subtitle";
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<string> in_subtitle_text = AddValueInput<string>("Subtitle text");
		ValueInput<bool> in_autoclose = AddValueInput<bool>("autoclose");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_autoclosed = AddFlowOutput("Autoclosed");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(in_subtitle_text.value))
			{
				if (GUIElements.me.illustrations_gui.is_open)
				{
					GUIElements.me.illustrations_gui.Hide();
				}
			}
			else
			{
				if (!GUIElements.me.illustrations_gui.is_open)
				{
					GUIElements.me.illustrations_gui.Open(with_dark_back: false);
				}
				GUIElements.me.illustrations_gui.SetText(in_subtitle_text.value, out var hold_time);
				if (in_autoclose.value)
				{
					GJTimer.AddTimer(hold_time, delegate
					{
						GUIElements.me.illustrations_gui.Hide();
						GJTimer.AddTimer(0.5f, delegate
						{
							flow_autoclosed.Call(f);
						});
					});
				}
			}
			flow_out.Call(f);
		});
	}
}
