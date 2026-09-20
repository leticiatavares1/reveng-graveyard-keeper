using System.Collections;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Emulate Sword Attack", 0)]
[Category("Game/Fighting")]
[Description("Emulates sword attack animation on a WgoData target.")]
public class Flow_EmulateSwordAttack : GKCustomFlowNode
{
	public enum AttackAction
	{
		Once,
		StartLoop,
		StopLoop
	}

	[GatherPortsCallback]
	public AttackAction action;

	private ValueInput<WgoData> targetWgo;

	private ValueInput<float> delay;

	private FlowOutput onFinished;

	public override string name => $"Emulate Sword Attack ({action})";

	protected override void RegisterPorts()
	{
		targetWgo = AddValueInput<WgoData>("Target Wgo");
		if (action != AttackAction.StopLoop)
		{
			delay = AddValueInput<float>("Delay Between Attacks");
		}
		onFinished = AddFlowOutput("On Finished");
		AddFlowInput("In", Execute);
	}

	private void Execute(Flow f)
	{
		WgoData value = targetWgo.value;
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
		SwordAttackEmulationComponent swordAttackEmulationComponent = wgoViewGlobal.GetComponentInChildren<SwordAttackEmulationComponent>();
		if (swordAttackEmulationComponent == null)
		{
			swordAttackEmulationComponent = wgoViewGlobal.gameObject.AddComponent<SwordAttackEmulationComponent>();
			AttackComponent componentInChildren = wgoViewGlobal.GetComponentInChildren<AttackComponent>();
			AnimationComponent componentInChildren2 = wgoViewGlobal.GetComponentInChildren<AnimationComponent>();
			swordAttackEmulationComponent.Init(componentInChildren, componentInChildren2);
		}
		switch (action)
		{
		case AttackAction.StopLoop:
			swordAttackEmulationComponent.StopLoop();
			f.Call(onFinished);
			break;
		case AttackAction.StartLoop:
			swordAttackEmulationComponent.StartLoop(delay.value);
			f.Call(onFinished);
			break;
		case AttackAction.Once:
			swordAttackEmulationComponent.PerformAttack(delegate
			{
				if (delay.value > 0f)
				{
					StartCoroutine(DelayRoutine(delay.value, f));
				}
				else
				{
					f.Call(onFinished);
				}
			});
			break;
		}
	}

	private IEnumerator DelayRoutine(float time, Flow f)
	{
		yield return new WaitForSeconds(time);
		f.Call(onFinished);
	}
}
