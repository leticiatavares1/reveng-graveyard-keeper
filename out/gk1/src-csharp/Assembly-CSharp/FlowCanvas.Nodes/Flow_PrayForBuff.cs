using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Pray for buff", 0)]
public class Flow_PrayForBuff : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_finished = AddFlowOutput("Finished");
		FlowOutput flow_middle = AddFlowOutput("Middle");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.pray_craft.DoPrayForBuff(PrayLogics.last_pray_result.success, delegate
			{
				flow_finished.Call(f);
			}, delegate
			{
				flow_middle.Call(f);
			});
		});
	}
}
