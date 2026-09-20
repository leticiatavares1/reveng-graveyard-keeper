using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCPointOfInterestConfiguration
{
	[SerializeField]
	private string id;

	[SerializeField]
	private bool enabledByDefault = true;

	[SerializeField]
	private NPCPointOfInterestAnimationType animationType;

	[SerializeField]
	private List<NPCPointOfInterestAnimationConfiguration> animationsForRoll = new List<NPCPointOfInterestAnimationConfiguration>();

	[SerializeField]
	private string idleTriggerId;

	[SerializeField]
	private float startWeight = 10f;

	public string Id => id;

	public bool EnabledByDefault => enabledByDefault;

	public float StartWeight => startWeight;

	public List<NPCPointOfInterestAnimationConfiguration> AnimationsForRoll => animationsForRoll;

	public NPCPointOfInterestAnimationType AnimationType => animationType;

	public string IdleTriggerId => idleTriggerId;
}
