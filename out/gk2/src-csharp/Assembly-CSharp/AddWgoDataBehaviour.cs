using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class AddWgoDataBehaviour : PlayableBehaviour
{
	public string wgoId;

	public string customTag;

	public bool useBindingPosition = true;

	public Vector3 position;

	public string gameSceneId;

	private bool hasFired;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!hasFired && !(info.effectiveWeight <= 0f))
		{
			Transform transform = playerData as Transform;
			Vector3 vector = ((useBindingPosition && transform != null) ? transform.position : position);
			string value = gameSceneId;
			if (string.IsNullOrEmpty(value))
			{
				value = ((MainGame.PlayerData == null || string.IsNullOrEmpty(MainGame.PlayerData.currentGameSceneId)) ? MainGame.EntrySceneToLoad : MainGame.PlayerData.currentGameSceneId);
			}
			if (!string.IsNullOrEmpty(wgoId))
			{
				MainGame.WorldData.AddWgoData(wgoId, vector, value, customTag, out var _);
			}
			hasFired = true;
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		hasFired = false;
	}
}
