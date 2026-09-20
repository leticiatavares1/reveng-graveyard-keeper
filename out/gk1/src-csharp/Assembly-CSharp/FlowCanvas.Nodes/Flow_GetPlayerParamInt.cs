using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("00ff00")]
[Category("Game Functions")]
[Name("Get Player Param Int", 0)]
[FlowNode.ContextDefinedOutputs(new Type[] { typeof(int) })]
[Icon("HumanArrow", false, "")]
public class Flow_GetPlayerParamInt : PureFunctionNode<int, string>
{
	public override int Invoke(string param)
	{
		return MainGame.me.player.GetParamInt(param);
	}
}
