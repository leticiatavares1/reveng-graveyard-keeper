using System;
using UnityEngine;

public class GDPointTeleportData : TeleportDataBase
{
	private GDPointData gdPoint;

	private string gdPointStr;

	private bool isTag;

	public GDPointData GDPointData
	{
		get
		{
			if (gdPoint != null)
			{
				return gdPoint;
			}
			if (!isTag)
			{
				return MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(gdPointStr);
			}
			return MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataByCustomTag(gdPointStr);
		}
	}

	public GDPointTeleportData(GDPointData gdPoint, string environmentPreset = "outdoor", string soundOnTeleport = "", Action onGameSceneLoaded = null, bool donNotFade = false, float delayInFade = 0.3f)
		: base(environmentPreset, soundOnTeleport, onGameSceneLoaded, donNotFade, delayInFade)
	{
		this.gdPoint = gdPoint;
	}

	public GDPointTeleportData(string gdPointStr, bool isTag = false, string environmentPreset = "outdoor", string soundOnTeleport = "", Action onGameSceneLoaded = null, bool donNotFade = false, float delayInFade = 0.3f)
		: base(environmentPreset, soundOnTeleport, onGameSceneLoaded, donNotFade, delayInFade)
	{
		this.gdPointStr = gdPointStr;
		this.isTag = isTag;
	}

	public override string GetDestinationId()
	{
		return GDPointData.Id;
	}

	public override GameSceneData GetDestinationSceneData()
	{
		return MainGame.Instance.GameSave.worldData.GetGameSceneDataById(GDPointData.GameSceneDataId);
	}

	public override Vector3 GetPosition()
	{
		return GDPointData.Position;
	}
}
