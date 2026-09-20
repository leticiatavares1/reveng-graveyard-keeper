using System;

namespace DarkTonic.MasterAudio;

[Serializable]
public class OcclusionFreqChangeInfo
{
	public SoundGroupVariation ActingVariation;

	public float StartFrequency;

	public float TargetFrequency;

	public float StartTime;

	public float CompletionTime;

	public bool IsActive = true;
}
