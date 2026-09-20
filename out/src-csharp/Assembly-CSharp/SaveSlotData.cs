using System;
using UnityEngine;

[Serializable]
public class SaveSlotData
{
	[NonSerialized]
	public string filename_no_extension;

	[NonSerialized]
	public GameSave linked_save;

	public string stats;

	public string real_time;

	public float game_time;

	public float version;

	public static SaveSlotData FromJSON(string s)
	{
		return JsonUtility.FromJson<SaveSlotData>(s);
	}

	public string ToJSON()
	{
		return JsonUtility.ToJson(this);
	}

	public void PrepareForSave()
	{
		game_time = MainGame.game_time;
		real_time = DateTime.Now.ToString("HH:mm, dd MMM yyyy");
		float totalQuality = WorldZone.GetZoneByID("church").GetTotalQuality();
		float totalQuality2 = WorldZone.GetZoneByID("graveyard").GetTotalQuality();
		stats = $"(wskull){totalQuality2:0.#}  (cross){totalQuality:0.#}";
		version = LazyConsts.VERSION;
		if (game_time > 1.5f)
		{
			PlatformSpecific.SetGameMetric(GameEvents.GameMetric.DaysInGame, game_time - 1.5f);
			PlatformSpecific.SetGameMetric(GameEvents.GameMetric.GraveyardQuality, totalQuality2);
			PlatformSpecific.SetGameMetric(GameEvents.GameMetric.ChurchQuality, totalQuality);
		}
	}

	public bool IsBinaryFormat()
	{
		return true;
	}
}
