using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Audio;

public class ModdedVoiceOverPlayer : VoiceOverPlayer
{
	private bool ownsModAudioClip;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void RegisterFactory()
	{
		LazyAudio.VoiceOverPlayerFactory = () => new ModdedVoiceOverPlayer(LazyAudio.VoiceOverAudioMixerGroup);
	}

	public ModdedVoiceOverPlayer(AudioMixerGroup outputAudioMixerGroup)
		: base(outputAudioMixerGroup)
	{
	}

	public override void Play(string id, VoiceID voiceId = null)
	{
		if (VoiceOverSettings.IsEnabled && !VoiceOverPlayer.IsMuted(id))
		{
			Stop();
			if (VoiceOverModLoader.TryLoadClip(id, out audioClip))
			{
				ownsModAudioClip = true;
				voiceClipData = null;
				currentLocalKey = id;
				currentVoiceId = voiceId;
				PlayLoadedClip();
			}
			else
			{
				base.Play(id, voiceId);
			}
		}
	}

	public override void Stop()
	{
		AudioClip audioClip = (ownsModAudioClip ? base.audioClip : null);
		base.Stop();
		if (audioClip != null)
		{
			VoiceOverModLoader.ReleaseClip(audioClip);
		}
		ownsModAudioClip = false;
	}
}
