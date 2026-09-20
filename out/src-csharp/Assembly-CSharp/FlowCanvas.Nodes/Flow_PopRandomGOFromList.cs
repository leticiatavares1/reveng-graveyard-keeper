using System;
using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Pop Random GO from List", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[ContextDefinedOutputs(new Type[] { typeof(GameObject) })]
public class Flow_PopRandomGOFromList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<GameObject>> par_list = AddValueInput<List<GameObject>>("List");
		AddValueOutput("GameObject", delegate
		{
			if (par_list.value.Count != 0)
			{
				int index = NGUITools.RandomRange(0, par_list.value.Count);
				GameObject result = par_list.value[index];
				par_list.value.RemoveAt(index);
				return result;
			}
			return (GameObject)null;
		});
	}
}
