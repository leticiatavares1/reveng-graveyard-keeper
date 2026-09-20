using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class AudioTransformExtensions
{
	public static void FadeOutSoundGroupOfTransform(this Transform sourceTrans, string sType, float fadeTime)
	{
		MasterAudio.FadeOutSoundGroupOfTransform(sourceTrans, sType, fadeTime);
	}

	public static List<SoundGroupVariation> GetAllPlayingVariationsOfTransform(this Transform sourceTrans)
	{
		return MasterAudio.GetAllPlayingVariationsOfTransform(sourceTrans);
	}

	public static bool PlaySound3DAtTransformAndForget(this Transform sourceTrans, string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null)
	{
		return MasterAudio.PlaySound3DAtTransformAndForget(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
	}

	public static PlaySoundResult PlaySound3DAtTransform(this Transform sourceTrans, string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null)
	{
		return MasterAudio.PlaySound3DAtTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
	}

	public static bool PlaySound3DFollowTransformAndForget(this Transform sourceTrans, string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null)
	{
		return MasterAudio.PlaySound3DFollowTransformAndForget(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
	}

	public static PlaySoundResult PlaySound3DFollowTransform(this Transform sourceTrans, string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null)
	{
		return MasterAudio.PlaySound3DFollowTransform(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName);
	}

	public static IEnumerator PlaySound3DAtTransformAndWaitUntilFinished(this Transform sourceTrans, string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, Action completedAction = null)
	{
		return MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay, completedAction);
	}

	public static IEnumerator PlaySound3DFollowTransformAndWaitUntilFinished(this Transform sourceTrans, string sType, float volumePercentage = 1f, float? pitch = null, float delaySoundTime = 0f, string variationName = null, double? timeToSchedulePlay = null, Action completedAction = null)
	{
		return MasterAudio.PlaySound3DFollowTransformAndWaitUntilFinished(sType, sourceTrans, volumePercentage, pitch, delaySoundTime, variationName, timeToSchedulePlay, completedAction);
	}

	public static void PauseAllSoundsOfTransform(Transform sourceTrans)
	{
		MasterAudio.PauseAllSoundsOfTransform(sourceTrans);
	}

	public static void PauseBusOfTransform(this Transform sourceTrans, string busName)
	{
		MasterAudio.PauseBusOfTransform(sourceTrans, busName);
	}

	public static void PauseSoundGroupOfTransform(this Transform sourceTrans, string sType)
	{
		MasterAudio.PauseSoundGroupOfTransform(sourceTrans, sType);
	}

	public static void StopAllSoundsOfTransform(this Transform sourceTrans)
	{
		MasterAudio.StopAllSoundsOfTransform(sourceTrans);
	}

	public static void StopBusOfTransform(this Transform sourceTrans, string busName)
	{
		MasterAudio.StopBusOfTransform(sourceTrans, busName);
	}

	public static void StopSoundGroupOfTransform(this Transform sourceTrans, string sType)
	{
		MasterAudio.StopSoundGroupOfTransform(sourceTrans, sType);
	}

	public static void UnpauseAllSoundsOfTransform(this Transform sourceTrans)
	{
		MasterAudio.UnpauseAllSoundsOfTransform(sourceTrans);
	}

	public static void UnpauseBusOfTransform(this Transform sourceTrans, string busName)
	{
		MasterAudio.UnpauseBusOfTransform(sourceTrans, busName);
	}

	public static void UnpauseSoundGroupOfTransform(this Transform sourceTrans, string sType)
	{
		MasterAudio.UnpauseSoundGroupOfTransform(sourceTrans, sType);
	}

	public static bool IsTransformPlayingSoundGroup(this Transform sourceTrans, string sType)
	{
		return MasterAudio.IsTransformPlayingSoundGroup(sType, sourceTrans);
	}
}
