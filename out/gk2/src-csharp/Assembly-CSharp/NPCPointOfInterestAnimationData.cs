using System;
using UnityEngine;

[Serializable]
public class NPCPointOfInterestAnimationData
{
	[SerializeField]
	private SGuid wgoId;

	[SerializeField]
	private float remainingTimeToRoll;

	public float RemainingTimeToRoll
	{
		get
		{
			return remainingTimeToRoll;
		}
		set
		{
			remainingTimeToRoll = value;
		}
	}

	public SGuid WgoId => wgoId;

	public NPCPointOfInterestAnimationData(WgoData wgoData, NPCPointOfInterestAnimationConfiguration configuration)
	{
		wgoId = wgoData.UniqueId;
		remainingTimeToRoll = UnityEngine.Random.Range(configuration.MinDelay, configuration.MaxDelay);
	}
}
