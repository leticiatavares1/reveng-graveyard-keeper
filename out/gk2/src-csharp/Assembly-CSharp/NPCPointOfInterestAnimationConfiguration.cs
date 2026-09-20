using System;
using UnityEngine;

[Serializable]
public class NPCPointOfInterestAnimationConfiguration
{
	[SerializeField]
	private string triggerId;

	[SerializeField]
	private float minDelay;

	[SerializeField]
	private float maxDelay;

	public string TriggerId => triggerId;

	public float MinDelay => minDelay;

	public float MaxDelay => maxDelay;
}
