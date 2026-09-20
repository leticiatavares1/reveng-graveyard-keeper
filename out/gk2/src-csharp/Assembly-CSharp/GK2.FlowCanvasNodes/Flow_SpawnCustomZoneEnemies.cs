using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Spawn Custom Zone Enemies", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_SpawnCustomZoneEnemies : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> spawnerName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), Spawn);
		@out = AddFlowOutput("out".CapitalizeFirst());
		spawnerName = AddValueInput<string>("spawnerName");
	}

	private void Spawn(Flow flow)
	{
		LazySingleton<FightingGameController>.Instance.ActivateCustomSpawner(spawnerName.value);
		@out.Call(flow);
	}
}
