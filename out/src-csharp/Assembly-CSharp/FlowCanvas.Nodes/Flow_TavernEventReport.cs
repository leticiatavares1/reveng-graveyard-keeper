using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("CubePlus", false, "")]
[Category("Game Actions")]
[Name("Tavern event report", 0)]
public class Flow_TavernEventReport : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_barmen = AddValueInput<WorldGameObject>("barmen");
		ValueInput<string> in_event = AddValueInput<string>("event ID");
		FlowOutput flow_out = AddFlowOutput("out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_barmen.value == null)
			{
				Debug.LogError("Flow_TavernEventReport error: barmen WGO is null!");
				flow_out.Call(f);
			}
			else
			{
				TavernEventDefinition data = GameBalance.me.GetData<TavernEventDefinition>(in_event.value);
				if (data == null)
				{
					Debug.LogError("Flow_TavernEventReport error: event_definition is null!");
					flow_out.Call(f);
				}
				else
				{
					GUIElements.me.tavern_event_report.OpenPlayersTavernEventResult(in_barmen.value, data);
					flow_out.Call(f);
				}
			}
		});
	}
}
