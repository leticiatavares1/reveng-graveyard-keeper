using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("CharSleep", 0)]
[Category("Game Actions")]
public class Flow_CharSleep : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput on_doesnt_need_sleep = AddFlowOutput("On Doesnt Need Sleep");
		FlowOutput on_appeared = AddFlowOutput("On Appeared");
		FlowOutput flow_after_save = AddFlowOutput("After save");
		FlowOutput on_wake_up = AddFlowOutput("On Wake Up");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.sleep_gui.Open(delegate
			{
				on_appeared.Call(f);
			}, delegate
			{
				on_wake_up.Call(f);
			}, delegate
			{
				on_doesnt_need_sleep.Call(f);
			}, delegate
			{
				flow_after_save.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
