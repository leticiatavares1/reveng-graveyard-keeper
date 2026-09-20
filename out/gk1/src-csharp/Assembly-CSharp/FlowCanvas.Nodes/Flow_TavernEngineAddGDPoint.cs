using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Tavern Engine Add GDPoint", 0)]
public class Flow_TavernEngineAddGDPoint : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<GDPoint> in_gd_point = AddValueInput<GDPoint>("GDPoint");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_gd_point.value == null)
			{
				Debug.LogError("Flow_TavernEngineAddGDPoint error: GDPoint is null!");
			}
			else
			{
				MainGame.me.save.players_tavern_engine.AddGDPoint(in_gd_point.value);
			}
			flow_out.Call(f);
		});
	}
}
