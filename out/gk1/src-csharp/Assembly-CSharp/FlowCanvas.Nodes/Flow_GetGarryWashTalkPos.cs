using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[FlowNode.ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
[Category("Game Functions")]
[Color("eed9a7")]
[Name("Get Garry Wash Talks Pos", 0)]
public class Flow_GetGarryWashTalkPos : PureFunctionNode<Transform>
{
	public override Transform Invoke()
	{
		return MainGame.me.player.GetComponent<PlayerComponent>().garry_wash_talk_pos.transform;
	}
}
