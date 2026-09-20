using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Functions")]
[Name("Pop Random WGO from List", 0)]
[Color("eed9a7")]
[ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
public class Flow_PopRandomWGOFromList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_list = AddValueInput<List<WorldGameObject>>("List");
		AddValueOutput("WGO", delegate
		{
			if (par_list.value.Count != 0)
			{
				int index = UnityEngine.Random.Range(0, par_list.value.Count);
				WorldGameObject result = par_list.value[index];
				par_list.value.RemoveAt(index);
				return result;
			}
			return (WorldGameObject)null;
		});
	}
}
