using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class WgoPartState
{
	public string variationId;

	public int rotationIndex;

	public GameObject gameObject;

	public bool mirror;

	public bool isDefault;

	public Transform customBubblePoint;

	public List<UnityEvent> customEvents;
}
