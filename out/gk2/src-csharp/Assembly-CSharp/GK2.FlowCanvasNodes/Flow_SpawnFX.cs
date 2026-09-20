using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Spawn FX", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SpawnFX : GKCustomFlowNode
{
	public Vector3 size = Vector3.one;

	protected FlowInput @in;

	protected FlowOutput @out;

	protected ValueInput<string> fxName;

	protected ValueInput<Vector3> worldPos;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SpawnFX);
		@out = AddFlowOutput("out".CapitalizeFirst());
		fxName = AddValueInput<string>("fxName");
		worldPos = AddValueInput<Vector3>("worldPos");
	}

	private void SpawnFX(Flow flow)
	{
		WorldFX.Spawn(worldPos.value, fxName.value, null, size);
		@out.Call(flow);
	}
}
