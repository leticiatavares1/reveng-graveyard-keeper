using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using Pathfinding.RVO;
using UnityEngine;

public class PlayerPhysicalBody : MonoBehaviour, ICombatEntity, IPhysicallyMutable
{
	public class GroundHit
	{
		public Vector3 position;

		public Vector3 normal;

		public GroundHit(Vector3 position, Vector3 normal)
		{
			this.position = position;
			this.normal = normal;
		}
	}

	private enum ZoneColliderType
	{
		Container,
		NonContainer,
		TownSubzone,
		Town
	}

	private const float WORLD_ZONE_RAYCAST_Y_SHIFT = 50f;

	[SerializeField]
	private PlayerPhysicsConfig physicsConfig;

	[Space]
	[SerializeField]
	private PlayerInvisibleWalls invisibleWalls;

	[Space]
	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private MeshCollider meshCollider;

	[SerializeField]
	private PlayerView playerView;

	private bool hasContactPoints;

	private bool isOnGround;

	private bool shouldStickOnGround;

	private bool isJumping;

	private Dictionary<int, SerializableCollision> contactCollisions = new Dictionary<int, SerializableCollision>();

	private List<SerializableContactPoint> contactPoints = new List<SerializableContactPoint>();

	[SerializeField]
	private RVOController rvo;

	private MultiFlagAND<PlayerDynamicType> dynamicMultiFlag = new MultiFlagAND<PlayerDynamicType>();

	private PlayerData playerData;

	private Vector3 prevPosition;

	private Vector2 prevDirection;

	private Vector3 gravityNormalized = Physics.gravity.normalized;

	private bool hasPhysUpdatedThisFixedFrame;

	private bool movedByDirectionThisFrame;

	private static RaycastHit[] raycastResults = new RaycastHit[10];

	private static RaycastHit[] raycastWZResults = new RaycastHit[15];

	[SerializeField]
	private DamageEffectComponent damageEffectComponent;

	[SerializeField]
	private int fightingQuality = 5;

	private bool isMoving;

	private bool isMovementLocked;

	private bool isSetRotationLocked;

	[SerializeField]
	private WorldZone currentContainerWorldZone;

	[SerializeField]
	private WorldZone currentNonContainerWorldZone;

	[SerializeField]
	private TownZone currentTownZone;

	[SerializeField]
	private TownSubZone currentTownSubZone;

	[SerializeField]
	private MapZone currentMapZone;

	private readonly Dictionary<ZoneColliderType, Action<PlayerPhysicalBody>> exitZoneActions = new Dictionary<ZoneColliderType, Action<PlayerPhysicalBody>>
	{
		{
			ZoneColliderType.Container,
			delegate(PlayerPhysicalBody obj)
			{
				obj.ExitZoneByType(ZoneColliderType.Container);
			}
		},
		{
			ZoneColliderType.NonContainer,
			delegate(PlayerPhysicalBody obj)
			{
				obj.ExitZoneByType(ZoneColliderType.NonContainer);
			}
		},
		{
			ZoneColliderType.TownSubzone,
			delegate(PlayerPhysicalBody obj)
			{
				obj.ExitZoneByType(ZoneColliderType.TownSubzone);
			}
		},
		{
			ZoneColliderType.Town,
			delegate(PlayerPhysicalBody obj)
			{
				obj.ExitZoneByType(ZoneColliderType.Town);
			}
		}
	};

	private Coroutine muteCoroutine;

	public PlayerView PlayerView => playerView;

	public Rigidbody Rb => rb;

	public PlayerPhysicsConfig PhysicsConfig => physicsConfig;

	public float SpeedMultiplier { get; set; } = 1f;


	public Vector3 GravityNormalized => gravityNormalized;

	public MeshCollider MeshCollider => meshCollider;

	public bool IsMuted { get; set; }

	public SGuid CombatEntityUID => playerData.Guid;

	public LazyConsts.Fighting.TeamType TeamType => LazyConsts.Fighting.TeamType.Player;

	public LazyConsts.Fighting.EntityType EntityType => LazyConsts.Fighting.EntityType.Player;

	public Vector3 CombatEntityPosition => rb.position;

