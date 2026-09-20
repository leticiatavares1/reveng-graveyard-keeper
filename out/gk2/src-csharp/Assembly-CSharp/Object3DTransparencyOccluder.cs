using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using UnityEngine;

public class Object3DTransparencyOccluder
{
	private class TweenContainer
	{
		public Tween tween;

		public bool isPlayingBackwards;
	}

	private const float TRANSPARENCY_TWEEN_DURATION = 0.25f;

	private const int MAX_OCCLUDER_HITS = 10;

	private readonly HashSet<Object3D> frameOccluders = new HashSet<Object3D>(10);

	private readonly HashSet<Object3D> playingOccluders = new HashSet<Object3D>(10);

	private readonly Dictionary<Object3D, TweenContainer> playingTweens = new Dictionary<Object3D, TweenContainer>(10);

	public static Object3DTransparencyOccluder Shared { get; } = new Object3DTransparencyOccluder();


	public void BeginFrame()
	{
		frameOccluders.Clear();
	}

	public void AddOccludersFromCapsule(Vector3 p1, Vector3 p2, float capsuleRadius, float distance)
	{
		NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(10, Allocator.TempJob);
		NativeArray<CapsulecastCommand> commands = new NativeArray<CapsulecastCommand>(1, Allocator.TempJob);
		QueryParameters queryParameters = new QueryParameters(32768, hitMultipleFaces: true, QueryTriggerInteraction.Ignore, hitBackfaces: true);
		Vector3 direction = Quaternion.Euler(53.130104f, 0f, 0f) * Vector3.back;
		CapsulecastCommand value = new CapsulecastCommand(p1, p2, capsuleRadius, direction, queryParameters, distance);
		commands[0] = value;
		CapsulecastCommand.ScheduleBatch(commands, results, 10, 10).Complete();
		for (int i = 0; i < 10; i++)
		{
			RaycastHit raycastHit = results[i];
			if (!(raycastHit.collider == null))
			{
				Object3D componentInParent = raycastHit.collider.gameObject.GetComponentInParent<Object3D>();
				if (componentInParent != null)
				{
					frameOccluders.Add(componentInParent);
				}
			}
		}
		results.Dispose();
		commands.Dispose();
	}

	public void EndFrame()
	{
		foreach (Object3D obj in frameOccluders)
		{
			if (obj == null)
			{
				continue;
			}
			TweenContainer value;
			if (!playingOccluders.Contains(obj))
			{
				TweenContainer tweenContainer = new TweenContainer();
				obj.TransparencyValue = 0f;
				Tween tween = DOTween.To(() => obj.TransparencyValue, delegate(float v)
				{
					obj.TransparencyValue = v;
				}, 1f, 0.25f).SetEase(Ease.OutExpo).SetAutoKill(autoKillOnCompletion: false)
					.OnRewind(delegate
					{
						playingTweens.Remove(obj);
						playingOccluders.Remove(obj);
					});
				tweenContainer.tween = tween;
				playingTweens[obj] = tweenContainer;
				playingOccluders.Add(obj);
			}
			else if (playingTweens.TryGetValue(obj, out value) && value.isPlayingBackwards)
			{
				value.tween.PlayForward();
				value.isPlayingBackwards = false;
			}
		}
		foreach (Object3D playingOccluder in playingOccluders)
		{
			if (!(playingOccluder == null) && !frameOccluders.Contains(playingOccluder) && playingTweens.TryGetValue(playingOccluder, out var value2))
			{
				value2.tween.PlayBackwards();
				value2.isPlayingBackwards = true;
			}
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<Object3D, TweenContainer> playingTween in playingTweens)
		{
			if (playingTween.Value?.tween != null && playingTween.Value.tween.IsActive())
			{
				playingTween.Value.tween.Kill();
			}
		}
		foreach (Object3D playingOccluder in playingOccluders)
		{
			if (playingOccluder != null)
			{
				playingOccluder.TransparencyValue = 0f;
			}
		}
		playingTweens.Clear();
		playingOccluders.Clear();
		frameOccluders.Clear();
	}
}
