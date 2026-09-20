using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("NULL GameObject", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[FlowNode.ContextDefinedOutputs(new Type[] { typeof(GameObject) })]
public class Flow_GetNullGameObject : PureFunctionNode<GameObject>
{
	public override GameObject Invoke()
	{
		return null;
	}
}
