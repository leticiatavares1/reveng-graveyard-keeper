using System;
using UnityEngine;

public class WgoTeleportData : TeleportDataBase
{
	public string wgoIdTeleportTo;

	public bool teleportToDockPoint;

	private WgoData wgoData;

	private GameSceneData gameSceneData;

	public WgoData WgoData
	{
		get
		{
			if (wgoData == null)
			{
				MainGame.Instance.GameSave.worldData.TryGetWgoData(wgoIdTeleportTo, out wgoData, out gameSceneData);
			}
			return wgoData;
		}
	}

	public WgoTeleportData(string wgoIdTeleportTo, string environmentPreset = "outdoor", string soundOnTeleport = "", bool teleportToDockPoint = false, Action onGameSceneLoaded = null, bool donNotFade = false, float delayInFade = 0.3f)
		: base(environmentPreset, soundOnTeleport, onGameSceneLoaded, donNotFade, delayInFade)
	{
		this.wgoIdTeleportTo = wgoIdTeleportTo;
		this.teleportToDockPoint = teleportToDockPoint;
	}

	public override string GetDestinationId()
	{
		return wgoIdTeleportTo;
	}

	public override GameSceneData GetDestinationSceneData()
	{
		if (gameSceneData == null)
		{
			MainGame.Instance.GameSave.worldData.TryGetWgoData(wgoIdTeleportTo, out wgoData, out gameSceneData);
		}
		return gameSceneData;
	}

	public override Vector3 GetPosition()
	{
		if (WgoData == null)
		{
			return default(Vector3);
		}
		if (!teleportToDockPoint)
		{
			return wgoData.GetTeleportPointPosition();
		}
		return wgoData.GetFirstDockPointDataWorldPosition();
	}

	public override bool CanTeleport(out string error)
	{
		error = string.Empty;
		if (WgoData == null)
		{
			error = "Can't teleport to " + wgoIdTeleportTo + ", wgoData not found";
			return false;
		}
		return true;
	}
}
