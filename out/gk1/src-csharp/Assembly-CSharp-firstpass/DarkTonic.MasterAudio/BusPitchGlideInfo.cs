using System;
using System.Collections.Generic;

namespace DarkTonic.MasterAudio;

[Serializable]
public class BusPitchGlideInfo
{
	public string NameOfBus;

	public float CompletionTime;

	public bool IsActive = true;

	public List<SoundGroupVariation> GlidingVariations;

	public Action completionAction;
}
