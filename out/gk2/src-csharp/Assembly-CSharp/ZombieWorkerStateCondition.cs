using System;
using UnityEngine;

[Serializable]
public class ZombieWorkerStateCondition : ConditionalDrawerConditionBase
{
	[Tooltip("Zombie type whose state is checked")]
	public ZombieType zombieType = ZombieType.Caretaker;

	[Tooltip("Expected state of the selected zombie type")]
	public int conditionType;

	[Tooltip("Inverts the result: true when the zombie is NOT in the expected state")]
	public bool invert;

	public override ConditionalEventType EventType => ConditionalEventType.ZombieWorkerStateChanged;

	public static Type GetStateEnumType(ZombieType zombieType)
	{
		return zombieType switch
		{
			ZombieType.Caretaker => typeof(ZombieWgoData.ZombieCaretakerState), 
			ZombieType.Gardener => typeof(ZombieWgoData.ZombieGardenerState), 
			ZombieType.ConveyorTransporter => typeof(ZombieWgoData.ZombieConveyorTransporterState), 
			_ => null, 
		};
	}

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context) && context.ZombieWgoData != null)
		{
			return context.ZombieWgoData.ZombieType == zombieType;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		ZombieWgoData zombieWgoData = context.ZombieWgoData;
		if (zombieWgoData == null || zombieWgoData.ZombieType != zombieType || !TryGetCurrentState(zombieWgoData, out var state))
		{
			return false;
		}
		bool flag = state == conditionType;
		if (!invert)
		{
			return flag;
		}
		return !flag;
	}

	private bool TryGetCurrentState(ZombieWgoData zombieWgoData, out int state)
	{
		switch (zombieType)
		{
		case ZombieType.Caretaker:
			state = (int)zombieWgoData.CaretakerState;
			return true;
		case ZombieType.Gardener:
			state = (int)zombieWgoData.GardenerState;
			return true;
		case ZombieType.ConveyorTransporter:
			state = (int)zombieWgoData.ConveyorTransporterState;
			return true;
		default:
			state = 0;
			return false;
		}
	}
}
