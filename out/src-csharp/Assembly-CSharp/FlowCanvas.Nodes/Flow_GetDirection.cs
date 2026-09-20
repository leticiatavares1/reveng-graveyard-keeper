using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Direction", 0)]
[Category("Game Functions")]
[Color("eed9a7")]
[Icon("Human", false, "")]
[ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
public class Flow_GetDirection : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("Char WGO");
		ValueInput<bool> in_invert = AddValueInput<bool>("invert");
		AddValueOutput("Direction", delegate
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				return Direction.None;
			}
			if (!worldGameObject.components.character.enabled)
			{
				return Direction.None;
			}
			Direction direction = worldGameObject.components.character.direction.ToDirection();
			if (direction == Direction.None)
			{
				Debug.LogError("Wrong char direction!");
				return direction;
			}
			if (in_invert.value)
			{
				direction = direction.Opposite();
			}
			return direction;
		});
	}
}
