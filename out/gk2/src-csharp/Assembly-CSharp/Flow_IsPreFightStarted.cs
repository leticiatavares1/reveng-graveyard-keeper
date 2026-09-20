using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("Is Pre Fight Started", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_IsPreFightStarted : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput trueOut;

	private FlowOutput falseOut;

	private ValueInput<string> levelName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), IsPreFightingStarted);
		trueOut = AddFlowOutput("<color=green>✔</color>");
		falseOut = AddFlowOutput("<color=red>✘</color>");
	}

	private void IsPreFightingStarted(Flow flow)
	{
		if (LazySingleton<FightingGameController>.Instance.CurrentFightState == FightState.InPreFight)
		{
			trueOut.Call(flow);
		}
		else
		{
			falseOut.Call(flow);
		}
	}
}
