using System;
using System.Collections.Generic;

namespace DarkTonic.MasterAudio;

[Serializable]
public class GroupPitchGlideInfo
{
	public MasterAudioGroup ActingGroup;

	public string NameOfGroup;

	public float CompletionTime;

	public bool IsActive = true;

	public List<SoundGroupVariation> GlidingVariations;

	public Action completionAction;
}
