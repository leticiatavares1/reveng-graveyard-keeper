using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("Cross", false, "")]
[Name("Destroy WGO or Stages", 0)]
[Category("Game/Actions")]
[Description("Destroys WGO by obj_id or by its' possible stages on given world position")]
public class Flow_DestroyWGOOrStages : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> par_wgo_name;

	private ValueInput<Vector2> par_coord;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", DestroyOrFindStages);
		@out = AddFlowOutput("Out");
		par_wgo_name = AddValueInput<string>("obj id");
		par_coord = AddValueInput<Vector2>("world position");
	}

	private void DestroyOrFindStages(Flow flow)
	{
		List<WorldGameObject> list = WorldMap.FindWGOs(new string[1] { par_wgo_name.value }, par_coord.value, string.Empty);
		if (list != null && list.Count > 0)
		{
			list[0].DestroyMe();
			Debug.Log("Destroy WGO with ID " + list[0].obj_id + " at coords " + par_coord.value.ToString());
		}
		else
		{
			string[] stagesOfWGO = par_wgo_name.value.GetStagesOfWGO();
			if (stagesOfWGO != null && stagesOfWGO.Length != 0)
			{
				List<WorldGameObject> list2 = WorldMap.FindWGOs(stagesOfWGO, par_coord.value, string.Empty);
				if (list2 != null && list2.Count > 0)
				{
					for (int i = 0; i < list2.Count; i++)
					{
						Debug.Log("Destroy stage " + list2[i].obj_id + " for WGO " + par_wgo_name.value + " at coords " + par_coord.value.ToString());
						list2[i].DestroyMe();
					}
				}
				else
				{
					Debug.LogError("Not found stages for " + par_wgo_name.value + " at coords " + par_coord.value.ToString());
				}
			}
			else
			{
				Debug.LogError("Couldn't find stages for WGO with obj_id" + par_wgo_name.value + " at coords " + par_coord.value.ToString());
			}
		}
		@out.Call(flow);
	}
}
