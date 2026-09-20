using System;

namespace DarkTonic.MasterAudio;

[Serializable]
public class BusFadeInfo
{
	public string NameOfBus;

	public GroupBus ActingBus;

	public float StartVolume;

	public float TargetVolume;

	public float StartTime;

	public float CompletionTime;

	public bool IsActive = true;

	public bool WillStopGroupAfterFade;

	public bool WillResetVolumeAfterFade;

	public Action completionAction;
}
