using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

public class FightingCapturePoint : MonoBehaviour
{
	[Serializable]
	public class TeamsFlag
	{
		public LazyConsts.Fighting.TeamType team;

		public List<GameObject> objs = new List<GameObject>();

		public GameObject ringVfxBase;

		public GameObject ringVfxActive;

		public ParticleSystem sparksOfRingParticleSystem;
	}

	private enum CaptureProgress
	{
		None,
		State,
		InProgressIncr,
		InProgressDecr
	}

	private const float CAPTURE_DURATION = 10f;

	private const float CAPTURE_COMPLEXITY = 20f;

	private const float VFX_BASE_SCALE = 2f;

	[SerializeField]
	private Collider col;

	public Transform flagTr;

	public Transform upPosTr;

	public Transform downPosTr;

	[Header("Ally flag stand")]
	[SerializeField]
	[Tooltip("Flag stand WGO in this sector.")]
	private FlagStandComponent allyFlagStand;

	public List<TeamsFlag> teamFlags = new List<TeamsFlag>();

	public bool isBasePoint;

	[SerializeField]
	private bool hasStartValue;

	[SerializeField]
	[Range(0f, 1f)]
	private float startValue;

	[Header("Capture duration")]
	public float captureDuration = 10f;

	public float captureComplexity = 20f;

	[SerializeField]
	private LazyConsts.Fighting.TeamType ownedByTeam = LazyConsts.Fighting.TeamType.WildZombie;

	[SerializeField]
	private LazyConsts.Fighting.TeamType ownedByTeamInGame;

	private bool isActivated;

	private bool isCapturedByAllies;

	private FightingSector sector;

	public readonly List<ICombatEntity> allies = new List<ICombatEntity>();

	public readonly List<ICombatEntity> enemies = new List<ICombatEntity>();

	private float currentProgress;

	private float radius;

	private float flagFlyingUpProgress;

	private Tween flagFlyAnimTween;

	private bool lockedForCapture;

	private CaptureProgress captureProgress;

	[SerializeReference]
	public List<CapturePointAction> OnCaptureActions = new List<CapturePointAction>();

	[SerializeReference]
	public List<CapturePointAction> OnLostActions = new List<CapturePointAction>();

	private Transform rotatableTransform;

	private ParticleSystem sparksOfRingParticleSystem;

	[SerializeField]
	private float maxRotationSpeedForSparks = 10f;

	[SerializeField]
	private float maxRotationSpeed = 100f;

	[SerializeField]
	private float timeToRotateToMaxSpeed = 1f;

	[SerializeField]
	private float currentRotationSpeed;

	private Dictionary<SGuid, Vector3> reservedStandardSlots = new Dictionary<SGuid, Vector3>();

	private Dictionary<SGuid, Vector3> reservedWaitingSlots = new Dictionary<SGuid, Vector3>();

	private const float MIN_SLOT_DISTANCE = 0.8f;

	private const float FLAG_STAND_POSITION_THRESHOLD = 0.5f;

	public bool LockedForCapture
	{
		get
		{
			return lockedForCapture;
		}
		set
		{
			if (lockedForCapture != value)
			{
				lockedForCapture = value;
				SetVfxActive(CaptureProgress.None, ignoreCheck: true);
			}
		}
	}

	public float CurrentProgress => currentProgress;

	public LazyConsts.Fighting.TeamType OwnedByTeamDefault => ownedByTeam;

	public LazyConsts.Fighting.TeamType OwnedByTeam => ownedByTeamInGame;

	public bool IsCapturedByAllies => isCapturedByAllies;

	public float Radius => radius;

	public FightingSector Sector => sector;

	public event Action<FightingCapturePoint> OnCapturedByTeam;

	public bool MatchesAllyFlagStandPosition(Vector3 worldPosition)
	{
		if (isBasePoint)
		{
			return false;
		}
		if (TryGetAllyFlagStandPosition(out var position))
		{
			float num = 0.25f;
			if ((worldPosition - position).XZ().sqrMagnitude <= num)
			{
				return true;
			}
		}
		return ContainsPosition(worldPosition);
	}

