using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Enable Pre Fight", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_EnablePreFight : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> levelName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetFightingLevelName);
		@out = AddFlowOutput("out".CapitalizeFirst());
		levelName = AddValueInput<string>("levelName");
	}

	private void SetFightingLevelName(Flow flow)
	{
		LazySingleton<FightingGameController>.Instance.StartPreFight(levelName.value);
		@out.Call(flow);
	}
}
