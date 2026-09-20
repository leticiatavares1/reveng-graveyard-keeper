using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class WorldFXSpawnBehaviour : PlayableBehaviour
{
	public string id;

	public bool attachToTarget;

	private bool hasSpawned;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		Transform transform = playerData as Transform;
		if (!(transform == null) && !hasSpawned && info.effectiveWeight > 0f)
		{
			WorldFX.Spawn(transform, id);
			hasSpawned = true;
		}
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		hasSpawned = false;
	}
}