	public bool IsLinkedFlagStand(FlagStandComponent flagStand)
	{
		if (!flagStand || isBasePoint)
		{
			return false;
		}
		if ((bool)allyFlagStand)
		{
			return allyFlagStand == flagStand;
		}
		Vector3 worldPosition = (flagStand.StandWgo ? flagStand.StandWgo.Data.Position : flagStand.transform.position);
		return ContainsPosition(worldPosition);
	}

	public void BindAllyFlagController(AgentsGroupFlagController controller)
	{
		if ((bool)controller)
		{
			if (isBasePoint)
			{
				controller.ClearCapturePointBinding();
			}
			else
			{
				controller.BindCapturePoint(this);
			}
		}
	}

	public void TryBindFlagStand(FlagStandComponent flagStand)
	{
		if (IsLinkedFlagStand(flagStand))
		{
			flagStand.TryBindFlagToCapturePoint(this);
		}
	}

	public bool ContainsPosition(Vector3 worldPosition)
	{
		float num = radius;
		if (num <= 0f && (bool)col)
		{
			num = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
		}
		if (num <= 0f)
		{
			return false;
		}
		return (worldPosition - base.transform.position).XZ().magnitude < num;
	}

	public void SetActiveState(bool isActive)
	{
		base.gameObject.SetActive(isActive);
		SetProgress(hasStartValue ? startValue : 1f);
		SetOwnedByTeam(ownedByTeam);
		if (isActive)
		{
			StartCoroutine(CalculateRadiusNextFixedUpdate());
		}
	}

	private IEnumerator CalculateRadiusNextFixedUpdate()
	{
		yield return new WaitForFixedUpdate();
		radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
		Debug.Log($"CapturePoint {base.name} radius: {radius}", this);
		float num = radius / 2f;
		foreach (TeamsFlag teamFlag in teamFlags)
		{
			teamFlag.ringVfxBase.transform.localScale = new Vector3(num, 1f, num);
			teamFlag.ringVfxActive.transform.localScale = new Vector3(num, 1f, num);
		}
	}

	public void Init(FightingSector sector = null)
	{
		base.enabled = true;
		isActivated = true;
		this.sector = sector;
		isCapturedByAllies = false;
		SetVfxActive(CaptureProgress.None, ignoreCheck: true);
		TryBindLinkedFlagStand();
	}

	private void TryBindLinkedFlagStand()
	{
		TryBindFlagStand(allyFlagStand);
	}

	private bool TryGetAllyFlagStandPosition(out Vector3 position)
	{
		if (!allyFlagStand)
		{
			position = default(Vector3);
			return false;
		}
		Wgo standWgo = allyFlagStand.StandWgo;
		position = (standWgo ? standWgo.Data.Position : allyFlagStand.transform.position);
		return true;
	}

	public void DeInit()
	{
		base.enabled = false;
		isActivated = false;
		allies.Clear();
		enemies.Clear();
		reservedStandardSlots.Clear();
		reservedWaitingSlots.Clear();
	}

	private void Awake()
	{
		if (!col)
		{
			col = GetComponent<Collider>();
		}
		SetActiveState(isActive: false);
		base.enabled = false;
		LazySingleton<FightingGameController>.Instance.TargetsDatabase.OnTargetRemoved += HandleTargetRemoved;
		radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
		SetProgress(hasStartValue ? startValue : 1f);
		SetVfxActive(CaptureProgress.None, ignoreCheck: true);
	}

