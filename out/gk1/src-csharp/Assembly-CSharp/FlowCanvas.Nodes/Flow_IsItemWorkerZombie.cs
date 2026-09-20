using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Returns bool")]
[Name("Is Item The Working Zombie", 0)]
public class Flow_IsItemWorkerZombie : MyFlowNode
{
	protected override void RegisterPorts()
	{
		bool is_worker = false;
		AddValueOutput("Is worker", () => is_worker);
		ValueInput<Item> item = AddValueInput<Item>("Item");
		FlowOutput flow_yes = AddFlowOutput("True");
		FlowOutput flow_no = AddFlowOutput("False");
		AddFlowInput("In", delegate(Flow f)
		{
			if (item.value == null || item.value.IsEmpty())
			{
				Debug.LogError("The item is null or empty!(Flow_IsItemWorkerZombie)");
				flow_no.Call(f);
			}
			else
			{
				is_worker = item.value.is_worker;
				if (is_worker)
				{
					flow_yes.Call(f);
				}
				else
				{
					flow_no.Call(f);
				}
			}
		});
	}
}
