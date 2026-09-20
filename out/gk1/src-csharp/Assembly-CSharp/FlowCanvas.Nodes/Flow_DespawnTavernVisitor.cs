using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Despawn Tavern visitor", 0)]
[Category("Game Actions")]
[Icon("CubePlus", false, "")]
public class Flow_DespawnTavernVisitor : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_visitor = AddValueInput<WorldGameObject>("Visitor WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_visitor);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_DespawnTavernVisitor error: WGO is null!");
				flow_out.Call(f);
			}
			else
			{
				MainGame.me.save.players_tavern_engine.RemoveVisitor(worldGameObject);
				worldGameObject.DestroyMe();
				flow_out.Call(f);
			}
		});
	}
}
