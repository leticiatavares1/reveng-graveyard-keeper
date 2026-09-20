using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[FlowNode.ContextDefinedOutputs(new Type[] { typeof(float) })]
[Icon("CubeArrow", false, "")]
[Category("Game Functions")]
[Name("Get WGO Param", 0)]
public class Flow_GetWGOParam : PureFunctionNode<float, string, WorldGameObject>
{
	public override float Invoke(string param, WorldGameObject wgo)
	{
		if (wgo == null)
		{
			Debug.LogError("WGO is null");
			return 0f;
		}
		return wgo.GetParam(param);
	}
}
