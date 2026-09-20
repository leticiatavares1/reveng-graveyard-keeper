using System.Collections.Generic;
using JetBrains.Annotations;
using LinqTools;
using UnityEngine;

public class SpearDestinationModifier : GoToDestinationModifier
{
	private struct SideWithOwner
	{
		public Direction Direction { get; private set; }

		public SGuid Owner { get; set; }

		public SideWithOwner(Direction direction, SGuid owner)
		{
			Direction = direction;
			Owner = owner;
		}
	}

	private const float PIKE_ATTACK_OFFSET = 1f;

	private Vector3 selectedAttackPositionLocal;

	private Vector3 targetPositionWhenSelected;

	private bool positionSelected;

	private static Dictionary<SGuid, List<SideWithOwner>> attachedEntities = new Dictionary<SGuid, List<SideWithOwner>>();

	public override float CustomDestinationOffset => 0.1f;

	private float PikeAttackOffset => ((float?)base.Agent?.FighterDef.atkRange.EvaluateInt(base.Wgo) * 0.7f) ?? 1f;

	private static List<SideWithOwner> CreateEmptySides()
	{
		return new List<SideWithOwner>
		{
			new SideWithOwner(Direction.Right, SGuid.Empty),
			new SideWithOwner(Direction.Up, SGuid.Empty),
			new SideWithOwner(Direction.Left, SGuid.Empty),
			new SideWithOwner(Direction.Down, SGuid.Empty)
		};
	}

	public override void OnStart()
	{
		base.OnStart();
		positionSelected = false;
		SelectBestAttackPosition();
	}

	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		if ((base.Agent.MobCommand.Position - targetPositionWhenSelected).XZ().magnitude > PikeAttackOffset)
		{
			positionSelected = false;
			SelectBestAttackPosition();
		}
	}

	public override bool TryGetTargetPositionForPathfinding(out ICombatEntity combatEntity, out Vector3 position)
	{
		combatEntity = null;
		position = GetCurrentTargetPosition();
		return true;
	}

	public override Vector3 GetCurrentTargetPosition()
	{
		if (!positionSelected)
		{
			SelectBestAttackPosition();
		}
		return base.Agent.MobCommand.Position + selectedAttackPositionLocal;
	}

	public override void OnReachedDestination()
	{
		base.OnReachedDestination();
		TryFreeOwnedSide(base.Agent.Wgo);
		base.Agent.MobCommand.TargetEntity?.OnOtherCombatTargetReachedToMe(base.Wgo);
	}

	public override void OnFinish()
	{
		base.OnFinish();
		TryFreeOwnedSide(base.Agent.Wgo);
	}

	private void TryFreeOwnedSide(ICombatEntity combatEntity)
	{
		if (base.Agent.MobCommand.TargetEntity == null || !attachedEntities.TryGetValue(base.Agent.MobCommand.TargetEntity.CombatEntityUID, out var value))
		{
			return;
		}
		for (int i = 0; i < value.Count; i++)
		{
			SideWithOwner value2 = value[i];
			if (value2.Owner == combatEntity.CombatEntityUID)
			{
				value2.Owner = SGuid.Empty;
				value[i] = value2;
				break;
			}
		}
		if (value.All((SideWithOwner x) => x.Owner == SGuid.Empty))
		{
			attachedEntities.Remove(base.Agent.MobCommand.TargetEntity.CombatEntityUID);
		}
	}

	private void SetNewOwner(List<SideWithOwner> list, SGuid owner, int idx)
	{
		SideWithOwner value = list[idx];
		value.Owner = owner;
		list[idx] = value;
	}

	[CanBeNull]
	private List<SideWithOwner> GetOrCreateListWithOwners()
	{
		if (base.Agent.MobCommand.TargetEntity == null)
		{
			return null;
		}
		if (attachedEntities.TryGetValue(base.Agent.MobCommand.TargetEntity.CombatEntityUID, out var value))
		{
			return value;
		}
		return attachedEntities[base.Agent.MobCommand.TargetEntity.CombatEntityUID] = CreateEmptySides();
	}

	private void SelectBestAttackPosition()
	{
		Vector3 position = base.Agent.MobCommand.Position;
		Vector3 position2 = base.Wgo.Data.Position;
		TryFreeOwnedSide(base.Agent.Wgo);
		float pikeAttackOffset = PikeAttackOffset;
		List<Vector3> list = new List<Vector3>
		{
			position + Vector3.right * pikeAttackOffset,
			position + Vector3.forward * pikeAttackOffset,
			position + Vector3.left * pikeAttackOffset,
			position + Vector3.back * pikeAttackOffset
		};
		float num = float.MaxValue;
		List<SideWithOwner> orCreateListWithOwners = GetOrCreateListWithOwners();
		bool flag = base.Agent.MobCommand.TargetEntity != null && (orCreateListWithOwners?.Any((SideWithOwner x) => x.Owner == SGuid.Empty) ?? false);
		Vector3 vector = position;
		int num2 = -1;
		for (int i = 0; i < list.Count; i++)
		{
			Vector3 vector2 = list[i];
			SideWithOwner sideWithOwner = orCreateListWithOwners?[i] ?? default(SideWithOwner);
			if (!flag || !(sideWithOwner.Owner != SGuid.Empty))
			{
				float magnitude = (vector2 - position2).XZ().magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					vector = vector2;
					num2 = i;
				}
			}
		}
		if (num2 >= 0)
		{
			SetNewOwner(orCreateListWithOwners, base.Agent.Wgo.CombatEntityUID, num2);
		}
		selectedAttackPositionLocal = vector - position;
		targetPositionWhenSelected = position;
		positionSelected = true;
	}

	public override GoToDestinationModifierDebugInfo GetDebugInfo()
	{
		GoToDestinationModifierDebugInfo debugInfo = base.GetDebugInfo();
		debugInfo.SpearPositionSelected = positionSelected;
		debugInfo.SpearAttackOffsetLocal = selectedAttackPositionLocal;
		debugInfo.SpearTargetPositionWhenSelected = targetPositionWhenSelected;
		return debugInfo;
	}
}
