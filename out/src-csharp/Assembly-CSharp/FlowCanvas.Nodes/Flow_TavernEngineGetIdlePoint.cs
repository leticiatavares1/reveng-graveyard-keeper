using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Tavern Engine get idle point", 0)]
[Category("Game Actions")]
[Icon("CubePlus", false, "")]
public class Flow_TavernEngineGetIdlePoint : MyFlowNode
{
	private GDPoint out_point;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_visitor = AddValueInput<WorldGameObject>("visitor");
		ValueInput<bool> in_change_lock = AddValueInput<bool>("change lock");
		AddValueOutput("GDPoint", () => out_point);
		FlowOutput flow_out_ok = AddFlowOutput("OK");
		FlowOutput flow_out_nope = AddFlowOutput("Error");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_change_lock.value && in_visitor.value == null)
			{
				Debug.LogError("Flow_TavernEngineGetIdlePoint error: in_change_lock == true but visitor WGO is null!");
				flow_out_nope.Call(f);
			}
			else
			{
				long lock_by = (in_change_lock.value ? in_visitor.value.unique_id : (-1));
				if (MainGame.me.save.players_tavern_engine.TryGetAvailablePoint(out out_point, lock_by))
				{
					flow_out_ok.Call(f);
				}
				else
				{
					Debug.LogError("Flow_TavernEngineGetIdlePoint error: not found available GDPoint!");
					flow_out_nope.Call(f);
				}
			}
		});
	}
}
