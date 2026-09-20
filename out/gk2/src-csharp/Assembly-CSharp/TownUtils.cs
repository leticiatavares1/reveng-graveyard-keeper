using UnityEngine;

public static class TownUtils
{
	public static void RepairWholeTown()
	{
		foreach (WsoData wsoData in MainGame.Instance.GameSave.worldData.GetGameSceneDataById("RuinedTemple").wsoDataList)
		{
			RepairHouse(wsoData);
		}
	}

	public static int RepairHouse(WsoData wsoData, int stageIdx = -1)
	{
		if (!wsoData.Definition.ReplacementConfig)
		{
			return 0;
		}
		WsoRepairablePartData componentData = wsoData.GetComponentData<WsoRepairablePartData>();
		if (componentData == null)
		{
			Debug.LogWarning("[Wso] Cannot repair - no WsoRepairablePartData component on " + wsoData.id);
			return 0;
		}
		if (wsoData.Definition.ReplacementConfig == null)
		{
			Debug.LogWarning($"[Wso] Cannot repair - no replacement config found for {wsoData}");
			return 0;
		}
		int num = ((stageIdx == -1) ? componentData.RepairAllStages(wsoData.Definition.ReplacementConfig) : componentData.RepairStage(stageIdx, wsoData.Definition.ReplacementConfig));
		if (num > 0)
		{
			wsoData.NotifyRepairStateChanged();
			Debug.Log($"[Wso] Repaired {num} parts of {wsoData.id}");
		}
		if (componentData.AreAllStagesRepaired() && !componentData.isTownQualityAdded)
		{
			componentData.isTownQualityAdded = true;
			if (MainGame.Instance?.GameSave?.townSystem != null)
			{
				MainGame.Instance.GameSave.townSystem.Quality += wsoData.Definition.townQuality;
			}
		}
		return num;
	}
}
