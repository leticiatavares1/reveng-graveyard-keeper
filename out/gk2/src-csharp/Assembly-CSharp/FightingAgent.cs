using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using JetBrains.Annotations;
using LazyBearTechnology;
using LinqTools;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FightingAgent : MonoBehaviour, IWgoCustomComponent<WCCD_ZombieAIAgent>
{
	[SerializeField]
	private Wgo wgo;

	[SerializeField]
	private FighterDef fighterDef;

	[SerializeField]
	private bool isExecutingCommand;

	[SerializeField]
	private FightingAgentSettings settings;

	[Space]
	[SerializeField]
	[CanBeNull]
	private Weapon weapon;

	[SerializeField]
	private AttackComponent attackComponent;

	private MobCommand.CommandType attackCommandType = MobCommand.CommandType.ZombieMeleeAttack;

	[SerializeField]
	private AssetReferenceT<AgentAI> aiRef;

	[SerializeField]
	private bool autoBakeAsset;

	[SerializeField]
	private AgentAI editorAI;

	[SerializeField]
	[CanBeNull]
	private NavmeshCut navmeshCut;

	[Header("Death")]
	private float deathDuration = 1f;

	private float deathElapsedTime;

	private bool isDyingNow;

	public string customDeathEffectId = "";

	public List<string> deathEffects = new List<string>();

	private AnimationCurve forceFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private float knockDuration = 0.5f;

	private Coroutine knockCoroutine;

	private bool isKnockbackActive;

	private RichAI_Custom richAI;

	private Seeker seeker;

	private RVOController rvoController;

	private MobCommand mobCommand;

	private MobCommand previousCommand;

	private AnimationEventReceiver animationEventReceiver;

	private AnimationEventReceiver subscribedAnimationEventReceiver;

	private WgoMovementAdjustComponent movementAdjustComponent;

	private DecoyComponent decoyComponent;

	private AgentAI agentAI;

	private AgentAI overrideAgentAI;

	private AsyncOperationHandle<AgentAI> agentAIHandle;

	[CanBeNull]
	private DamageEffectComponent damageEffectComponent;

	private AgentsGroupBehaviourController parentController;

	private bool wasAnimSubscribed;

	private bool isValid;

	private float mainHeroPushActiveUntil;

	private float? rvoPriorityBeforeMainHeroPush;

	private float movementPauseStartedAt = -1f;

	private bool? rvoLockedBeforeMovementPause;

	private bool hasLockedFacing;

	private float lockedFacingAngle;

	private const float FACING_HYSTERESIS_DEGREES = 12f;

	private const float ALLIED_WGO_RICHAI_SYNC_THRESHOLD_SQR = 0.0001f;

	public Wgo Wgo => wgo;

	public RichAI_Custom RichAI => richAI;

	public MobCommand MobCommand => mobCommand;

	public MobCommand PreviousCommand => previousCommand;

	public bool IsExecutingCommand => isExecutingCommand;

	public AgentsGroupBehaviourController ParentController
	{
		get
		{
			return parentController;
		}
		set
		{
			parentController = value;
		}
	}

	public AgentsGroupFlagController FlagController { get; set; }

	public AttackComponent AttackComponent => attackComponent;

	public MobCommand.CommandType AttackCommandType => attackCommandType;

	public ICombatEntity LastEntity { get; set; }

	public DecoyComponent DecoyComponent => decoyComponent;

	public Weapon Weapon
	{
		get
		{
			return weapon;
		}
		set
		{
			weapon = value;
		}
	}

	public AgentAI AgentAI => overrideAgentAI ?? agentAI;

	public GoToDestinationModifier GoToDestinationModifier
	{
		get
		{
			if (!weapon || weapon.ItemDef.type != ItemType.Pike)
			{
				return null;
			}
			return new SpearDestinationModifier();
		}
	}

	public DamageEffectComponent DamageEffectComponent => damageEffectComponent;

	public FightingAgentSettings Settings => settings;

	public FighterDef FighterDef => fighterDef;

	public bool IsValid => isValid;

	public bool IsUnderMainHeroPush => mainHeroPushActiveUntil > Time.time;

	public bool IsAnchoredAtDockPoint { get; set; }

	public bool IsPushableByMainHero
	{
		get
		{
			if (wgo != null && wgo.TeamType == LazyConsts.Fighting.TeamType.Player)
			{
				return !IsAnchoredAtDockPoint;
			}
			return false;
		}
	}

	private bool IsMovable => (wgo?.Data?.Definition?.isMovable).GetValueOrDefault();

	public float RVO_Priority
	{
		get
		{
			return rvoController.priority;
		}
		set
		{
			rvoController.priority = value;
		}
	}

	public bool RVO_Locked
	{
		get
		{
			return rvoController.locked;
		}
		set
		{
			rvoController.locked = value;
		}
	}

	public Vector3 RVO_Velocity
	{
		get
		{
			return rvoController.velocity;
		}
		set
		{
			rvoController.velocity = value;
		}
	}

	public bool RVO_Enabled
	{
		get
		{
			if ((UnityEngine.Object)(object)rvoController != null)
			{
				return ((Behaviour)(object)rvoController).enabled;
			}
			return false;
		}
		set
		{
			((Behaviour)(object)rvoController).enabled = value;
		}
	}

	public bool RVO_AvoidingAnyAgents
	{
		get
		{
			if ((UnityEngine.Object)(object)rvoController != null)
			{
				return rvoController.AvoidingAnyAgents;
			}
			return false;
		}
	}

	public float RVO_CalculatedSpeed
	{
		get
		{
			if (rvoController?.rvoAgent == null)
			{
				return 0f;
			}
			return rvoController.rvoAgent.CalculatedSpeed;
		}
	}

	public int RVO_NeighbourCount
	{
		get
		{
			if (rvoController?.rvoAgent == null)
			{
				return 0;
			}
			return rvoController.rvoAgent.NeighbourCount;
		}
	}

	public event Action<bool> OnInitialized;

	public event Action<FightingAgent, MobCommand> OnCommandCompleted;

	public void SetFacingDirection(Vector2 direction, bool instant = false)
	{
		if (wgo?.Data == null || direction.sqrMagnitude < 0.0001f)
		{
			return;
		}
		float num = BasicNpcSteppedRotationPreset.Instance.ComputeAngle(direction);
		if (!instant && hasLockedFacing)
		{
			float current = Vector2.SignedAngle(Vector2.right, direction);
			float num2 = Mathf.Abs(Mathf.DeltaAngle(current, lockedFacingAngle));
			if (Mathf.Abs(Mathf.DeltaAngle(current, num)) + 12f >= num2)
			{
				return;
			}
		}
		hasLockedFacing = true;
		lockedFacingAngle = num;
		wgo.Data.direction.Value = new Vector2(Mathf.Cos(num * (MathF.PI / 180f)), Mathf.Sin(num * (MathF.PI / 180f)));
	}

	private void ResetFacingHysteresis()
	{
		hasLockedFacing = false;
		lockedFacingAngle = 0f;
	}

	public void Init(Wgo wgo, AgentsGroupBehaviourController parentController)
	{
		StopCommandExecution();
		previousCommand = null;
		isDyingNow = false;
		deathElapsedTime = 0f;
		LastEntity = null;
		IsAnchoredAtDockPoint = false;
		SetNavmeshCutActive(active: false);
		ResetMainHeroPushState();
		movementPauseStartedAt = -1f;
		rvoLockedBeforeMovementPause = null;
		ResetFacingHysteresis();
		if ((bool)attackComponent)
		{
			AttackComponent obj = attackComponent;
			obj.OnWeaponChanged = (Action)Delegate.Remove(obj.OnWeaponChanged, new Action(UpdateArmorLayers));
		}
		if (subscribedAnimationEventReceiver != null)
		{
			subscribedAnimationEventReceiver.onEvent1.RemoveListener(OnAnimationTriggerAttack);
			subscribedAnimationEventReceiver = null;
			wasAnimSubscribed = false;
		}
		this.wgo = wgo;
		this.parentController = parentController;
		attackComponent = GetComponentInChildren<AttackComponent>();
		if (!settings)
		{
			settings = LazySingletonSO<GlobalResources>.Instance.fighting.defaultFightingAgentSettings;
		}
		fighterDef = GameBalance.Me.GetData<FighterDef>(wgo.Id);
		if (fighterDef == null)
		{
			Debug.LogError("[ZombieAgent] FighterDef not found for wgo: " + wgo.Id + ".");
			isValid = false;
			this.OnInitialized?.Invoke(isValid);
			return;
		}
		isValid = true;
		this.OnInitialized?.Invoke(isValid);
		if ((bool)attackComponent)
		{
			attackComponent.Init(wgo, fighterDef);
			AttackComponent obj2 = attackComponent;
			obj2.OnWeaponChanged = (Action)Delegate.Combine(obj2.OnWeaponChanged, new Action(UpdateArmorLayers));
			attackComponent.animationComponent = GetComponentInChildren<AnimationComponent>();
			attackComponent.teamType = wgo.TeamType;
			if (attackComponent.teamType == LazyConsts.Fighting.TeamType.Player)
			{
				attackComponent.animationComponent.OnDeathAnimFinished += HandlePlayerDeathAnimFinished;
			}
			if ((bool)weapon)
			{
				attackComponent.EquipWeapon(weapon);
			}
		}
		if (!wgo.TryGetComponent<RichAI_Custom>(out richAI))
		{
			richAI = wgo.gameObject.AddComponent<RichAI_Custom>();
		}
		if (!wgo.TryGetComponent<Seeker>(out seeker))
		{
			seeker = wgo.gameObject.AddComponent<Seeker>();
		}
		if (!wgo.TryGetComponent<RVOController>(out rvoController))
		{
			rvoController = wgo.gameObject.AddComponent<RVOController>();
		}
		animationEventReceiver = wgo.MainWgoPart.GetComponentInChildren<AnimationEventReceiver>();
		if (wgo.MainWgoPart.TryGetComponent<TriggerColliderComponentLinker>(out var component))
		{
			component.Component = wgo;
		}
		int num = fighterDef.hp.EvaluateInt(wgo);
		if (num > 0)
		{
			wgo.Data.HpComponent.SetCustomHpValue(num);
		}
		if (wgo.TryGetComponent<WgoMovementAdjustComponent>(out movementAdjustComponent))
		{
			movementAdjustComponent.SetAdjustmentActive(active: false);
			wgo.UpdatePosByData = false;
		}
		if (TryGetComponent<DecoyComponent>(out decoyComponent))
		{
			decoyComponent.Initialize(wgo);
		}
		wgo.Data.SetCustomDeathMoment();
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePos);
		CinemachineCore.CameraUpdatedEvent.AddListener(UpdatePos);
		ConfigureAgent();
		if (!wasAnimSubscribed && animationEventReceiver != null)
		{
			wasAnimSubscribed = true;
			animationEventReceiver.onEvent1.AddListener(OnAnimationTriggerAttack);
			subscribedAnimationEventReceiver = animationEventReceiver;
		}
	}

	public void DeInitForPool()
	{
		if (parentController != null)
		{
			parentController.RemoveAgent(this);
			parentController = null;
		}
		if (isExecutingCommand)
		{
			StopCommandExecution();
		}
		if (knockCoroutine != null)
		{
			StopCoroutine(knockCoroutine);
			knockCoroutine = null;
		}
		if ((bool)attackComponent)
		{
			AttackComponent obj = attackComponent;
			obj.OnWeaponChanged = (Action)Delegate.Remove(obj.OnWeaponChanged, new Action(UpdateArmorLayers));
			if ((bool)attackComponent.animationComponent && attackComponent.teamType == LazyConsts.Fighting.TeamType.Player)
			{
				attackComponent.animationComponent.OnDeathAnimFinished -= HandlePlayerDeathAnimFinished;
			}
		}
		if (wasAnimSubscribed && animationEventReceiver != null)
		{
			animationEventReceiver.onEvent1.RemoveListener(OnAnimationTriggerAttack);
		}
		wasAnimSubscribed = false;
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePos);
		mobCommand = null;
		previousCommand = null;
		isExecutingCommand = false;
		isKnockbackActive = false;
		isDyingNow = false;
		IsAnchoredAtDockPoint = false;
		SetNavmeshCutActive(active: false);
		ResetMainHeroPushState();
		movementPauseStartedAt = -1f;
		rvoLockedBeforeMovementPause = null;
		LastEntity = null;
		ResetFacingHysteresis();
		overrideAgentAI = null;
		FlagController = null;
		weapon = null;
		attackCommandType = MobCommand.CommandType.ZombieMeleeAttack;
		isValid = false;
	}

	private void HandlePlayerDeathAnimFinished()
	{
		if (!(wgo == null))
		{
			wgo.Data.TriggerCustomDeathMoment();
			wgo.UpdateFlag(ChunkingIgnoreType.Animation, newValue: false);
		}
	}

	public void SetPathPenalties(List<PathfindingPenalty> penalties)
	{
		if (penalties == null)
		{
			return;
		}
		foreach (PathfindingPenalty penalty in penalties)
		{
			if (penalty.isTraversable)
			{
				seeker.tagEntryCosts[penalty.tag.value] = penalty.penalty;
			}
			else
			{
				seeker.traversableTags &= ~(1 << (int)penalty.tag.value);
			}
		}
	}

	public void SetGraphMask(LazyConsts.Navigation.Graph graph)
	{
		if ((UnityEngine.Object)(object)seeker != null)
		{
			seeker.graphMask = GraphMask.FromGraphIndex((uint)graph);
		}
	}

	public void SetGraphMask(params LazyConsts.Navigation.Graph[] graphs)
	{
		if ((UnityEngine.Object)(object)seeker != null)
		{
			seeker.graphMask = NavigationGraphMaskUtils.ToGraphMask(graphs);
		}
	}

	public void SetAgentAI(AgentAI ai)
	{
		overrideAgentAI = ai;
	}

	public void UpdateArmorLayers()
	{
		AnimationComponent.Layers layer = AnimationComponent.Layers.Armor;
		if (attackComponent?.weapon != null)
		{
			layer = (attackComponent.IsRangedWeapon ? AnimationComponent.Layers.ArmorWithBow : AnimationComponent.Layers.ArmorWithPike);
		}
		attackComponent?.animationComponent.ResetArmorLayers();
		attackComponent?.animationComponent.SetLayerWeight(layer, 1f);
		attackComponent?.animationComponent.Animator.Update(0f);
	}

	public void SetCommand(MobCommand command)
	{
		if (!isDyingNow)
		{
			if (mobCommand != null)
			{
				previousCommand = mobCommand;
			}
			mobCommand = command;
			mobCommand.Init(this);
			StartCommandExecution();
		}
	}

	public void ClearCommand()
	{
		StopCommandExecution();
	}

	public void CustomUpdate(float deltaTime)
	{
		if (!isDyingNow)
		{
			CustomUpdateInner(deltaTime);
		}
	}

	private void CustomUpdateInner(float deltaTime)
	{
		if (isKnockbackActive)
		{
			wgo.Data.Position = richAI.position;
		}
		if ((UnityEngine.Object)(object)richAI != null && richAI.IsMovementPaused)
		{
			return;
		}
		if (isExecutingCommand && mobCommand != null)
		{
			mobCommand.OnUpdate(deltaTime);
			if (IsPushableByMainHero && IsMovable && !(mobCommand is MobCommandGoTo))
			{
				TickMainHeroPushState(pushedThisFrame: false);
				if (TryApplyRvoPushDisplacement(updateAnimation: false, deltaTime))
				{
					TickMainHeroPushState(pushedThisFrame: true);
					InterruptCommandDueToPlayerPush();
					wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Walk);
				}
				else if (richAI.simulateMovement)
				{
					SyncAlliedWgoPositionFromRichAI();
				}
			}
			return;
		}
		if (IsAnchoredAtDockPoint)
		{
			RVO_Locked = true;
			wgo.MainWgoPart.AnimationComponent?.SetState(AnimationState.Idle);
			return;
		}
		if (IsPushableByMainHero && IsMovable)
		{
			TickMainHeroPushState(pushedThisFrame: false);
			bool num = TryApplyRvoPushDisplacement(updateAnimation: true, deltaTime);
			if (num)
			{
				TickMainHeroPushState(pushedThisFrame: true);
			}
			else if (richAI.simulateMovement)
			{
				SyncAlliedWgoPositionFromRichAI();
			}
			if (num)
			{
				return;
			}
		}
		wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
	}

	public IEnumerator SetPositionWithDelayedMovementLock(Vector3 position)
	{
		RVO_Locked = true;
		TeleportToNavmesh(position, clearPath: true);
		yield return null;
		RVO_Locked = false;
	}

	public void TeleportToNavmesh(Vector3 position, bool clearPath = false)
	{
		richAI.Teleport(position, clearPath);
		wgo.Data.Position = richAI.position;
	}

	public void SetPosition(Vector3 position, bool stopCurrentCommand = true, bool snapToNavmesh = true)
	{
		if (stopCurrentCommand)
		{
			StopCommandExecution();
		}
		if (snapToNavmesh)
		{
			TeleportToNavmesh(position);
		}
		else
		{
			wgo.Data.Position = position;
		}
	}

	public void OnAnimationTriggerAttack()
	{
		if (MobCommand is ZombieMeleeAttackCommand zombieMeleeAttackCommand)
		{
			zombieMeleeAttackCommand.HandleAttackHit();
		}
	}

	public void StopCommandExecution(bool reportAlsoAsCompletion = false)
	{
		if (isExecutingCommand)
		{
			mobCommand.OnFinish();
			isExecutingCommand = false;
			LastEntity = mobCommand.TargetEntity;
			previousCommand = mobCommand;
			mobCommand = null;
			richAI.destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			if (reportAlsoAsCompletion)
			{
				this.OnCommandCompleted?.Invoke(this, previousCommand);
			}
		}
	}

	public void AssignWeapon(ItemDef weaponDef)
	{
		if (weaponDef == null)
		{
			weapon = null;
			attackCommandType = MobCommand.CommandType.None;
			return;
		}
		attackCommandType = MobCommand.CommandType.None;
		switch (weaponDef.type)
		{
		case ItemType.Bow:
			weapon = GetComponentInChildren<BowWeapon>(includeInactive: true);
			if ((bool)weapon)
			{
				attackCommandType = MobCommand.CommandType.ZombieBowAttack;
			}
			break;
		case ItemType.Pike:
			weapon = GetComponentsInChildren<Weapon>(includeInactive: true).First((Weapon c) => c.name.Contains("Pike"));
			if ((bool)weapon)
			{
				attackCommandType = MobCommand.CommandType.ZombiePikeAttack;
			}
			break;
		case ItemType.None:
			weapon = null;
			attackCommandType = MobCommand.CommandType.None;
			break;
		}
		if ((bool)weapon)
		{
			attackComponent.EquipWeapon(weapon);
		}
	}

	public void DoKnockback(Vector3 dir, float knockDuration)
	{
		if (knockCoroutine != null)
		{
			StopCoroutine(knockCoroutine);
		}
		knockCoroutine = StartCoroutine(KnockRoutine(dir, knockDuration));
	}

	public void PlayDying()
	{
		if (isDyingNow)
		{
			return;
		}
		isDyingNow = true;
		if ((bool)attackComponent && attackComponent.teamType == LazyConsts.Fighting.TeamType.Player)
		{
			wgo.UpdateFlag(ChunkingIgnoreType.Animation, newValue: true);
			attackComponent.animationComponent.SetState(AnimationState.Death);
			attackComponent.animationComponent.CancelBowAimLoop();
			return;
		}
		foreach (string deathEffect in deathEffects)
		{
			WorldFX.Spawn(base.transform.position, deathEffect);
		}
		if ((bool)damageEffectComponent && (bool)damageEffectComponent.Settings)
		{
			FightEffectsManager fightEffectsManager = LazySingleton<FightingGameController>.Instance.FightEffectsManager;
			if (damageEffectComponent.Settings.spawnBloodPaddle)
			{
				fightEffectsManager.SpawnBloodPaddle(wgo.Data.Position, Direction.None);
			}
			if (damageEffectComponent.Settings.spawnBonesDecals && UnityEngine.Random.value < LazySingletonSO<GlobalResources>.Instance.fighting.bonesDecalSpawnProbability)
			{
				fightEffectsManager.bonesDecalsCollection.SpawnDecal(base.transform.position, Direction.None, customDeathEffectId);
			}
			if (damageEffectComponent.Settings.spawnGutsDecals && UnityEngine.Random.value < LazySingletonSO<GlobalResources>.Instance.fighting.gutsDecalSpawnProbability)
			{
				fightEffectsManager.gutsDecalsCollection.SpawnDecal(base.transform.position, Direction.None);
			}
		}
		wgo.Data.HandleDeath();
		wgo.Data.TriggerCustomDeathMoment();
	}

	public WCCD_ZombieAIAgent OnSave()
	{
		return new WCCD_ZombieAIAgent
		{
			aiRef = aiRef
		};
	}

	public void OnLoad(WCCD_ZombieAIAgent data)
	{
		SetStrategyReference(data.aiRef);
	}

	public void OnUnload()
	{
	}

	private void SetStrategyReference(AssetReferenceT<AgentAI> newReference)
	{
		aiRef = newReference;
		agentAI = AddressableUtils.LoadAssetReferenceSync(aiRef, ref agentAIHandle);
	}

	public float RVO_GetMovementDeltaMagnitude(Vector3 position, float deltaTime)
	{
		if ((UnityEngine.Object)(object)rvoController == null || !((Behaviour)(object)rvoController).enabled || deltaTime <= 0f)
		{
			return 0f;
		}
		return rvoController.CalculateMovementDelta(position, deltaTime).XZ().magnitude;
	}

	public void RVO_SetPlayerAvoidance(FightingAgent agent, bool isAvoiding)
	{
		if (isAvoiding)
		{
			rvoController.collidesWith |= RVOLayer.Layer2;
		}
		else
		{
			rvoController.collidesWith &= (RVOLayer)(-5);
		}
	}

	public void RvoStopAt(Vector3 position)
	{
		if (!((UnityEngine.Object)(object)rvoController == null))
		{
			rvoController.SetTarget(position, 0f, 0f, position);
		}
	}

	public void ProvideReturnDamageLogic(AttackContext ctxHitMe)
	{
		int num = (fighterDef.retDamage.HasExpression ? fighterDef.retDamage.EvaluateInt() : 0);
		if (num == 0 || ctxHitMe.isReturnDamage)
		{
			return;
		}
		ItemType? itemType = ctxHitMe.weaponDef?.type;
		if (!itemType.HasValue || itemType.GetValueOrDefault() != ItemType.Spit)
		{
			Wgo attacker = wgo;
			LazyConsts.Fighting.TeamType teamType = wgo.TeamType;
			FighterDef obj = fighterDef;
			ItemDef weaponDef = weapon?.ItemDef;
			Vector3 position = wgo.Data.Position;
			Vector3 direction = ctxHitMe.hitPosition - wgo.Data.Position;
			int customDamage = ((num > 0) ? num : (-1));
			AttackContext ctx = new AttackContext(attacker, teamType, obj, weaponDef, position, direction, default(Vector3), customDamage, isReturnDamage: true);
			if (ctxHitMe.attacker is UnityEngine.Object @object && (bool)@object)
			{
				ctxHitMe.attackersAttackComponent?.DoHit(ctxHitMe.attacker, ctx);
			}
		}
	}

	public void AssignWeaponFromInventory()
	{
		Item itemByGroupId = wgo.Data.Inventory.GetItemByGroupId("weapon");
		if (itemByGroupId == null)
		{
			Debug.LogError("PreSet Fighter [" + wgo.Id + "] has no weapon in inventory", this);
			return;
		}
		ItemType type = itemByGroupId.Definition.type;
		if (type == ItemType.Bow || type == ItemType.Pike)
		{
			AssignWeapon(itemByGroupId.Definition);
		}
	}

	public void PauseMovement()
	{
		if (!((UnityEngine.Object)(object)richAI != null) || !richAI.IsMovementPaused)
		{
			richAI?.SetMovementPaused(paused: true);
			movementPauseStartedAt = Time.time;
			if ((UnityEngine.Object)(object)rvoController != null)
			{
				rvoLockedBeforeMovementPause = rvoController.locked;
				RVO_Locked = true;
				Vector3 position = (((UnityEngine.Object)(object)richAI != null) ? richAI.position : wgo.Data.Position);
				RvoStopAt(position);
			}
		}
	}

	public void UnpauseMovement()
	{
		if ((UnityEngine.Object)(object)richAI != null && !richAI.IsMovementPaused && !rvoLockedBeforeMovementPause.HasValue)
		{
			return;
		}
		float num = ((movementPauseStartedAt >= 0f) ? (Time.time - movementPauseStartedAt) : 0f);
		movementPauseStartedAt = -1f;
		if (rvoLockedBeforeMovementPause.HasValue)
		{
			RVO_Locked = rvoLockedBeforeMovementPause.Value;
			rvoLockedBeforeMovementPause = null;
		}
		richAI?.SetMovementPaused(paused: false);
		if (!(num <= 0f))
		{
			if (mainHeroPushActiveUntil > 0f)
			{
				mainHeroPushActiveUntil += num;
			}
			mobCommand?.CompensatePause(num);
		}
	}

	public void SetNavmeshCutActive(bool active)
	{
		if ((bool)(UnityEngine.Object)(object)navmeshCut)
		{
			((Behaviour)(object)navmeshCut).enabled = active;
			navmeshCut.ForceUpdate();
		}
	}

	private void StartCommandExecution()
	{
		if (isExecutingCommand)
		{
			StopCommandExecution();
		}
		MobCommand mobCommand = this.mobCommand;
		if (mobCommand != null)
		{
			isExecutingCommand = true;
			mobCommand.OnStart();
		}
	}

	private IEnumerator KnockRoutine(Vector3 totalDisplacement, float knockDuration)
	{
		isKnockbackActive = true;
		float t = 0f;
		while (t < knockDuration)
		{
			if (!MainGame.IsGamePaused)
			{
				float deltaTime = Time.deltaTime;
				float num = forceFalloff.Evaluate(t / knockDuration);
				richAI.Move(totalDisplacement * num * deltaTime / knockDuration);
				t += deltaTime;
			}
			yield return null;
		}
		isKnockbackActive = false;
	}

	private void TickMainHeroPushState(bool pushedThisFrame)
	{
		if (!IsPushableByMainHero)
		{
			ResetMainHeroPushState();
		}
		else if (IsInMainHeroPushProximity() || pushedThisFrame)
		{
			BeginMainHeroPushPriorityOverride();
			mainHeroPushActiveUntil = Time.time + settings.mainHeroPushHoldTime;
		}
		else if (!IsUnderMainHeroPush)
		{
			EndMainHeroPushPriorityOverride();
		}
	}

	private void BeginMainHeroPushPriorityOverride()
	{
		if (!rvoPriorityBeforeMainHeroPush.HasValue)
		{
			rvoPriorityBeforeMainHeroPush = RVO_Priority;
			RVO_Priority = settings.mainHeroPushAllyRvoPriority;
		}
	}

	private void EndMainHeroPushPriorityOverride()
	{
		if (rvoPriorityBeforeMainHeroPush.HasValue)
		{
			RVO_Priority = rvoPriorityBeforeMainHeroPush.Value;
			rvoPriorityBeforeMainHeroPush = null;
		}
	}

	private void ResetMainHeroPushState()
	{
		mainHeroPushActiveUntil = 0f;
		EndMainHeroPushPriorityOverride();
	}

	private void SyncAlliedWgoPositionFromRichAI()
	{
		if (IsPushableByMainHero && IsMovable && !((UnityEngine.Object)(object)richAI == null) && wgo?.Data != null)
		{
			Vector3 vector = richAI.ApplySeekerGraphSnap(richAI.position);
			if (!((wgo.Data.Position - vector).XZ().sqrMagnitude < 0.0001f))
			{
				wgo.Data.Position = vector;
			}
		}
	}

	private bool TryApplyRvoPushDisplacement(bool updateAnimation, float deltaTime)
	{
		if (!RVO_Enabled || deltaTime <= 0f)
		{
			return false;
		}
		if (IsPushableByMainHero)
		{
			if (!IsInMainHeroPushProximity())
			{
				return false;
			}
			if (RVO_Locked)
			{
				RVO_Locked = false;
			}
		}
		else if (RVO_Locked)
		{
			return false;
		}
		if (!richAI.simulateMovement)
		{
			RvoStopAt(richAI.position);
		}
		Vector3 position = richAI.position;
		Vector3 vector = rvoController.CalculateMovementDelta(position, deltaTime);
		if (vector.sqrMagnitude < 1E-06f)
		{
			return false;
		}
		Vector3 position2 = richAI.ApplySeekerGraphSnap(position + vector);
		wgo.Data.Position = position2;
		RvoStopAt(position2);
		SetFacingDirection(rvoController.velocity.XZ2());
		if (updateAnimation)
		{
			wgo.MainWgoPart.AnimationComponent?.SetState(AnimationState.Walk);
		}
		return true;
	}

	private bool IsInMainHeroPushProximity()
	{
		ICombatEntity combatEntity = MainGame.PlayerController?.PhysicalBody;
		if (combatEntity == null || wgo?.Data == null)
		{
			return false;
		}
		float num = rvoController.radius + 0.3f + 0.15f;
		return (combatEntity.CombatEntityPosition - wgo.Data.Position).XZ().sqrMagnitude <= num * num;
	}

	private void InterruptCommandDueToPlayerPush()
	{
		if (isExecutingCommand)
		{
			if (mobCommand is ZombieAttackCommand)
			{
				attackComponent?.CancelAttack();
			}
			StopCommandExecution(reportAlsoAsCompletion: true);
		}
	}

	private void ConfigureAgent()
	{
		richAI.autoRepath.mode = AutoRepathPolicy.Mode.Never;
		richAI.updatePosition = false;
		richAI.updateRotation = false;
		richAI.radius = settings.aiPathRadius;
		richAI.height = settings.aiPathHeight;
		richAI.funnelSimplification = true;
		richAI.slowWhenNotFacingTarget = settings.slowWhenNotFacingTarget;
		richAI.preventMovingBackwards = settings.slowWhenNotFacingTarget && settings.preventMovingBackwards;
		richAI.endReachedDistance = 0.15f;
		richAI.slowdownTime = 0.7f;
		richAI.wallForce = settings.wallForce;
		richAI.wallDist = settings.wallDist;
		richAI.rotationSpeed = 360f;
		richAI.rvoDensityBehavior.enabled = false;
		richAI.rvoDensityBehavior.returnAfterBeingPushedAway = true;
		richAI.rvoDensityBehavior.densityThreshold = settings.rvoDensityBehaviorDensityThreshold;
		richAI.gravity = Vector3.zero;
		richAI.maxSpeed = fighterDef.mvtSpeed;
		richAI.acceleration = fighterDef.mvtAcceleration;
		seeker.graphMask = GraphMask.FromGraphIndex(12u);
		rvoController.layer = RVOLayer.DefaultAgent;
		rvoController.collidesWith = (RVOLayer)5;
		rvoController.agentTimeHorizon = settings.rvoAgentTimeHorizon;
		rvoController.obstacleTimeHorizon = settings.rvoAgentObstacleTimeHorizon;
		rvoController.maxNeighbours = settings.rvoMaxNeighbours;
		rvoController.priority = settings.defaultRvoPriority;
	}

	private void Awake()
	{
		if (autoBakeAsset)
		{
			agentAI = AddressableUtils.LoadAssetReferenceSync(aiRef, ref agentAIHandle);
		}
		else
		{
			agentAI = editorAI;
		}
		damageEffectComponent = GetComponent<DamageEffectComponent>();
	}

	private void OnDestroy()
	{
		richAI?.SetPath(null, updateDestinationFromPath: false);
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePos);
		if (subscribedAnimationEventReceiver != null)
		{
			subscribedAnimationEventReceiver.onEvent1.RemoveListener(OnAnimationTriggerAttack);
		}
		AddressableUtils.ReleaseAssetReference(ref agentAIHandle);
	}

	private void UpdatePos(CinemachineBrain brain)
	{
		RichAI_Custom richAI_Custom = richAI;
		if (richAI_Custom != null && !richAI_Custom.GroundSnapEnabled)
		{
			wgo.transform.position = wgo.Data.Position;
		}
		else if (movementAdjustComponent != null && IsMovable)
		{
			movementAdjustComponent.UpdatePos(wgo.Data.Position);
		}
	}
}
