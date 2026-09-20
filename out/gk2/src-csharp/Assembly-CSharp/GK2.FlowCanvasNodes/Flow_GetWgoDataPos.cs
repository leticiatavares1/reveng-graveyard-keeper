using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Get WgoData Pos", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
public class Flow_GetWgoDataPos : GKCustomFlowNodeWithWgoData
{
	private ValueOutput<Vector3> wgoDataPos;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		wgoDataPos = AddValueOutput("Pos", () => (GetWgoData() != null) ? GetWgoData().Position : Vector3.zero);
	}
}