	public int ArmorValue => playerData.toolBeltInventory.GetItemByType(ItemType.BodyArmor)?.Definition.quality ?? 0;

	public HPComponent CombatEntityHpComponent => playerData.hpComponent;

	public int CombatEntityQuality => fightingQuality;

	public int AttackPriority { get; set; }

	public bool HasAnyDockPoint => false;

	public bool IsActiveCombatant { get; set; } = true;


	public void Init()
	{
		playerView.Init();
		prevDirection = playerView.GetAnimationDirection();
		dynamicMultiFlag.Init(SetDynamicActive, initialFlag: true);
		if (physicsConfig == null)
		{
			Debug.LogError("PlayerPhysicsConfig must be set");
		}
	}

	public void PrepareForGame(PlayerData playerData)
	{
		this.playerData = playerData;
		SetPosition(playerData.position.Value);
		playerView.PrepareForGame(playerData);
		damageEffectComponent.Init(this);
		if ((UnityEngine.Object)(object)rvo != null)
		{
			rvo.priority = 0.85f;
		}
	}

	public void UnPrepareFromGame()
	{
		isOnGround = false;
		hasContactPoints = false;
		shouldStickOnGround = false;
		isJumping = false;
		isMovementLocked = false;
		playerView.UnPrepareFromGame();
	}

	public void SetDirectionLock(bool isEnabled)
	{
		isSetRotationLocked = !isEnabled;
	}

	public void InitNetworkPlayer(NetworkPlayer networkPlayer)
	{
		PlayerController component = GetComponent<PlayerController>();
		component.SetPlayerData(networkPlayer.playerData);
		component.Initialize();
		component.ResetControlState();
		component.gameObject.SetActive(value: true);
		component.WispController.gameObject.SetActive(value: true);
		component.enabled = false;
		SetDynamicActive(isDynamic: true);
		SetPosition(networkPlayer.playerData.position.Value);
		base.enabled = false;
	}

	public void SetPosition(Vector3 position)
	{
		bool flag = false;
		if (playerData.CurrentWorldZoneData != null && !playerData.CurrentWorldZoneData.wholeZoneRect.Contains(position.XZ2()))
		{
			playerData.SetCurrentWorldZoneData(null, RedrawWorldZoneWidget);
			currentContainerWorldZone = null;
			currentNonContainerWorldZone = null;
			flag = true;
		}
		for (int num = playerData.insideTownZones.Count - 1; num >= 0; num--)
		{
			TownZone townZone = playerData.insideTownZones[num];
			if ((bool)townZone && !townZone.Collider.bounds.Contains(position))
			{
				playerData.RemoveTownZone(townZone);
				flag = true;
			}
		}
		for (int num2 = playerData.insideTownSubZones.Count - 1; num2 >= 0; num2--)
		{
			TownSubZone townSubZone = playerData.insideTownSubZones[num2];
			if ((bool)townSubZone && !townSubZone.Collider.bounds.Contains(position))
			{
				playerData.RemoveTownSubZone(townSubZone);
				flag = true;
			}
		}
		if (flag)
		{
			GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
		}
		contactCollisions.Clear();
		contactPoints.Clear();
		rb.position = position;
		base.transform.position = position;
		playerData.position.Value = position;
	}

	public void MoveByDirection(Vector2 direction)
	{
		if ((bool)physicsConfig && !MainGame.IsGamePaused)
		{
			UpdatePhysics();
			if (!HasMovementPossibility(direction))
			{
				SetCurrentAnimationState(AnimationState.Idle);
				return;
			}
			float num = physicsConfig.speed * SpeedMultiplier;
			SetCurrentAnimationState(AnimationState.Walk);
			playerView.UpdateAnimationDirection(isSetRotationLocked ? playerData.Direction : direction.normalized);
			Vector3 toDirection = Vector3.ProjectOnPlane(gravityNormalized, Vector3.right);
			Vector3 toDirection2 = Vector3.ProjectOnPlane(gravityNormalized, Vector3.back);
			Quaternion quaternion = Quaternion.FromToRotation(Physics.gravity.normalized, toDirection);
			Quaternion quaternion2 = Quaternion.FromToRotation(Physics.gravity.normalized, toDirection2);
			Vector3 normalized = new Vector3(direction.x, 0f, direction.y).normalized;
			Vector3 a = quaternion * quaternion2 * normalized;
			a = Vector3.Scale(a, new Vector3(1f, 0.5999999f, 0.8f));
			Vector3 vector = Vector3.ProjectOnPlane(Physics.gravity.normalized * a.magnitude, gravityNormalized);
			a += vector * physicsConfig.slopeGravityMovementSlowdown;
			Vector3 force = a * (num * Time.fixedDeltaTime);
			rb.AddForce(force, ForceMode.VelocityChange);
			Debug.DrawRay(base.transform.position, a, Color.black, 0.2f);
			movedByDirectionThisFrame = true;
		}
	}

