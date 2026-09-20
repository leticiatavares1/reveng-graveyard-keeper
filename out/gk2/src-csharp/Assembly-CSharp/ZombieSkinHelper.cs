using UnityEngine;

public static class ZombieSkinHelper
{
	public const string ZOMBIE_WORKER_DATA_ID = "zombie_worker";

	public const string ZOMBIE_ASSISTANT_DATA_ID = "zombie_assistant";

	public static SkinPresetGK2 GetPresetForCustomizationData(string dataId, int body, int head, string bodyLut, string headLut)
	{
		SkinPresetGK2 skinPresetGK = ScriptableObject.CreateInstance<SkinPresetGK2>();
		skinPresetGK.body = new SkinPresetPartGK2
		{
			id = body,
			colorReplaceType = ColorReplaceType.USE_LUT_COLOR_REPLACE,
			palette = (string.IsNullOrEmpty(bodyLut) ? null : ZombieCustomizationConfig.GetBodyTextureByName(dataId, bodyLut))
		};
		skinPresetGK.head = new SkinPresetPartGK2
		{
			id = head,
			colorReplaceType = ColorReplaceType.USE_LUT_COLOR_REPLACE,
			palette = (string.IsNullOrEmpty(headLut) ? null : ZombieCustomizationConfig.GetHeadTextureByName(dataId, headLut))
		};
		skinPresetGK.arms = new SkinPresetPartGK2();
		skinPresetGK.beard = new SkinPresetPartGK2();
		skinPresetGK.hairstyle = new SkinPresetPartGK2();
		return skinPresetGK;
	}

	public static SkinPresetGK2 GetPresetForWgoData(WgoData wgoData, string dataId)
	{
		return GetPresetForCustomizationData(dataId, wgoData.GetGameResInt("zombie_body_id"), wgoData.GetGameResInt("zombie_head_id"), wgoData.GameResStr.Get("zombie_body_lut"), wgoData.GameResStr.Get("zombie_head_lut"));
	}

	public static SkinPresetGK2 CopySkinPresetAndChangeHead(SkinPresetGK2 source, WgoData wgoData, string dataId)
	{
		SkinPresetGK2 skinPresetGK = ScriptableObject.CreateInstance<SkinPresetGK2>();
		skinPresetGK.body = source.body;
		skinPresetGK.hairstyle = source.hairstyle;
		skinPresetGK.arms = source.arms;
		skinPresetGK.beard = source.beard;
		int gameResInt = wgoData.GetGameResInt("zombie_head_id");
		string text = wgoData.GameResStr.Get("zombie_head_lut");
		skinPresetGK.head = new SkinPresetPartGK2
		{
			id = gameResInt,
			colorReplaceType = ColorReplaceType.USE_LUT_COLOR_REPLACE,
			palette = (string.IsNullOrEmpty(text) ? null : ZombieCustomizationConfig.GetHeadTextureByName(dataId, text))
		};
		return skinPresetGK;
	}

	public static (int body, int head, string bodyLut, string headLut) RollZombie(string dataId)
	{
		return (body: ZombieCustomizationConfig.GetRandomBody(dataId), head: ZombieCustomizationConfig.GetRandomHead(dataId), bodyLut: ZombieCustomizationConfig.GetRandomBodyLut(dataId)?.name, headLut: ZombieCustomizationConfig.GetRandomHeadLut(dataId)?.name);
	}

	public static void RollAndApplyZombieSkinToWgoData(WgoData wgoData, string dataId)
	{
		(int, int, string, string) tuple = RollZombie(dataId);
		ApplySkinToZombieWgoData(wgoData, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
	}

	public static void ApplySkinToZombieWgoData(WgoData wgoData, int body, int head, string bodyLut, string headLut)
	{
		wgoData.SetGameRes("zombie_body_id", body);
		wgoData.SetGameRes("zombie_head_id", head);
		wgoData.GameResStr.Set("zombie_body_lut", bodyLut);
		wgoData.GameResStr.Set("zombie_head_lut", headLut);
	}
}
