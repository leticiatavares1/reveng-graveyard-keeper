using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Set Time", 0)]
[Category("Game Actions")]
[Description("Set Time")]
public class Flow_SetTime : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<float> in_time = AddValueInput<float>("Time");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			TimeOfDay.me.SetTimeK(in_time.value);
			MainGame.me.gui_elements.hud.Update();
			TimeOfDay.me.Update();
			flow_out.Call(f);
		});
	}
}