	public void MoveByPosition(Vector3 position, Vector2 movementDirection, bool needDirectionChange = true)
	{
		if (!rb.isKinematic)
		{
			Debug.LogError("Trying to move non-static RB by position");
			return;
		}
		SetCurrentAnimationState(AnimationState.Walk);
		if (needDirectionChange)
		{
			playerView.UpdateAnimationDirection(movementDirection.normalized);
		}
		rb.position = position;
		UpdatePlayerData();
	}

	public void StopMoving()
	{
		if (playerView.PlayerAnimation.AnimationState == AnimationState.Walk)
		{
			SetCurrentAnimationState(AnimationState.Idle);
		}
	}

	public void SetFacingDirection(Vector2 direction)
	{
		if (!(direction.sqrMagnitude < 0.0001f))
		{
			Vector2 normalized = direction.normalized;
			if (!isSetRotationLocked)
			{
				playerData.Direction = normalized;
			}
			playerView.UpdateAnimationDirection(normalized);
		}
	}

	public void SetNonKinematicFlag(PlayerDynamicType type, bool isDynamic)
	{
		dynamicMultiFlag.UpdateFlag(type, isDynamic);
	}

	private void SetDynamicActive(bool isDynamic)
	{
		Mute();
		rb.isKinematic = !isDynamic;
	}

	public void LockMovement(bool isLock)
	{
		isMovementLocked = isLock;
		SetDynamicActive(!isLock);
	}

	public void Mute(Action onUnmuted = null)
	{
		if (muteCoroutine != null)
		{
			StopCoroutine(muteCoroutine);
		}
		muteCoroutine = StartCoroutine(MuteForOneFrameCoroutine(onUnmuted));
	}

	private IEnumerator MuteForOneFrameCoroutine(Action onUnmuted)
	{
		IsMuted = true;
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		IsMuted = false;
		muteCoroutine = null;
		onUnmuted?.Invoke();
	}

