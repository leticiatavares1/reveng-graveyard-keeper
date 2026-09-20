using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Tavern Engine Need to spawn visitor", 0)]
[Category("Game Actions")]
public class Flow_TavernEngineNeedToSpawn : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_need_spawn = AddFlowOutput("Need to spawn");
		FlowOutput flow_do_nothing = AddFlowOutput("Do nothing");
		FlowOutput flow_need_to_remove = AddFlowOutput("Need to remove");
		AddFlowInput("In", delegate(Flow f)
		{
			int count = MainGame.me.save.players_tavern_engine.visitors.Count;
			int num = Mathf.FloorToInt((float)MainGame.me.save.players_tavern_engine.locks.Count * 0.6f) - count;
			if (num > 0)
			{
				flow_need_spawn.Call(f);
			}
			else if (num < 0)
			{
				flow_need_to_remove.Call(f);
			}
			else
			{
				flow_do_nothing.Call(f);
			}
		});
	}
}
