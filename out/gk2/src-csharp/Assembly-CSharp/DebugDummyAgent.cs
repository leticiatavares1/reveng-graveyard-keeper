using System;
using UnityEngine;

public class DebugDummyAgent : MonoBehaviour, ICombatEntity
{
	[SerializeField]
	private SGuid combatEntityUid = new SGuid();

	[SerializeField]
	private LazyConsts.Fighting.TeamType teamType;

	[SerializeField]
	[Min(1f)]
	private int maxHpValue = 10;

	[SerializeField]
	[Min(0f)]
	private int startHpValue = 10;

	[SerializeField]
	[Min(0f)]
	private int combatEntityQuality = 1;

	[SerializeField]
	private int attackPriority = 20;

	[SerializeField]
	private bool isActiveCombatant = true;

	[SerializeField]
	private bool ensurePhysicsComponents = true;

	private HPComponent hpComponent;

	private FightingGameController ownerController;

	public SGuid CombatEntityUID => combatEntityUid;

	public LazyConsts.Fighting.TeamType TeamType => teamType;

	public LazyConsts.Fighting.EntityType EntityType => LazyConsts.Fighting.EntityType.None;

	public Vector3 CombatEntityPosition => base.transform.position;

	public int ArmorValue { get; }

	public HPComponent CombatEntityHpComponent => hpComponent;

	public int CombatEntityQuality => combatEntityQuality;

	public int AttackPriority
	{
		get
		{
			return attackPriority;
		}
		set
		{
			attackPriority = value;
		}
	}

	public bool HasAnyDockPoint => false;

	public bool IsActiveCombatant
	{
		get
		{
			return isActiveCombatant;
		}
		set
		{
			isActiveCombatant = value;
		}
	}

	public float GetCombatEntityDistance(Vector3 from, LazyConsts.Fighting.TeamType teamType)
	{
		return (from - CombatEntityPosition).magnitude;
	}

	private void Awake()
	{
		RefreshHpComponent();
		if (ensurePhysicsComponents)
		{
			EnsurePhysicsSetup();
		}
	}

	private void Reset()
	{
		startHpValue = Mathf.Clamp(startHpValue, 0, maxHpValue);
	}

	private void OnDestroy()
	{
		ownerController?.OnDebugDummyDestroyed(this);
	}

	public void Configure(LazyConsts.Fighting.TeamType newTeam, int newMaxHp, int newStartHp, int newQuality, int newPriority, bool newIsFightingMember)
	{
		teamType = newTeam;
		maxHpValue = Mathf.Max(1, newMaxHp);
		startHpValue = Mathf.Clamp(newStartHp, 0, maxHpValue);
		combatEntityQuality = Math.Max(0, newQuality);
		attackPriority = newPriority;
		isActiveCombatant = newIsFightingMember;
		combatEntityUid = new SGuid();
		RefreshHpComponent();
	}

	public void AssignOwner(FightingGameController controller)
	{
		ownerController = controller;
	}

	public void EnsureInitialized()
	{
		if (hpComponent == null)
		{
			RefreshHpComponent();
		}
	}

	public void OnOtherCombatTargetReachedToMe(ICombatEntity other)
	{
	}

	public void OnOtherCombatTargetHitMe(AttackContext ctx)
	{
	}

	public float GetCombatEntityGameRes(string resId)
	{
		return 0f;
	}

	private void RefreshHpComponent()
	{
		startHpValue = Mathf.Clamp(startHpValue, 0, maxHpValue);
		hpComponent = new HPComponent(maxHpValue, startHpValue);
		hpComponent.Init();
	}

	private void EnsurePhysicsSetup()
	{
		if (!TryGetComponent<Collider>(out var _))
		{
			CapsuleCollider capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
			capsuleCollider.height = 1.8f;
			capsuleCollider.radius = 0.35f;
			capsuleCollider.center = Vector3.up * capsuleCollider.height * 0.5f;
		}
		if (!TryGetComponent<Rigidbody>(out var component2))
		{
			component2 = base.gameObject.AddComponent<Rigidbody>();
			component2.isKinematic = true;
			component2.useGravity = false;
		}
	}
}
