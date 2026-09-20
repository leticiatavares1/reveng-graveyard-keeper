using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WsoRepairablePartData : WsoComponentDataBase
{
	[Tooltip("List of repairable stages within this Wso")]
	[SerializeField]
	private List<WsoStageData> stages = new List<WsoStageData>();

	public bool isTownQualityAdded;

	public IReadOnlyList<WsoStageData> Stages => stages;

	public void AddStage(WsoStageData stageData)
	{
		stages.Add(stageData);
	}

	public WsoStageData GetStage(int stageIndex)
	{
		if (stageIndex < 0 || stageIndex >= stages.Count)
		{
			return null;
		}
		return stages[stageIndex];
	}

	public int GetTotalPartsCount()
	{
		int num = 0;
		foreach (WsoStageData stage in stages)
		{
			num += stage.PartsData.Count;
		}
		return num;
	}

	public bool AreAllStagesRepaired()
	{
		foreach (WsoStageData stage in stages)
		{
			if (!stage.IsRepaired)
			{
				return false;
			}
		}
		return stages.Count > 0;
	}

	public bool IsAnyStageRepaired()
	{
		foreach (WsoStageData stage in stages)
		{
			if (stage.IsRepaired)
			{
				return true;
			}
		}
		return false;
	}

	public int RepairStage(int stageIndex, ConstructorPartReplacementConfig config)
	{
		if (config == null)
		{
			return 0;
		}
		WsoStageData stage = GetStage(stageIndex);
		if (stage == null)
		{
			Debug.LogWarning($"[WsoRepairablePartData] Stage index '{stageIndex}' not found");
			return 0;
		}
		return stage.RepairParts(config);
	}

	public int RepairAllStages(ConstructorPartReplacementConfig config)
	{
		if (config == null)
		{
			return 0;
		}
		int num = 0;
		foreach (WsoStageData stage in stages)
		{
			num += stage.RepairParts(config);
		}
		return num;
	}

	public void ResetStage(int stageIndex)
	{
		GetStage(stageIndex)?.ResetParts();
	}

	public void ResetAllStages()
	{
		foreach (WsoStageData stage in stages)
		{
			stage.ResetParts();
		}
	}

	public void ClearStages()
	{
		stages.Clear();
	}
}
