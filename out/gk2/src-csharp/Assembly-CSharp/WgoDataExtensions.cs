using LazyBearTechnology;
using UnityEngine;

public static class WgoDataExtensions
{
	private const float COLLIDER_BOX_SEARCH_HALF_SIZE = 10f;

	public static bool TryGetNearestBuilderWorldZone(this WgoData builderWgoData, out WorldZoneData worldZoneData)
	{
		worldZoneData = null;
		if (builderWgoData == null || string.IsNullOrEmpty(builderWgoData.WorldId))
		{
			return false;
		}
		GameSceneData gameSceneDataById = MainGame.Instance.GameSave.WorldData.GetGameSceneDataById(builderWgoData.WorldId);
		if (gameSceneDataById == null)
		{
			return false;
		}
		Vector2 point = builderWgoData.Position.XZ2();
		Rect other = new Rect(point.x - 10f, point.y - 10f, 20f, 20f);
		float num = float.PositiveInfinity;
		WorldZoneData worldZoneData2 = null;
		for (int i = 0; i < gameSceneDataById.worldZones.Count; i++)
		{
			WorldZoneData worldZoneData3 = gameSceneDataById.worldZones[i];
			if (!IsBuilderForWorldZone(builderWgoData, worldZoneData3))
			{
				continue;
			}
			Rect wholeZoneRect = worldZoneData3.wholeZoneRect;
			if (wholeZoneRect.Overlaps(other, allowInverse: true))
			{
				if (wholeZoneRect.Contains(point))
				{
					worldZoneData = worldZoneData3;
					return true;
				}
				float rectSqrDistance = GetRectSqrDistance(wholeZoneRect, point);
				if (num > rectSqrDistance)
				{
					num = rectSqrDistance;
					worldZoneData2 = worldZoneData3;
				}
			}
		}
		if (worldZoneData2 == null)
		{
			return false;
		}
		worldZoneData = worldZoneData2;
		return true;
	}

	private static bool IsBuilderForWorldZone(WgoData builderWgoData, WorldZoneData worldZoneData)
	{
		if (builderWgoData == null || worldZoneData == null)
		{
			return false;
		}
		WorldZoneDef worldZoneDef = worldZoneData.Definition ?? GameBalance.Me.GetDataOrNull<WorldZoneDef>(worldZoneData.id);
		if (worldZoneDef != null && !string.IsNullOrEmpty(worldZoneDef.builderId))
		{
			return worldZoneDef.builderId == builderWgoData.id;
		}
		return false;
	}

	private static float GetRectSqrDistance(Rect rect, Vector2 point)
	{
		float num = Mathf.Clamp(point.x, rect.xMin, rect.xMax);
		float num2 = Mathf.Clamp(point.y, rect.yMin, rect.yMax);
		float num3 = point.x - num;
		float num4 = point.y - num2;
		return num3 * num3 + num4 * num4;
	}

	public static void StoreSermonResult(this WgoData data, SermonResultData sermonResultData)
	{
		data.AddGameRes("money", sermonResultData.Money);
		bool flag = false;
		foreach (InteractionEvent @event in data.Events)
		{
			if (@event.str == "sermon_reward")
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			data.AddInteractionEvent("sermon_reward");
		}
	}

	public static void AddMoneyToPlayerAndClearSermonResult(this WgoData data)
	{
		MainGame.PlayerData.AddRes("money", data.GetGameResInt("money"));
		LazyAudio.PlayAndForget("coins_sound");
		data.SetGameRes("money", 0f);
	}

	public static void StorePaletteTradingResult(this WgoData data, int totalMoney)
	{
		data.AddGameRes("money", totalMoney);
		bool flag = false;
		foreach (InteractionEvent @event in data.Events)
		{
			if (@event.str == "palette_trading_reward")
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			data.AddInteractionEvent("palette_trading_reward");
		}
	}

	public static void AddMoneyToPlayerAndPaletteTradingResult(this WgoData data)
	{
		MainGame.PlayerData.AddRes("money", data.GetGameResInt("money"));
		LazyAudio.PlayAndForget("coins_sound");
		data.SetGameRes("money", 0f);
	}
}
