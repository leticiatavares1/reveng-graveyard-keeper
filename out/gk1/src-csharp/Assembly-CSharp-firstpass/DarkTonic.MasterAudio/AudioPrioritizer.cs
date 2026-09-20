using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class AudioPrioritizer
{
	private const int MaxPriority = 0;

	private const int HighestPriority = 16;

	private const int LowestPriority = 128;

	public static void Set2DSoundPriority(AudioSource audio)
	{
		audio.priority = 16;
	}

	public static void SetSoundGroupInitialPriority(AudioSource audio)
	{
		audio.priority = 128;
	}

	public static void SetPreviewPriority(AudioSource audio)
	{
		audio.priority = 0;
	}

	public static void Set3DPriority(SoundGroupVariation variation, bool useClipAgePriority)
	{
		if (MasterAudio.ListenerTrans == null)
		{
			return;
		}
		AudioSource varAudio = variation.VarAudio;
		if (varAudio.spatialBlend == 0f)
		{
			Set2DSoundPriority(variation.VarAudio);
			return;
		}
		float num = Vector3.Distance(varAudio.transform.position, MasterAudio.ListenerTrans.position);
		float num2 = varAudio.rolloffMode switch
		{
			AudioRolloffMode.Logarithmic => varAudio.volume / Mathf.Max(varAudio.minDistance, num - varAudio.minDistance), 
			AudioRolloffMode.Linear => Mathf.Lerp(varAudio.volume, 0f, Mathf.Max(0f, num - varAudio.minDistance) / (varAudio.maxDistance - varAudio.minDistance)), 
			_ => Mathf.Lerp(varAudio.volume, 0f, Mathf.Max(0f, num - varAudio.minDistance) / (varAudio.maxDistance - varAudio.minDistance)), 
		};
		if (useClipAgePriority && !varAudio.loop)
		{
			num2 = Mathf.Lerp(num2, num2 * 0.1f, AudioUtil.GetAudioPlayedPercentage(varAudio) * 0.01f);
		}
		varAudio.priority = (int)Mathf.Lerp(16f, 128f, Mathf.InverseLerp(1f, 0f, num2));
	}
}
