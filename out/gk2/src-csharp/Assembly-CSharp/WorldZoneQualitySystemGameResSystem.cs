using LazyBearTechnology;
using UnityEngine;

public class WorldZoneQualitySystemGameResSystem : GK2GameResSystem
{
	private string worldZoneId;

	public WorldZoneQualitySystemGameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
		worldZoneId = gameResAtomName.Replace("wz_", "");
	}

	public override float Get()
	{
		string text = worldZoneId;
		if (!(text == "town"))
		{
			if (text == "conveyor")
			{
				float value = Mathf.Min(MainGame.Instance.conveyorSystem.ConveyorWorldZone.GetTotalQuality(WorldZoneWgoQualityType.ConveyorPowerSource), MainGame.Instance.conveyorSystem.ConveyorWorldZone.GetTotalQuality(WorldZoneWgoQualityType.ConveyorCells));
				GK2GameResSystem.PlayerData.SetResWithoutSystemsCheck(gameResAtomName, value);
			}
			else
			{
				WorldZoneData worldZoneDataById = MainGame.Instance.GameSave.worldData.GetWorldZoneDataById(worldZoneId);
				float withoutSystemsCheck = resForChanges.GetWithoutSystemsCheck(gameResAtomName);
				float totalQuality = worldZoneDataById.GetTotalQuality();
				GK2GameResSystem.PlayerData.SetResWithoutSystemsCheck(gameResAtomName, totalQuality);
				if (!Mathf.Approximately(withoutSystemsCheck, totalQuality))
				{
					TryUnlockGraveyardQualityAchievement(totalQuality);
				}
			}
		}
		else
		{
			GK2GameResSystem.PlayerData.SetResWithoutSystemsCheck(gameResAtomName, MainGame.Instance.GameSave.townSystem.Quality);
		}
		return base.Get();
	}

	public override void Add(float value, bool silent = false)
	{
		if (worldZoneId == "town")
		{
			MainGame.Instance.GameSave.townSystem.Quality += (int)value;
		}
		else
		{
			base.Add(value, silent);
		}
	}

	private void TryUnlockGraveyardQualityAchievement(float totalQuality)
	{
		if (worldZoneId == "graveyard" && totalQuality >= 200f)
		{
			AchievementsSystem.Instance.Unlock("ach_graveyard_quality_200");
		}
	}
}
