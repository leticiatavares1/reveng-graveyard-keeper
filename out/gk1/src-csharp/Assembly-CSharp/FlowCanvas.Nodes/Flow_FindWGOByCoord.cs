using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Find WGO By Coordinates", 0)]
[Category("Game Functions")]
[Icon("Cube", false, "")]
[Color("eed9a7")]
[ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
public class Flow_FindWGOByCoord : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_obj_id = AddValueInput<string>("Obj ID", "obj_id");
		ValueInput<string> par_ctag = AddValueInput<string>("Custom tag", "custom_tag");
		ValueInput<Vector2> par_coord = AddValueInput<Vector2>("Coords");
		AddValueOutput("WGO", delegate
		{
			List<WorldGameObject> list = new List<WorldGameObject>();
			string value = par_obj_id.value;
			list = WorldMap.FindWGOs(new string[1] { value }, par_coord.value, par_ctag.value);
			if (list != null)
			{
				if (list.Count == 0)
				{
					Debug.LogError("Flow_FindWGO: no input parameters set");
					return (WorldGameObject)null;
				}
				return list[0];
			}
			Debug.LogError("Flow_FindWGO: wgos is null");
			return (WorldGameObject)null;
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("obj_id");
		MakeStringNullIfEmpty("custom_tag");
	}
}
