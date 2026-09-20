using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Icon("Cube", false, "")]
[Name("Get current WGO", 0)]
[Category("Game Functions")]
[FlowNode.ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
[Color("00ff00")]
public class Flow_GetCurWGO : MyPureFunctionNode<WorldGameObject>
{
	public override WorldGameObject Invoke()
	{
		return base.wgo;
	}
}
