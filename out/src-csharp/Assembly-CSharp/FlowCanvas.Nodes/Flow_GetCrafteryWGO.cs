using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Get Craftery WGO", 0)]
[Category("Game Functions")]
[ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
public class Flow_GetCrafteryWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		AddValueOutput("WGO", () => GUIElements.me.craft.GetCrafteryWGO());
	}
}
