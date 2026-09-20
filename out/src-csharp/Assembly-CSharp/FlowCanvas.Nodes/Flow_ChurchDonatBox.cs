using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[FlowNode.ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
[Name("Get Church Donat Box", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[Icon("Human", false, "")]
public class Flow_ChurchDonatBox : PureFunctionNode<WorldGameObject>
{
	public override WorldGameObject Invoke()
	{
		return WorldMap.GetWorldGameObjectByCustomTag("donat_box_inside");
	}
}
