using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("CubePlus", false, "")]
[Name("Spawn WGO", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
public class Flow_SpawnWGO : MyFlowNode
{
	private WorldGameObject o_wgo;

	protected override void RegisterPorts()
	{
		ValueInput<GameObject> par_go = AddValueInput<GameObject>("Point");
		ValueInput<string> par_obj_id = AddValueInput<string>("Object id");
		ValueInput<string> par_custom_tag = AddValueInput<string>("Custom tag");
		ValueInput<bool> par_do_on_came = AddValueInput<bool>("Do OnCame to GDPoint");
		AddValueOutput("WGO", () => o_wgo);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			o_wgo = GS.Spawn(par_obj_id.value, par_go.value.transform, par_custom_tag.value);
			if (o_wgo == null)
			{
				Debug.LogError("Couldn't spawn: " + par_obj_id.value + " at " + par_go.value);
			}
			else if (par_do_on_came.value)
			{
				GDPoint component = par_go.value.GetComponent<GDPoint>();
				if (component != null)
				{
					o_wgo.OnCameToGDPoint(component);
				}
			}
			flow_out.Call(f);
		});
	}
}
