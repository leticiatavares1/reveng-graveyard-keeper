using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[FlowNode.ContextDefinedOutputs(new Type[] { typeof(float) })]
[Icon("HumanArrow", false, "")]
[Category("Game Functions")]
[Name("Get Player Param", 0)]
[Color("00ff00")]
public class Flow_GetPlayerParam : PureFunctionNode<float, string>
{
	public override float Invoke(string param)
	{
		return MainGame.me.player.GetParam(param);
	}
}