	private void Update()
	{
		if (MainGame.IsGamePaused)
		{
			return;
		}
		CustomUpdate();
		CaptureProgress captureProgress = this.captureProgress;
		if (captureProgress == CaptureProgress.InProgressDecr || captureProgress == CaptureProgress.InProgressIncr || currentRotationSpeed.More(0f))
		{
			captureProgress = this.captureProgress;
			float num = ((captureProgress == CaptureProgress.InProgressDecr || captureProgress == CaptureProgress.InProgressIncr) ? 1f : (-1f));
			currentRotationSpeed = Mathf.Clamp01(currentRotationSpeed + num * Time.deltaTime);
			if ((bool)rotatableTransform)
			{
				rotatableTransform.Rotate(Vector3.up, Time.deltaTime * currentRotationSpeed * maxRotationSpeed, Space.World);
			}
			if ((bool)sparksOfRingParticleSystem)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = sparksOfRingParticleSystem.velocityOverLifetime;
				velocityOverLifetime.enabled = true;
				ParticleSystem.MinMaxCurve orbitalY = velocityOverLifetime.orbitalY;
				orbitalY.constant = Time.deltaTime * currentRotationSpeed * maxRotationSpeedForSparks;
				velocityOverLifetime.orbitalY = orbitalY;
			}
		}
	}

	public void CustomUpdate()
	{
		if (!isActivated || LockedForCapture)
		{
			return;
		}
		RefreshCaptureOccupants();
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		bool flag2 = false;
		foreach (ICombatEntity ally in allies)
		{
			num += (float)ally.CombatEntityQuality;
			flag = !num.EqualsTo(0f);
		}
		foreach (ICombatEntity enemy in enemies)
		{
			num2 += (float)enemy.CombatEntityQuality;
			flag2 = !num2.EqualsTo(0f);
		}
		float num3 = num / captureComplexity;
		float num4 = num2 / captureComplexity;
		float delta = ((ownedByTeamInGame == LazyConsts.Fighting.TeamType.Player) ? 1f : (-1f)) * (num3 - num4);
		float num5 = PreCalculateCaptureProgress(delta);
		if (flag && flag2)
		{
			SetVfxActive(CaptureProgress.State);
			return;
		}
		float num6 = Mathf.Sign(num5 - currentProgress);
		SetVfxActive(Mathf.Abs(Mathf.Clamp01(num5) - currentProgress).EqualsTo(0f) ? CaptureProgress.State : ((num6 > 0f) ? CaptureProgress.InProgressIncr : CaptureProgress.InProgressDecr));
		if (currentProgress.EqualsOrMore(0f) && num5 < 0f)
		{
			LazyConsts.Fighting.TeamType teamType = ((ownedByTeamInGame == LazyConsts.Fighting.TeamType.Player) ? LazyConsts.Fighting.TeamType.WildZombie : LazyConsts.Fighting.TeamType.Player);
			isCapturedByAllies = teamType == LazyConsts.Fighting.TeamType.Player;
			ExecuteCaptureEvents(teamType);
			ExecuteLostEvents(ownedByTeamInGame);
			SetOwnedByTeam(teamType);
			flagFlyingUpProgress = 0f;
			flagFlyAnimTween = DOTween.To(() => flagFlyingUpProgress, delegate(float v)
			{
				flagFlyingUpProgress = v;
				SetProgress(v);
			}, 1f, 0.5f).SetEase(Ease.OutBounce);
			SetVfxActive(CaptureProgress.State, ignoreCheck: true);
			this.OnCapturedByTeam?.Invoke(this);
		}
		CalculateCaptureProgress(delta);
	}

	public void SetOwnedByTeam(LazyConsts.Fighting.TeamType team)
	{
		ownedByTeamInGame = team;
		foreach (TeamsFlag teamFlag in teamFlags)
		{
			foreach (GameObject obj in teamFlag.objs)
			{
				obj.gameObject.SetActive(teamFlag.team == team);
				if (teamFlag.team == team)
				{
					rotatableTransform = teamFlag.ringVfxBase.transform.GetChild(0);
					sparksOfRingParticleSystem = teamFlag.sparksOfRingParticleSystem;
				}
			}
		}
	}

	private void HandleTargetRemoved(TargetInfo targetInfo)
	{
		switch (targetInfo.Team)
		{
		case LazyConsts.Fighting.TeamType.Player:
			allies.Remove(targetInfo.entity);
			break;
		case LazyConsts.Fighting.TeamType.WildZombie:
			enemies.Remove(targetInfo.entity);
			break;
		}
		ReleaseSlot(targetInfo.entity.CombatEntityUID);
	}

	public bool TryReserveSlot(SGuid agentId, out Vector3 position, out bool isWaitingSlot)
	{
		position = Vector3.zero;
		isWaitingSlot = false;
		if (reservedStandardSlots.TryGetValue(agentId, out position))
		{
			return true;
		}
		if (reservedWaitingSlots.TryGetValue(agentId, out position))
		{
			isWaitingSlot = true;
			return true;
		}
		if (TryFindAvailablePosition(radius, 0f, reservedStandardSlots.Values, out position))
		{
			reservedStandardSlots[agentId] = position;
			return true;
		}
		if (TryFindAvailablePosition(radius + 3f, radius + 0.5f, reservedWaitingSlots.Values, out position))
		{
			reservedWaitingSlots[agentId] = position;
			isWaitingSlot = true;
			return true;
		}
		return false;
	}

	public void ReleaseSlot(SGuid agentId)
	{
		reservedStandardSlots.Remove(agentId);
		reservedWaitingSlots.Remove(agentId);
	}

	private bool TryFindAvailablePosition(float maxRadius, float minRadius, IEnumerable<Vector3> existingSlots, out Vector3 position)
	{
		position = Vector3.zero;
		if ((UnityEngine.Object)(object)AstarPath.active == null || AstarPath.active.graphs.Length <= 12)
		{
			return false;
		}
		if (!(AstarPath.active.graphs[12] is RecastGraph recastGraph))
		{
			return false;
		}
		Vector3 position2 = base.transform.position;
		NearestNodeConstraint walkable = NearestNodeConstraint.Walkable;
		walkable.graphMask = GraphMask.FromGraph(recastGraph);
		for (int i = 0; i < 24; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle;
			Mathf.Lerp(minRadius, maxRadius, UnityEngine.Random.value);
			if (minRadius > 0f)
			{
				vector = vector.normalized * Mathf.Lerp(minRadius, maxRadius, Mathf.Sqrt(UnityEngine.Random.value));
			}
			else
			{
				vector *= maxRadius;
			}
			Vector3 position3 = new Vector3(position2.x + vector.x, position2.y, position2.z + vector.y);
			GraphNode graphNode = recastGraph.PointOnNavmesh(position3, walkable);
			if (graphNode == null)
			{
				continue;
			}
			Vector3 vector2 = (Vector3)graphNode.position;
			float num = vector2.x - position2.x;
			float num2 = vector2.z - position2.z;
			float num3 = num * num + num2 * num2;
			if (!(num3 <= maxRadius * maxRadius) || !(num3 >= minRadius * minRadius))
			{
				continue;
			}
			bool flag = false;
			foreach (Vector3 existingSlot in existingSlots)
			{
				if ((vector2 - existingSlot).sqrMagnitude < 0.64000005f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				position = vector2;
				return true;
			}
		}
		return false;
	}

	private void OnDestroy()
	{
		if ((bool)LazySingleton<FightingGameController>.Instance && LazySingleton<FightingGameController>.Instance.TargetsDatabase != null)
		{
			LazySingleton<FightingGameController>.Instance.TargetsDatabase.OnTargetRemoved -= HandleTargetRemoved;
		}
		flagFlyAnimTween?.Kill();
	}

	private void RefreshCaptureOccupants()
	{
		PruneInvalidCaptureOccupants();
		FightingGameController instance = LazySingleton<FightingGameController>.Instance;
		if (!(instance == null) && instance.TargetsDatabase != null)
		{
			TryAddOverlappingCombatants(instance.TargetsDatabase.GetTargetsByTeam(LazyConsts.Fighting.TeamType.Player), allies);
			TryAddOverlappingCombatants(instance.TargetsDatabase.GetTargetsByTeam(LazyConsts.Fighting.TeamType.WildZombie), enemies);
		}
	}

	private void PruneInvalidCaptureOccupants()
	{
		for (int num = allies.Count - 1; num >= 0; num--)
		{
			ICombatEntity combatEntity = allies[num];
			if (!CountsAsOccupyingCapturePoint(combatEntity))
			{
				allies.RemoveAt(num);
				if (combatEntity != null)
				{
					ReleaseSlot(combatEntity.CombatEntityUID);
				}
			}
		}
		for (int num2 = enemies.Count - 1; num2 >= 0; num2--)
		{
			ICombatEntity combatEntity2 = enemies[num2];
			if (!CountsAsOccupyingCapturePoint(combatEntity2))
			{
				enemies.RemoveAt(num2);
				if (combatEntity2 != null)
				{
					ReleaseSlot(combatEntity2.CombatEntityUID);
				}
			}
		}
	}

	private void TryAddOverlappingCombatants(IReadOnlyList<ICombatEntity> candidates, List<ICombatEntity> occupants)
	{
		for (int i = 0; i < candidates.Count; i++)
		{
			ICombatEntity combatEntity = candidates[i];
			if (CountsAsOccupyingCapturePoint(combatEntity) && !occupants.Contains(combatEntity))
			{
				occupants.Add(combatEntity);
			}
		}
	}

	private bool CountsAsOccupyingCapturePoint(ICombatEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity is UnityEngine.Object @object && !@object)
		{
			return false;
		}
		if (!entity.IsActiveCombatant)
		{
			return false;
		}
		HPComponent combatEntityHpComponent = entity.CombatEntityHpComponent;
		if (combatEntityHpComponent != null && combatEntityHpComponent.Hp <= 0 && combatEntityHpComponent.WasDamagedAtLeastOnce)
		{
			return false;
		}
		return OverlapsCaptureVolume(entity.CombatEntityPosition);
	}

	private bool OverlapsCaptureVolume(Vector3 position)
	{
		if (!IsPointInsideCaptureCollider(position))
		{
			return IsPointInsideCaptureCollider(position + Vector3.up * 0.5f);
		}
		return true;
	}

	private bool IsPointInsideCaptureCollider(Vector3 position)
	{
		if (!col)
		{
			return ContainsPosition(position);
		}
		return (col.ClosestPoint(position) - position).sqrMagnitude < 0.0001f;
	}

	private void CalculateCaptureProgress(float delta)
	{
		currentProgress = PreCalculateCaptureProgress(delta);
		SetProgress(currentProgress);
	}

	private float PreCalculateCaptureProgress(float delta)
	{
		return currentProgress + delta * Time.deltaTime / captureDuration;
	}

	private void SetProgress(float progress)
	{
		currentProgress = Mathf.Clamp01(progress);
		flagTr.position = Vector3.Lerp(downPosTr.position, upPosTr.position, currentProgress);
	}

	private void ExecuteCaptureEvents(LazyConsts.Fighting.TeamType whoCaptured)
	{
		foreach (CapturePointAction onCaptureAction in OnCaptureActions)
		{
			onCaptureAction.Execute(whoCaptured, this);
			onCaptureAction.Execute(whoCaptured, LazySingleton<FightingGameController>.Instance.CurrentLevel);
		}
	}

	private void ExecuteLostEvents(LazyConsts.Fighting.TeamType whoLost)
	{
		foreach (CapturePointAction onLostAction in OnLostActions)
		{
			onLostAction.Execute(whoLost, this);
			onLostAction.Execute(whoLost, LazySingleton<FightingGameController>.Instance.CurrentLevel);
		}
	}

	private void SetVfxActive(CaptureProgress captureProgress, bool ignoreCheck = false)
	{
		if (!ignoreCheck && this.captureProgress == captureProgress)
		{
			return;
		}
		foreach (TeamsFlag teamFlag in teamFlags)
		{
			teamFlag.ringVfxActive.SetActive(captureProgress == CaptureProgress.InProgressDecr && teamFlag.team == LazyConsts.Fighting.TeamType.Player);
			teamFlag.ringVfxBase.SetActive(value: true);
		}
		this.captureProgress = captureProgress;
	}
}
