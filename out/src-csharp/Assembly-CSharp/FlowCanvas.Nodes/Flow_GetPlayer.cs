using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Get Player", 0)]
[FlowNode.ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
[Category("Game Functions")]
[Color("eed9a7")]
[Icon("Human", false, "")]
public class Flow_GetPlayer : PureFunctionNode<WorldGameObject>
{
	public override WorldGameObject Invoke()
	{
		return MainGame.me.player;
	}
}
