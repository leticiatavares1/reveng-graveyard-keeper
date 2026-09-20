using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class EnvironmentControlBehaviour : PlayableBehaviour
{
	public bool setTime;

	[Range(0f, 1f)]
	public float timeOfDay;

	private bool isInitialStateSaved;

	private float initialTimeOfDay;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!(EnvironmentEngine.Instance == null))
		{
			if (!isInitialStateSaved)
			{
				isInitialStateSaved = true;
				initialTimeOfDay = EnvironmentEngine.Instance.timeOfDay;
			}
			if (setTime)
			{
				EnvironmentEngine.Instance.SetTimeOfDay(timeOfDay);
			}
		}
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		if (!Application.isPlaying && !(EnvironmentEngine.Instance == null) && isInitialStateSaved)
		{
			EnvironmentEngine.Instance.SetTimeOfDay(initialTimeOfDay);
			isInitialStateSaved = false;
		}
	}
}
