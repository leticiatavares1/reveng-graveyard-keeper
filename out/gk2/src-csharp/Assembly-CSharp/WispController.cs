using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LinqTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class WispController : SerializedMonoBehaviour
{
	private static int FAIRY_APPEAR_TRIGEGR = Animator.StringToHash("fairy_appear");

	private static int FAIRY_DISAPPEAR_TRIGEGR = Animator.StringToHash("fairy_disappear");

	[SerializeField]
	private string id;

	[SerializeField]
	private Animator wispAnimator;

	[SerializeField]
	private Transform animationObjectForPositionChange;

	[SerializeField]
	private Transform targetTransform;

	[SerializeField]
	private Vector3 targetVector;

	[SerializeField]
	private GDPointData targetGDPoint;

	[SerializeField]
	private WgoData targetWgoData;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private float safeDistanceToTarget;

	[SerializeField]
	private float defaultDistanceToTarget;

	[SerializeField]
	private float bigDistanceToTarget;

	[SerializeField]
	private float teleportDistance = 5f;

	[SerializeField]
	private float followSpeedMinInDefaultDistance;

	[SerializeField]
	private float followSpeedMaxInDefaultDistance;

	[SerializeField]
	private float followSpeedMinInBigDistance;

	[SerializeField]
	private float followSpeedMaxInBigDistance;

	[SerializeField]
	private float maxForce;

	[SerializeField]
	private float bigDistanceTimeLimit;

	[SerializeField]
	private float yOffsetForWgoDataFollow = 1f;

	[SerializeField]
	private Transform bubblePoint;

	private List<ParticleSystem> particleSystems;

	[OdinSerialize]
	private Dictionary<WispType, WispTypedData> wispTypedDatas = new Dictionary<WispType, WispTypedData>();

	private WispTargetType currentTargetType;

	private WispType wispType;

	private float bigDistanceTime;

	public Transform BubblePoint => bubblePoint;

	private List<ParticleSystem> ParticleSystems
	{
		get
		{
			if (particleSystems == null || particleSystems.Count == 0)
			{
				particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true).ToList();
			}
			return particleSystems;
		}
	}

	public string Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public Transform TargetTransform => targetTransform;

	public void ChangeTargetType(WispTargetType wispTargetType)
	{
		currentTargetType = wispTargetType;
	}

	public void ChangeWispType(WispType wispType)
	{
		switch (wispType)
		{
		case WispType.Fairy:
			wispAnimator.SetTrigger(FAIRY_APPEAR_TRIGEGR);
			break;
		case WispType.Normal:
			wispAnimator.SetTrigger(FAIRY_DISAPPEAR_TRIGEGR);
			break;
		}
		this.wispType = wispType;
	}

	public void ChangeActiveState(bool enabled)
	{
		Debug.Log($"Wisp ChangeActiveState enabled:[{enabled}]");
		base.gameObject.SetActive(enabled);
	}

	public bool GetActiveState()
	{
		return base.gameObject.activeSelf;
	}

	public void SetTargetTransform(Transform target)
	{
		targetTransform = target;
	}

	public void SetTargetVector(Vector3 target)
	{
		targetVector = target;
	}

	public void SetTargetGDPoint(GDPointData gdPoint)
	{
		targetGDPoint = gdPoint;
	}

	public void SetTargetWgoData(WgoData wgoData)
	{
		targetWgoData = wgoData;
	}

	public void TeleportToTarget(bool updateWgoData = true)
	{
		DefineTarget(out var hasTarget, out var targetPos);
		if (hasTarget)
		{
			Teleport(targetPos, updateWgoData);
		}
	}

	public void SetDirection(Direction direction)
	{
		GetWispWgoData().direction.Value = direction.ConvertToVector3();
	}

	[CanBeNull]
	public WgoData GetWispWgoData()
	{
		WgoData wgoDataForWisp = MainGame.WorldData.GetWgoDataForWisp(this);
		wgoDataForWisp.SetBubblePointOffset(bubblePoint.localPosition);
		return wgoDataForWisp;
	}

	private void Teleport(Vector3 targetPos, bool updateWgoData = true)
	{
		foreach (ParticleSystem particleSystem in ParticleSystems)
		{
			particleSystem.Simulate(0f, withChildren: true, restart: true);
		}
		SetPosition(targetPos, updateWgoData);
		foreach (ParticleSystem particleSystem2 in ParticleSystems)
		{
			particleSystem2.Play();
		}
	}

	private void FixedUpdate()
	{
		if (!MainGame.WorldData.HasCache || MainGame.IsGamePaused)
		{
			return;
		}
		DefineTarget(out var hasTarget, out var targetPos);
		if (hasTarget)
		{
			float num = Vector3.Distance(base.transform.position, targetPos);
			if (num > teleportDistance)
			{
				bigDistanceTime += Time.deltaTime;
				if (bigDistanceTime >= bigDistanceTimeLimit)
				{
					Teleport(targetPos);
					return;
				}
			}
			else
			{
				bigDistanceTime = 0f;
			}
			if (num > safeDistanceToTarget)
			{
				Vector3 normalized = (targetPos - base.transform.position).normalized;
				float num2 = ((!(num > defaultDistanceToTarget)) ? Mathf.Lerp(followSpeedMinInDefaultDistance, followSpeedMaxInDefaultDistance, num / defaultDistanceToTarget) : Mathf.Lerp(followSpeedMinInBigDistance, followSpeedMaxInBigDistance, num / bigDistanceToTarget));
				Vector3 vector = normalized * num2 - rb.linearVelocity;
				vector = Vector3.ClampMagnitude(vector, maxForce);
				rb.AddForce(vector, ForceMode.VelocityChange);
			}
		}
		else
		{
			SetPosition(targetPos);
		}
		switch (wispType)
		{
		case WispType.Normal:
			base.transform.localScale = Vector3.one;
			break;
		case WispType.Fairy:
		{
			if (Vector3.Distance(base.transform.position, targetPos) > 0.1f)
			{
				bool flag = base.transform.position.x <= targetPos.x;
				base.transform.localScale = (flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
				break;
			}
			WgoData wispWgoData = GetWispWgoData();
			if (wispWgoData != null)
			{
				bool flag2 = wispWgoData.Direction == Direction.Left;
				base.transform.localScale = (flag2 ? Vector3.one : new Vector3(-1f, 1f, 1f));
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetPosition(Vector3 targetPos, bool updateWgoData = true)
	{
		base.transform.position = targetPos;
		rb.position = targetPos;
		if (updateWgoData && currentTargetType != WispTargetType.WgoData && MainGame.Instance.gameState == MainGame.GameState.InGame)
		{
			WgoData wispWgoData = GetWispWgoData();
			if (wispWgoData != null)
			{
				wispWgoData.Position = targetPos;
			}
		}
	}

	private void DefineTarget(out bool hasTarget, out Vector3 targetPos)
	{
		hasTarget = false;
		targetPos = animationObjectForPositionChange.localPosition;
		switch (currentTargetType)
		{
		case WispTargetType.Transform:
			if (targetTransform != null)
			{
				hasTarget = true;
				targetPos += targetTransform.position;
			}
			break;
		case WispTargetType.GDPoint:
			if (targetGDPoint != null)
			{
				hasTarget = true;
				targetPos += targetGDPoint.Position;
			}
			break;
		case WispTargetType.Vector:
			if (targetVector != default(Vector3))
			{
				hasTarget = true;
				targetPos += targetVector;
			}
			break;
		case WispTargetType.WgoData:
			if (targetWgoData != null)
			{
				hasTarget = true;
				targetPos += targetWgoData.Position;
				targetPos.y += yOffsetForWgoDataFollow;
			}
			break;
		}
	}
}
