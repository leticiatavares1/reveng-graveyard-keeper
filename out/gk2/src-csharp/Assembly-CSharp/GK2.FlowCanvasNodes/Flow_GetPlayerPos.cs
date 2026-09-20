using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get Player Pos", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_GetPlayerPos : GKCustomFlowNode
{
	private ValueOutput<Vector3> posValOut;

	protected override void RegisterPorts()
	{
		posValOut = AddValueOutput("Pos", () => MainGame.PlayerController.transform.position);
	}
}
