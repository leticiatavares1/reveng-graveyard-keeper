using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Emulate Bow Attack", 0)]
[Category("Game/Fighting")]
[Description("Emulates bow attack animation on a WgoData target using random targets list.")]
public class Flow_EmulateBowAttack : GKCustomFlowNode
{
	public enum AttackAction
	{
		Once,
		StartLoop,
		StopLoop
	}

	[GatherPortsCallback]
	public AttackAction action;

	private ValueInput<WgoData> attackerWgo;

	private ValueInput<List<WgoData>> targets;

	private ValueInput<float> delay;

	private FlowOutput onFinished;

	public override string name => $"Emulate Bow Attack ({action})";

	protected override void RegisterPorts()
	{
		attackerWgo = AddValueInput<WgoData>("Attacker Wgo");
		targets = AddValueInput<List<WgoData>>("Targets");
		if (action != AttackAction.StopLoop)
		{
			delay = AddValueInput<float>("Delay Between Attacks");
		}
		onFinished = AddFlowOutput("On Finished");
		AddFlowInput("In", Execute);
	}

	private void Execute(Flow f)
	{
		WgoData value = attackerWgo.value;
		if (value == null)
		{
			f.Call(onFinished);
			return;
		}
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(value.UniqueId);
		if (wgoViewGlobal == null)
		{
			f.Call(onFinished);
			return;
		}
		BowAttackEmulationComponent bowAttackEmulationComponent = wgoViewGlobal.GetComponentInChildren<BowAttackEmulationComponent>();
		if (bowAttackEmulationComponent == null)
		{
			bowAttackEmulationComponent = wgoViewGlobal.gameObject.AddComponent<BowAttackEmulationComponent>();
		}
		AttackComponent componentInChildren = wgoViewGlobal.GetComponentInChildren<AttackComponent>();
		AnimationComponent componentInChildren2 = wgoViewGlobal.GetComponentInChildren<AnimationComponent>();
		if (componentInChildren == null || componentInChildren2 == null)
		{
			Debug.LogError("AttackComponent or AnimationComponent not found");
			return;
		}
		bowAttackEmulationComponent.Init(componentInChildren, componentInChildren2);
		bowAttackEmulationComponent.SetTargets(targets.value);
		switch (action)
		{
		case AttackAction.StopLoop:
			bowAttackEmulationComponent.StopLoop();
			f.Call(onFinished);
			break;
		case AttackAction.StartLoop:
			bowAttackEmulationComponent.StartLoop(delay.value);
			f.Call(onFinished);
			break;
		case AttackAction.Once:
			bowAttackEmulationComponent.PerformAttack(delay.value, delegate
			{
				f.Call(onFinished);
			});
			break;
		}
	}
}
