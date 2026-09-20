using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class FishingDef : BalanceBaseObject
{
	[AutoParse("fish_item")]
	public string fishId;

	[AutoParse("reservoir_id")]
	public string reservoirId;

	[AutoParse("curve_preset_id")]
	public string curvePresetId;

	public float[] intervalTimeRange = new float[2];

	public float[] resistTimeRange = new float[2];

	public float[] waitTimeRange = new float[2];

	[AutoParse("bait_time")]
	public float baitTime;

	[AutoParse("speed_change_time")]
	public float speedChangeTime;

	[AutoParse("anger")]
	public LazyExpression anger = new LazyExpression();

	[AutoParse("base_weight")]
	public int baseWeight;

	[AutoParse("bait_mod")]
	public GameRes baitMod;

	public float[] daytimeMod = new float[2];

	[AutoParse("base_count")]
	public int baseCount;

	[AutoParse("regen_time")]
	public float regenTime;

	[AutoParse("expression_on_end")]
	public List<LazyExpression> expressionsOnEnd;

	[AutoParse("underwater_gfx_type")]
	public FishGfxUnderwaterType underwaterGfxType = FishGfxUnderwaterType.Small;

	[AutoParse("splash_coef")]
	public float splashCoef = 1f;

	[AutoParse("wriggle_coef")]
	public float wriggleCoef = 1f;

	public float IntervalTimeLeft => intervalTimeRange[0];

	public float IntervalTimeRight => intervalTimeRange[1];

	public float ResistTimeLeft => resistTimeRange[0];

	public float ResistTimeRight => resistTimeRange[1];

	public float WaitTimeLeft => waitTimeRange[0];

	public float WaitTimeRight => waitTimeRange[1];

	public static List<FishingDef> GetAllForReservoir(string reservoirId)
	{
		return GameBalance.Me.fishingDefs.FindAll((FishingDef x) => x.reservoirId == reservoirId);
	}

	public static List<Item> GetAvailableBaits(List<Item> baits, List<FishingDef> fishingDefs)
	{
		List<Item> list = new List<Item>(baits);
		foreach (Item bait in baits)
		{
			bool flag = false;
			foreach (FishingDef fishingDef in fishingDefs)
			{
				if (fishingDef.baitMod != null && fishingDef.baitMod.List.Exists((GameResAtom x) => x.type == bait.id))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Remove(bait);
			}
		}
		return list;
	}

	public float GetDayTimeMod()
	{
		TimeOfDayType timeOfDayType = EnvironmentEngine.Instance.GetTimeOfDayType();
		switch (timeOfDayType)
		{
		case TimeOfDayType.Day:
			return daytimeMod[0];
		case TimeOfDayType.Night:
			return daytimeMod[1];
		default:
			Debug.LogError(string.Format("[{0}]: Unknown time of day type: {1}", "FishingDef", timeOfDayType));
			return 0f;
		}
	}
}
