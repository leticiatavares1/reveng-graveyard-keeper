using System;
using UnityEngine;

public abstract class TeleportDataBase
{
	public string environmentPreset;

	public string soundOnTeleport;

	public bool donNotFade;

	public Action onGameSceneLoaded;

	public float delayInFade;

	public TeleportDataBase(string environmentPreset = "outdoor", string soundOnTeleport = "", Action onGameSceneLoaded = null, bool donNotFade = false, float delayInFade = 0.3f)
	{
		this.environmentPreset = environmentPreset;
		this.soundOnTeleport = soundOnTeleport;
		this.onGameSceneLoaded = onGameSceneLoaded;
		this.donNotFade = donNotFade;
		this.delayInFade = delayInFade;
	}

	public abstract string GetDestinationId();

	public abstract GameSceneData GetDestinationSceneData();

	public abstract Vector3 GetPosition();

	public virtual bool CanTeleport(out string error)
	{
		error = string.Empty;
		return true;
	}
}
