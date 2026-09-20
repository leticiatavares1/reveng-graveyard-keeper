using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Drop Item and Fly", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Icon("ArrowDown", false, "")]
[Color("857fff")]
public class Flow_DropItemAndFly : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO to drop");
		ValueInput<string> par_res = AddValueInput<string>("Res_id");
		ValueInput<int> par_amount = AddValueInput<int>("Amount");
		ValueInput<string> par_gdp = AddValueInput<string>("GDP tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GDPoint gDPointByName = WorldMap.GetGDPointByName(par_gdp.value);
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				if (gDPointByName != null)
				{
					worldGameObject.DropItemAndFly(new Item(par_res.value, par_amount.value), gDPointByName.pos);
				}
				else
				{
					Debug.LogError("Trying to drop " + par_res.value + " to the null GD point");
					worldGameObject.DropItem(new Item(par_res.value, par_amount.value));
				}
				flow_out.Call(f);
			}
		});
	}
}