	private bool HasMovementPossibility(Vector2 direction)
	{
		bool flag = !direction.magnitude.EqualsTo(0f);
		if (flag && !isMoving)
		{
			isMoving = true;
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerStartMoving);
		}
		else if (!flag && isMoving)
		{
			isMoving = false;
		}
		if (flag)
		{
			return !isMovementLocked;
		}
		return false;
	}

	public float GetCombatEntityDistance(Vector3 from, LazyConsts.Fighting.TeamType teamType)
	{
		return (from - CombatEntityPosition).magnitude;
	}

	public float GetCombatEntityGameRes(string resId)
	{
		if (playerData == null)
		{
			return 0f;
		}
		return playerData.GetRes(resId);
	}

	public void OnOtherCombatTargetReachedToMe(ICombatEntity other)
	{
	}

	public void OnOtherCombatTargetHitMe(AttackContext ctx)
	{
	}

	private void FixedUpdate()
	{
		UpdateWorldZone();
		if (!rb.isKinematic)
		{
			if ((bool)invisibleWalls)
			{
				invisibleWalls.UpdateEdgeWallsState(base.transform.position);
			}
			hasPhysUpdatedThisFixedFrame = false;
			UpdatePhysics();
			UpdatePlayerData();
			movedByDirectionThisFrame = false;
		}
	}

	private void UpdatePlayerData()
	{
		if ((prevPosition - rb.position).magnitude > 0.001f)
		{
			prevPosition = rb.position;
			playerData.position.Value = prevPosition;
		}
		Vector2 animationDirection = playerView.GetAnimationDirection();
		if ((prevDirection - animationDirection).magnitude > 0.001f)
		{
			prevDirection = animationDirection;
			playerData.Direction = animationDirection;
		}
	}

	private void SetCurrentAnimationState(AnimationState state)
	{
		playerData.charState.Value = state;
		playerView.PlayerAnimation.SetState(state);
	}

	private void UpdatePhysics()
	{
		if (!physicsConfig || hasPhysUpdatedThisFixedFrame)
		{
			return;
		}
		hasPhysUpdatedThisFixedFrame = true;
		gravityNormalized = Physics.gravity.normalized;
		GroundHit groundHit = GetGroundHit();
		Vector3 fieldNormal;
		bool flag = HasCustomGravityField(out fieldNormal);
		isOnGround = !isJumping && (hasContactPoints || shouldStickOnGround);
		if ((bool)invisibleWalls)
		{
			if (!isOnGround && invisibleWalls.IsWallsCheckEnabled)
			{
				invisibleWalls.ChangeWallsVisibility(visible: false);
			}
			invisibleWalls.IsWallsCheckEnabled = isOnGround;
		}
		if (isOnGround)
		{
			if (hasContactPoints)
			{
				gravityNormalized = CalculateGravityVectorByContacts();
				rb.AddForce(gravityNormalized * (Physics.gravity.magnitude * rb.mass * physicsConfig.contactForceMult), ForceMode.Force);
				Debug.DrawRay(base.transform.position, gravityNormalized, Color.cyan);
				if (!movedByDirectionThisFrame)
				{
					Vector3 planeNormal = -gravityNormalized;
					Vector3 vector = Vector3.ProjectOnPlane(rb.linearVelocity, planeNormal);
					rb.linearVelocity -= vector;
				}
			}
			else
			{
				gravityNormalized = (flag ? (-fieldNormal) : (-groundHit.normal.normalized));
				rb.AddForce(gravityNormalized * (Physics.gravity.magnitude * rb.mass * physicsConfig.stickForceMult), ForceMode.Force);
				Debug.DrawRay(base.transform.position, gravityNormalized, Color.green, 1f);
			}
		}
		else if (!isJumping)
		{
			rb.AddForce(gravityNormalized * (Physics.gravity.magnitude * rb.mass * physicsConfig.gravityFallScale), ForceMode.Acceleration);
		}
	}

	private void OnCollisionEnter(Collision collisionSource)
	{
		if (!IsCollisionValid(collisionSource))
		{
			return;
		}
		SerializableCollision value = SerializableCollision.CreateFrom(collisionSource);
		if (contactCollisions.TryAdd(collisionSource.gameObject.GetHashCode(), value))
		{
			if (!shouldStickOnGround)
			{
				shouldStickOnGround = true;
			}
			UpdateContactPoints();
		}
	}

	private void OnCollisionStay(Collision collisionSource)
	{
		if (IsCollisionValid(collisionSource) && contactCollisions.TryGetValue(collisionSource.gameObject.GetHashCode(), out var value))
		{
			value.UpdateContactPoints(collisionSource);
			UpdateContactPoints();
		}
	}

	private void OnCollisionExit(Collision collisionSource)
	{
		if (IsCollisionValid(collisionSource) && contactCollisions.Remove(collisionSource.gameObject.GetHashCode()))
		{
			UpdateContactPoints();
		}
	}

	private int RaycastWorldZones()
	{
		return Physics.RaycastNonAlloc(new Ray(base.transform.position + Vector3.up * 50f, Vector3.down), raycastWZResults, 100f, 131072);
	}

	private static bool TryGetWorldZone(Collider col, out WorldZone worldZone)
	{
		if (!col.TryGetComponent<WorldZone>(out worldZone))
		{
			if ((bool)col.transform.parent)
			{
				return col.transform.parent.gameObject.TryGetComponent<WorldZone>(out worldZone);
			}
			return false;
		}
		return true;
	}

	private void EnterWorldZone(WorldZone worldZone)
	{
		if (playerData.CurrentWorldZoneData != worldZone.Data)
		{
			playerData.SetCurrentWorldZoneData(worldZone.Data, RedrawWorldZoneWidget);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.WalkIntoWorldZone, worldZone.Data.id);
			Debug.Log($"Player entered world zone: {worldZone.Id}, Quality: [{worldZone.Data.GetTotalQuality()}]");
			WorldZoneWidgetData data = new WorldZoneWidgetData(worldZone.Data);
			GUIElements.Instance.WorldZoneWidget.Draw(data);
			if (MainGame.Instance.GameSave.knowledgeSystem.visitedWorldZones != null && !MainGame.Instance.GameSave.knowledgeSystem.visitedWorldZones.Contains(worldZone.Id))
			{
				MainGame.Instance.GameSave.knowledgeSystem.visitedWorldZones.Add(worldZone.Id);
			}
			worldZone.RedrawWgosWidgets();
		}
	}

	private void ExitWorldZone(WorldZone worldZone)
	{
		WorldZoneData worldZoneData = ((worldZone != null) ? worldZone.Data : playerData.CurrentWorldZoneData);
		if (worldZoneData != null)
		{
			Debug.Log("Player exited world zone: " + worldZoneData.id);
			playerData.SetCurrentWorldZoneData(null, RedrawWorldZoneWidget);
			GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
			if (worldZone != null)
			{
				worldZone.HideWgosWorldZoneWidgets();
			}
		}
	}

	private bool IsCollisionValid(Collision collision)
	{
		if (!IsInLayerMask(collision.gameObject, 6144))
		{
			return false;
		}
		if (collision.gameObject == base.gameObject)
		{
			return false;
		}
		return true;
	}

	private Vector3 CalculateGravityVectorByContacts()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < contactPoints.Count; i++)
		{
			zero += contactPoints[i].normal;
		}
		return -zero.normalized;
	}

	private void UpdateContactPoints()
	{
		contactPoints.Clear();
		foreach (SerializableCollision value in contactCollisions.Values)
		{
			for (int i = 0; i < value.contactPoints.Length; i++)
			{
				SerializableContactPoint serializableContactPoint = value.contactPoints[i];
				if (value.layer == 12)
				{
					contactPoints.Add(serializableContactPoint);
				}
				else if ((bool)physicsConfig && physicsConfig.maxGroundAngle > Vector3.Angle(serializableContactPoint.normal, -Physics.gravity.normalized))
				{
					contactPoints.Add(serializableContactPoint);
				}
			}
		}
		hasContactPoints = contactPoints.Count > 0;
		UpdateShadowcasterFromContactPoints();
	}

	private void UpdateShadowcasterFromContactPoints()
	{
		if ((bool)playerView && (bool)playerView.cylinderShadowcaster)
		{
			if (!isOnGround)
			{
				playerView.cylinderShadowcaster.gameObject.SetActive(value: false);
				return;
			}
			playerView.cylinderShadowcaster.gameObject.SetActive(value: true);
			Vector3 groundNormal = -CalculateGravityVectorByContacts();
			UpdateShadowcaster(groundNormal);
		}
	}

	private void UpdateShadowcaster(Vector3 groundNormal)
	{
		float num = (playerView.cylinderShadowcaster.parent ? playerView.cylinderShadowcaster.parent.localScale.x : 1f);
		Quaternion localRotation = Quaternion.FromToRotation(Vector3.up, groundNormal);
		if (num < 0f)
		{
			localRotation = Quaternion.Euler(localRotation.eulerAngles.x, localRotation.eulerAngles.y, 0f - localRotation.eulerAngles.z);
		}
		playerView.cylinderShadowcaster.localRotation = localRotation;
		float num2 = Vector3.Angle(groundNormal, Vector3.up) * (MathF.PI / 180f);
		float num3 = 1f + 0.5f * Mathf.Sin(num2 * 2f);
		float num4 = Mathf.Pow(Mathf.Cos(num2), 2f);
		Vector3 cylinderShadowCasterScale = playerView.cylinderShadowCasterScale;
		cylinderShadowCasterScale.y *= num3;
		cylinderShadowCasterScale.z *= num4;
		playerView.cylinderShadowcaster.localScale = cylinderShadowCasterScale;
	}

	private GroundHit GetGroundHit()
	{
		float num = 0.01666667f;
		Vector3 vector = base.transform.position + Vector3.up * num;
		GroundHit result = new GroundHit(new Vector3(base.transform.position.x, 0f, base.transform.position.z), Vector3.up);
		if (!physicsConfig)
		{
			return result;
		}
		int num2 = Physics.RaycastNonAlloc(new Ray(vector, Vector3.down), raycastResults, physicsConfig.groundRaycastLength, 6144);
		if (num2 > 0)
		{
			RaycastHit raycastHit = raycastResults[0];
			float num3 = (raycastHit.point - vector).magnitude;
			for (int i = 1; i < num2; i++)
			{
				RaycastHit raycastHit2 = raycastResults[i];
				float magnitude = (raycastHit2.point - vector).magnitude;
				if (magnitude < num3)
				{
					num3 = magnitude;
					raycastHit = raycastHit2;
				}
			}
			result = new GroundHit(raycastHit.point, raycastHit.normal);
		}
		return result;
	}

	private GroundHit GetGroundHitFromVector(Vector3 position, Vector3 direction)
	{
		if (!physicsConfig)
		{
			return null;
		}
		GroundHit result = null;
		Ray ray = new Ray(position, direction);
		RaycastHit[] array = new RaycastHit[2];
		int num = Physics.RaycastNonAlloc(ray, array, physicsConfig.groundRaycastLength, 2048);
		if (num > 0)
		{
			RaycastHit raycastHit = array[0];
			float num2 = (raycastHit.point - position).magnitude;
			for (int i = 1; i < num; i++)
			{
				RaycastHit raycastHit2 = array[i];
				float magnitude = (raycastHit2.point - position).magnitude;
				if (magnitude < num2)
				{
					num2 = magnitude;
					raycastHit = raycastHit2;
				}
			}
			result = new GroundHit(raycastHit.point, raycastHit.normal);
		}
		return result;
	}

	private bool HasCustomGravityField(out Vector3 fieldNormal)
	{
		fieldNormal = Vector3.up;
		float num = 0.01666667f;
		Vector3 origin = base.transform.position + Vector3.up * num;
		Ray ray = new Ray(origin, Vector3.down);
		RaycastHit[] array = new RaycastHit[1];
		if (Physics.RaycastNonAlloc(ray, array, physicsConfig.groundRaycastLength, 8192) > 0)
		{
			fieldNormal = array[0].normal;
			return true;
		}
		return false;
	}

	private static bool IsInLayerMask(GameObject gameObject, LayerMask mask)
	{
		return (int)mask == ((int)mask | (1 << gameObject.layer));
	}

	private void RedrawWorldZoneWidget()
	{
		if (playerData.CurrentWorldZoneData != null)
		{
			WorldZoneWidgetData data = new WorldZoneWidgetData(playerData.CurrentWorldZoneData);
			GUIElements.Instance.WorldZoneWidget.Draw(data);
		}
	}

	private void UpdateWorldZone()
	{
		int num = RaycastWorldZones();
		WorldZone worldZone = null;
		WorldZone worldZone2 = null;
		TownZone townZone = null;
		TownSubZone townSubZone = null;
		MapZone mapZone = null;
		for (int i = 0; i < num; i++)
		{
			Collider collider = raycastWZResults[i].collider;
			if (TryGetWorldZone(collider, out var worldZone3))
			{
				if (worldZone3.Data.Definition.displayType == WorldZoneDef.DisplayType.Hidden)
				{
					continue;
				}
				switch (worldZone3.WorldZoneType)
				{
				case WorldZoneData.WorldZoneType.Default:
					if (worldZone == null)
					{
						worldZone = worldZone3;
					}
					else if (Vector3.Distance(worldZone3.Data.Center, playerData.position.Value) < Vector3.Distance(worldZone.Data.Center, playerData.position.Value))
					{
						worldZone = worldZone3;
					}
					break;
				case WorldZoneData.WorldZoneType.SimpleNotContainer:
					if (worldZone2 == null)
					{
						worldZone2 = worldZone3;
					}
					break;
				}
			}
			if (townZone == null && collider.TryGetComponent<TownZone>(out var component))
			{
				townZone = component;
			}
			if (townSubZone == null && collider.TryGetComponent<TownSubZone>(out var component2))
			{
				townSubZone = component2;
			}
			if (mapZone == null && collider.TryGetComponent<MapZone>(out var component3))
			{
				mapZone = component3;
			}
		}
		if (worldZone == null && currentContainerWorldZone != null)
		{
			ExitZoneByType(ZoneColliderType.Container);
		}
		if (worldZone2 == null && currentNonContainerWorldZone != null)
		{
			ExitZoneByType(ZoneColliderType.NonContainer);
		}
		if (townZone == null && currentTownZone != null)
		{
			ExitZoneByType(ZoneColliderType.Town);
		}
		if (townSubZone == null && currentTownSubZone != null)
		{
			ExitZoneByType(ZoneColliderType.TownSubzone);
		}
		if (currentMapZone != null && mapZone != currentMapZone)
		{
			currentMapZone = null;
		}
		if (worldZone != null)
		{
			if (worldZone != currentContainerWorldZone || playerData.CurrentWorldZoneData != worldZone.Data)
			{
				ExitAllZones();
				EnterWorldZone(worldZone);
				currentContainerWorldZone = worldZone;
			}
		}
		else if (worldZone2 != null)
		{
			if (worldZone2 != currentNonContainerWorldZone || playerData.CurrentWorldZoneData != worldZone2.Data)
			{
				ExitAllZones();
				EnterWorldZone(worldZone2);
				currentNonContainerWorldZone = worldZone2;
			}
		}
		else if (townZone != null)
		{
			if (townZone != currentTownZone)
			{
				ExitAllZonesExcept(ZoneColliderType.NonContainer);
				playerData.AddTownZone(townZone);
				GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
				currentTownZone = townZone;
			}
		}
		else if (townSubZone != null && townSubZone != currentTownSubZone)
		{
			ExitAllZonesExcept(ZoneColliderType.Town);
			playerData.AddTownSubZone(townSubZone);
			GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
			currentTownSubZone = townSubZone;
		}
		if (mapZone != null && mapZone != currentMapZone)
		{
			if (!MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(CharacterWindowData.CharPage.Map) && !MainGame.Instance.GameSave.knowledgeSystem.knownMapZones.Contains(mapZone.Id))
			{
				MainGame.Instance.GameSave.knowledgeSystem.knownMapZones.Add(mapZone.Id);
			}
			currentMapZone = mapZone;
		}
	}

	private void ExitZoneByType(ZoneColliderType zoneColliderType)
	{
		switch (zoneColliderType)
		{
		case ZoneColliderType.Container:
			if (currentContainerWorldZone != null)
			{
				ExitWorldZone(currentContainerWorldZone);
				currentContainerWorldZone = null;
			}
			break;
		case ZoneColliderType.NonContainer:
			if (currentNonContainerWorldZone != null)
			{
				ExitWorldZone(currentNonContainerWorldZone);
				currentNonContainerWorldZone = null;
			}
			break;
		case ZoneColliderType.TownSubzone:
			if (currentTownSubZone != null)
			{
				playerData.RemoveTownSubZone(currentTownSubZone);
				GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
				currentTownSubZone = null;
			}
			break;
		case ZoneColliderType.Town:
			if (currentTownZone != null)
			{
				playerData.RemoveTownZone(currentTownZone);
				GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
				currentTownZone = null;
			}
			break;
		}
	}

	private void ExitAllZonesExcept(ZoneColliderType zoneColliderType)
	{
		foreach (KeyValuePair<ZoneColliderType, Action<PlayerPhysicalBody>> exitZoneAction in exitZoneActions)
		{
			if (exitZoneAction.Key != zoneColliderType)
			{
				exitZoneAction.Value(this);
			}
		}
	}

	private void ExitAllZones()
	{
		foreach (KeyValuePair<ZoneColliderType, Action<PlayerPhysicalBody>> exitZoneAction in exitZoneActions)
		{
			exitZoneAction.Value(this);
		}
	}
}
