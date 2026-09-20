using System;
using UnityEngine;

public class WorldFX : MonoBehaviour
{
	private Action onActionTicked;

	private Action onFinished;

	private ParticleSystem mainParticleSystem;

	private Transform followTarget;

	private bool isPlaying;

	private float playingTime;

	private bool actionWasExecuted;

	private Animator animator;

	[NonSerialized]
	public bool returnToPoolAfterEnd;

	[SerializeField]
	private float actionTime = 1f;

	[Space]
	[SerializeField]
	private bool useAnimator;

	[SerializeField]
	[HideInInspector]
	private float customDuration;

	private float Duration => MainParticleSystem?.main.duration ?? customDuration;

	public ParticleSystem MainParticleSystem
	{
		get
		{
			if (!mainParticleSystem)
			{
				TryGetComponent<ParticleSystem>(out mainParticleSystem);
			}
			return mainParticleSystem;
		}
	}

	public static WorldFX Spawn(Vector3 worldPos, string name, Action onAction = null, Vector3 size = default(Vector3))
	{
		if (size == default(Vector3))
		{
			size = Vector3.one;
		}
		WorldFX worldFX = WorldFXPool.Get(name);
		if (worldFX == null)
		{
			Debug.LogError("Error spawning WorldFX \"" + name + "\": Effect on found.");
			return null;
		}
		worldFX.name = name;
		worldFX.transform.position = worldPos;
		worldFX.transform.rotation = Quaternion.identity;
		worldFX.transform.localScale = size;
		worldFX.gameObject.SetActive(value: true);
		worldFX.Play(onAction);
		worldFX.returnToPoolAfterEnd = true;
		return worldFX;
	}

	public static WorldFX Spawn(Transform atTransform, string id, Action onAction = null, Vector3 size = default(Vector3))
	{
		if (atTransform == null)
		{
			Debug.LogError("Error spawning WorldFX: target Transform is null.");
			return null;
		}
		return Spawn(atTransform.position, id, onAction, size);
	}

	public void Play(Action onAction, Action onFinished = null)
	{
		if ((bool)MainParticleSystem)
		{
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem obj in componentsInChildren)
			{
				obj.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				obj.Simulate(0f, withChildren: true, restart: true);
				obj.Play();
			}
		}
		if (useAnimator && (bool)animator)
		{
			animator.Rebind();
			animator.Update(0f);
			animator.Play(0, -1, 0f);
		}
		this.onFinished = onFinished;
		onActionTicked = onAction;
		followTarget = null;
		playingTime = 0f;
		isPlaying = true;
		actionWasExecuted = false;
	}

	public void Follow(Transform target)
	{
		followTarget = target;
		SyncFollowTransform();
	}

	private void Stop()
	{
		if ((bool)MainParticleSystem)
		{
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		isPlaying = false;
	}

	private void OnDisable()
	{
		isPlaying = false;
		followTarget = null;
		onActionTicked = null;
		onFinished = null;
	}

	private void LateUpdate()
	{
		SyncFollowTransform();
	}

	private void SyncFollowTransform()
	{
		if (!(followTarget == null))
		{
			base.transform.SetPositionAndRotation(followTarget.position, followTarget.rotation);
		}
	}

	private void Update()
	{
		if (!isPlaying)
		{
			return;
		}
		playingTime += Time.deltaTime;
		if (!actionWasExecuted && playingTime > actionTime)
		{
			Action action = onActionTicked;
			onActionTicked = null;
			actionWasExecuted = true;
			action?.Invoke();
		}
		if (playingTime > Duration)
		{
			Stop();
			Action action2 = onFinished;
			onFinished = null;
			if (returnToPoolAfterEnd)
			{
				WorldFXPool.Release(base.name, this);
			}
			action2?.Invoke();
		}
	}
}
