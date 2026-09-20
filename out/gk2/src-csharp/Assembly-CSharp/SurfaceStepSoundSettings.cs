using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceStepSoundSettings", menuName = "GK2/SurfaceStepSoundSettings", order = 0)]
public class SurfaceStepSoundSettings : LazySingletonSO<SurfaceStepSoundSettings>
{
	[Serializable]
	private class StepSoundPair
	{
		public SurfaceType surfaceType;

		public string soundId;
	}

	[Range(0f, 100f)]
	public int delayChance = 50;

	public float maxDelayTime = 0.04f;

	public float minDelayTime = 0.01f;

	[SerializeField]
	[Space]
	private List<StepSoundPair> soundPairs;

	public string GetStepSoundIdForSurfaceType(SurfaceType surfaceType)
	{
		StepSoundPair stepSoundPair = soundPairs.Find((StepSoundPair p) => p.surfaceType == surfaceType);
		if (stepSoundPair == null)
		{
			Debug.LogError($"Can't find sound for surface type:[{surfaceType}]");
			return string.Empty;
		}
		return stepSoundPair.soundId;
	}
}
