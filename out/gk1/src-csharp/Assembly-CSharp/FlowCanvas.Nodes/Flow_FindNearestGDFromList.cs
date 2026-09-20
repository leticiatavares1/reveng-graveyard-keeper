using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Functions")]
[Name("Find Nearest GD point from List<gd_tag>", 0)]
[ContextDefinedOutputs(new Type[] { typeof(GameObject) })]
public class Flow_FindNearestGDFromList : MyFlowNode
{
	public List<string> gd_point_tags = new List<string>();

	protected override void RegisterPorts()
	{
		WorldGameObject _wgo = null;
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("GD point", delegate
		{
			if ((_wgo = WGOParamOrSelf(par_wgo)) == null)
			{
				Debug.LogError("Error: WGO is null");
			}
			return WorldMap.FIndNearestGDPointFromList(gd_point_tags, _wgo).gameObject;
		});
	}
}
