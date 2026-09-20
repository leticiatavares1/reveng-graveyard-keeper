using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Pray: Spread faith income", 0)]
public class Flow_SpreadFaithIncome : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_prayers = AddValueInput<List<WorldGameObject>>("prayers");
		ValueInput<int> par_faith = AddValueInput<int>("faith");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			PrayLogics.SpreadFaithIncome(par_prayers.value, par_faith.value);
			flow_out.Call(f);
		});
	}
}
