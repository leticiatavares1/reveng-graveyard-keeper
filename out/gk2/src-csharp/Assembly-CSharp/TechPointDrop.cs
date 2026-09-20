using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TechPointDrop : MonoBehaviour
{
	private static Pool pool;

	private static readonly int color = Animator.StringToHash("color");

	private static readonly List<TechPointDrop> activeDrops = new List<TechPointDrop>();

	[SerializeField]
	private float delay = 0.7f;

	[SerializeField]
	private float magnetRadius = 200f;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Collider coll;

	[SerializeField]
	private float collectRadius = 0.25f;

	[SerializeField]
	private float magnetForce = 35f;

	[SerializeField]
	private float magnetTurnSpeed = 720f;

	private Transform tf;

	private Transform player;

	private TechPointDropData data;

	private float collectDelay = 1f;

	private float curTime;

	private string techPointName;

	private bool isInitialized;

	private bool isTimedCollecting;

	private Coroutine moveToCollectorCoroutine;

	public Rigidbody RigidBody => rigidBody;

	public TechPointDropData Data => data;

	public static TechPointDrop Spawn(TechPointDropData techPointDropData, Transform parent)
	{
		if (pool == null)
		{
			pool = LazyPooler.CreatePool(Addressables.LoadAssetAsync<GameObject>("Assets/AddressableAssets/Drops/TechPointDrop.prefab").WaitForCompletion().GetComponent<TechPointDrop>(), 0, Pool.PoolType.ImmediateActivation, parentAllObjectsInPool: true);
		}
		TechPointDrop orCreateObject = pool.GetOrCreateObject<TechPointDrop>();
		orCreateObject.Init(techPointDropData);
		orCreateObject.transform.SetParent(parent);
		orCreateObject.transform.position = techPointDropData.pos;
		orCreateObject.animator.SetInteger(color, (int)techPointDropData.type);
		orCreateObject.RigidBody.linearVelocity = Vector3.zero;
		orCreateObject.RigidBody.angularVelocity = Vector3.zero;
		return orCreateObject;
	}

	public static void CollectAllToPlayer(Transform target, float duration)
	{
		for (int num = activeDrops.Count - 1; num >= 0; num--)
		{
			TechPointDrop techPointDrop = activeDrops[num];
			if (techPointDrop != null)
			{
				techPointDrop.MoveToCollectorTimed(target, duration);
			}
		}
	}

	public static bool IsTracked(TechPointDropData dropData)
	{
		if (dropData == null)
		{
			return false;
		}
		for (int i = 0; i < activeDrops.Count; i++)
		{
			TechPointDrop techPointDrop = activeDrops[i];
			if (techPointDrop != null && techPointDrop.data == dropData)
			{
				return true;
			}
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, collectRadius);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.position, magnetRadius);
	}

	private void Init(TechPointDropData techPointDropData)
	{
		StopTimedMove();
		data = techPointDropData;
		techPointName = TechDef.FlyingReses[(int)techPointDropData.type];
		coll.enabled = true;
		collectDelay = delay;
		curTime = 0f;
		tf = base.transform;
		player = MainGame.PlayerController.transform;
		isInitialized = true;
		isTimedCollecting = false;
		rigidBody.useGravity = false;
		rigidBody.isKinematic = false;
		RegisterActive();
	}

	private void FixedUpdate()
	{
		if (!isInitialized || isTimedCollecting || MainGame.IsGamePaused)
		{
			return;
		}
		data.pos = rigidBody.position;
		curTime += Time.fixedDeltaTime;
		if (curTime < collectDelay)
		{
			return;
		}
		Vector3 vector = player.position - tf.position;
		float sqrMagnitude = vector.XZ().sqrMagnitude;
		if (sqrMagnitude > magnetRadius)
		{
			coll.isTrigger = false;
			return;
		}
		coll.isTrigger = true;
		if (sqrMagnitude < collectRadius * collectRadius)
		{
			Collect();
			return;
		}
		Vector3 linearVelocity = rigidBody.linearVelocity;
		if (linearVelocity.sqrMagnitude > 0.0001f && vector.sqrMagnitude > 0.0001f)
		{
			rigidBody.linearVelocity = Vector3.RotateTowards(linearVelocity, vector, magnetTurnSpeed * (MathF.PI / 180f) * Time.fixedDeltaTime, 0f);
		}
		rigidBody.AddForce(vector * (magnetForce * 1.2f));
	}

	public void MoveToCollectorTimed(Transform target, float duration)
	{
		if (isInitialized && data != null && !isTimedCollecting)
		{
			StopTimedMove();
			isTimedCollecting = true;
			coll.enabled = false;
			rigidBody.linearVelocity = Vector3.zero;
			rigidBody.angularVelocity = Vector3.zero;
			rigidBody.isKinematic = true;
			if (target == null || duration <= 0f)
			{
				Collect();
			}
			else
			{
				moveToCollectorCoroutine = StartCoroutine(MoveToCollectorTimedCoroutine(target, duration));
			}
		}
	}

	private IEnumerator MoveToCollectorTimedCoroutine(Transform target, float duration)
	{
		Vector3 startPosition = ((rigidBody != null) ? rigidBody.position : base.transform.position);
		float elapsed = 0f;
		WaitForEndOfFrame wait = new WaitForEndOfFrame();
		while (elapsed < duration)
		{
			if (!isInitialized || data == null)
			{
				yield break;
			}
			if (target == null)
			{
				moveToCollectorCoroutine = null;
				Collect();
				yield break;
			}
			if (!MainGame.IsGamePaused)
			{
				elapsed += Time.deltaTime;
				float num = Mathf.Clamp01(elapsed / duration);
				SetWorldPosition(Vector3.Lerp(startPosition, target.position, num * num));
			}
			yield return wait;
		}
		if (isInitialized && data != null)
		{
			if (target != null)
			{
				SetWorldPosition(target.position);
			}
			moveToCollectorCoroutine = null;
			Collect();
		}
	}

	private void SetWorldPosition(Vector3 position)
	{
		base.transform.position = position;
		if (rigidBody != null)
		{
			rigidBody.position = position;
		}
		if (data != null)
		{
			data.pos = position;
		}
	}

	private void Collect()
	{
		if (isInitialized && data != null)
		{
			StopTimedMove();
			UnregisterActive();
			isInitialized = false;
			coll.enabled = false;
			DebugDraw.DrawCross(FlyingTechPoint.Drop(MainGame.PlayerController.transform.position, techPointName).transform.position, 1f, Color.yellow, 2f);
			PlaySound();
			MainGame.Instance.GameSave.WorldData.GetGameSceneDataById(data.worldId).RemoveTechPointDrop(data);
			collectDelay = 1f;
			curTime = 0f;
			data = null;
			pool.ReleaseObject(this);
		}
	}

	private void PlaySound()
	{
		if (data.type == TechPointsSpawner.Type.H)
		{
			FlyingTechPoint.FlyingTechPointsCountByName.TryGetValue("happiness", out var value);
			if (MainGame.Instance.GameSave.townSystem.Quality > 0)
			{
				LazyAudio.Play((MainGame.PlayerData.GetResInt("happiness") + value > MainGame.Instance.GameSave.townSystem.Quality) ? "tech_point_collect_failed" : "tech_point_collect");
			}
			else
			{
				LazyAudio.Play("tech_point_collect");
			}
		}
		else
		{
			LazyAudio.Play("tech_point_collect");
		}
	}

	private void RegisterActive()
	{
		if (!activeDrops.Contains(this))
		{
			activeDrops.Add(this);
		}
	}

	private void UnregisterActive()
	{
		activeDrops.Remove(this);
	}

	private void StopTimedMove()
	{
		if (moveToCollectorCoroutine != null)
		{
			StopCoroutine(moveToCollectorCoroutine);
			moveToCollectorCoroutine = null;
		}
		isTimedCollecting = false;
		if (rigidBody != null)
		{
			rigidBody.isKinematic = false;
		}
	}
}
