using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Pray: Spread money income", 0)]
[Category("Game Actions")]
public class Flow_SpreadMoneyIncome : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_prayers = AddValueInput<List<WorldGameObject>>("prayers");
		ValueInput<float> par_money = AddValueInput<float>("money");
		ValueInput<float> par_chance = AddValueInput<float>("chance");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			PrayLogics.SpreadMoneyIncome(par_prayers.value, par_money.value, par_chance.value);
			flow_out.Call(f);
		});
	}
}
