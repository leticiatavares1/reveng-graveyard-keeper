using System;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "VoiceClipData", menuName = "Lazy/VoiceClipData", order = 1)]
public class VoiceClipData : ScriptableObject
{
	public float startSilence;

	public float endSilence;

	public LoudInterval[] loudIntervals = Array.Empty<LoudInterval>();
}
