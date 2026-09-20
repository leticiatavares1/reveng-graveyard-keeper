using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Fighting State", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_SetFightingState : GKCustomFlowNode
{
	public enum State
	{
		Start,
		Pause,
		Continue,
		Stop
	}

	[GatherPortsCallback]
	public State state;

	[GatherPortsCallback]
	public bool customFinishTransition;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput customOut;

	private ValueInput<string> levelId;

	public override string name => state switch
	{
		State.Start => "Start Fighting", 
		State.Pause => "Pause Fighting", 
		State.Continue => "Continue Fighting", 
		State.Stop => "Stop Fighting", 
		_ => "Set Fighting State", 
	};

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetFightingPlayState);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (this.state == State.Start)
		{
			levelId = AddValueInput<string>("levelId");
		}
		if (customFinishTransition)
		{
			State state = this.state;
			if (state == State.Start || state == State.Stop)
			{
				customOut = AddFlowOutput("customOut".CapitalizeFirst());
			}
		}
	}

	private void SetFightingPlayState(Flow flow)
	{
		switch (this.state)
		{
		case State.Start:
			LazySingleton<FightingGameController>.Instance.Play(levelId.value);
			break;
		case State.Pause:
			LazySingleton<FightingGameController>.Instance.SetPauseState(isPaused: true);
			break;
		case State.Continue:
			LazySingleton<FightingGameController>.Instance.SetPauseState(isPaused: false);
			break;
		case State.Stop:
			LazySingleton<FightingGameController>.Instance.Stop(customFinishTransition);
			break;
		}
		if (customFinishTransition)
		{
			State state = this.state;
			if (state == State.Start || state == State.Stop)
			{
				LazySingleton<FightingGameController>.Instance.SetCustomFinishCallback(delegate
				{
					customOut?.Call(flow);
				});
			}
		}
		@out.Call(flow);
	}
}
