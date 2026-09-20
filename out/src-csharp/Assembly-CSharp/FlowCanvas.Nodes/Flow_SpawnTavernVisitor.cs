using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Spawn Tavern visitor", 0)]
[Category("Game Actions")]
[Icon("CubePlus", false, "")]
public class Flow_SpawnTavernVisitor : MyFlowNode
{
	private WorldGameObject o_wgo;

	protected override void RegisterPorts()
	{
		ValueInput<GameObject> par_go = AddValueInput<GameObject>("Point");
		ValueInput<string> par_obj_id = AddValueInput<string>("Object id");
		ValueInput<string> par_custom_tag = AddValueInput<string>("Custom tag");
		AddValueOutput("WGO", () => o_wgo);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			o_wgo = GS.Spawn(par_obj_id.value, par_go.value.transform, par_custom_tag.value);
			if (o_wgo == null)
			{
				Debug.LogError("Couldn't spawn: " + par_obj_id.value + " at " + par_go.value);
			}
			else
			{
				MainGame.me.save.players_tavern_engine.AddNewVisitor(o_wgo);
			}
			flow_out.Call(f);
		});
	}
}
