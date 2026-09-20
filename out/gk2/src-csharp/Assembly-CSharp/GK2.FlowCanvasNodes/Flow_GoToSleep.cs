using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Go To Sleep", 0)]
[Category("Game/Environment")]
public class Flow_GoToSleep : GKCustomFlowNode
{
	private const string BED_UPGRADE_GAME_RES = "bed_upgrade";

	[GatherPortsCallback]
	public bool sleepWithoutSavingGame;

	[GatherPortsCallback]
	public bool sleepWithMaxEnergy;

	[GatherPortsCallback]
	[ShowIf("IsWgoDataConnected", 0)]
	public SleepAnimType animType;

	[GatherPortsCallback]
	public float sleepDuration = 1.5f;

	private FlowInput @in;

	private FlowOutput @out;

	private FlowOutput onFinished;

	private FlowOutput onFinishedIfNoNeedToSleep;

	private ValueInput<WgoData> wgoData;

	public bool IsWgoDataConnected
	{
		get
		{
			if (wgoData != null)
			{
				return wgoData.isConnected;
			}
			return false;
		}
	}

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), GoToSleep);
		@out = AddFlowOutput("out".CapitalizeFirst());
		onFinished = AddFlowOutput("onFinished".CapitalizeFirst());
		if (!sleepWithMaxEnergy)
		{
			onFinishedIfNoNeedToSleep = AddFlowOutput("onFinishedIfNoNeedToSleep".CapitalizeFirst());
		}
		wgoData = AddValueInput<WgoData>("WgoData");
	}

	private void GoToSleep(Flow flow)
	{
		MainGame.PlayerData.energySystem.StartSleeping(delegate
		{
			onFinished.Call(flow);
		}, delegate
		{
			onFinishedIfNoNeedToSleep.Call(flow);
		}, sleepWithoutSavingGame, sleepWithMaxEnergy, ResolveAnimType(), sleepDuration);
		@out.Call(flow);
	}

	private SleepAnimType ResolveAnimType()
	{
		if (!IsWgoDataConnected)
		{
			return animType;
		}
		WgoData value = wgoData.value;
		if (value == null)
		{
			Debug.LogError("Flow_GoToSleep: WgoData is connected but null");
			return animType;
		}
		if (value.GetGameResInt("bed_upgrade") < 1)
		{
			return SleepAnimType.StrawBed;
		}
		return SleepAnimType.RegularBed;
	}
}
