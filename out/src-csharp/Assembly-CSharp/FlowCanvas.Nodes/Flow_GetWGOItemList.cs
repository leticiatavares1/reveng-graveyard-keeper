using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Color("eed9a7")]
[Icon("Cube", false, "")]
[Category("Game Functions")]
[Name("Get WGO List Item", 0)]
[ContextDefinedOutputs(new Type[] { typeof(List<WorldGameObject>) })]
public class Flow_GetWGOItemList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_me_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("List<Item>", delegate
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_me_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
				return (List<Item>)null;
			}
			return worldGameObject.data.inventory;
		});
	}
}
