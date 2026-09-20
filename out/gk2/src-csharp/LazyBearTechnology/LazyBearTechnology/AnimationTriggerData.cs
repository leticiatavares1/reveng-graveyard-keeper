using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class AnimationTriggerData
{
	public string triggerName;

	[Range(1f, 100f)]
	public int weight;
}
