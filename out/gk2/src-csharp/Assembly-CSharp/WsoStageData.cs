using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WsoStageData
{
	[Tooltip("Index of this stage in the Wso (based on hierarchy order)")]
	[SerializeField]
	private int stageIndex;

	[Tooltip("Whether all parts in this stage have been repaired")]
	[SerializeField]
	private bool isRepaired;

	[Tooltip("List of constructor part data within this stage")]
	[SerializeField]
	private List<ConstructorPartStateData> partsData = new List<ConstructorPartStateData>();

	public int StageIndex => stageIndex;

	public bool IsRepaired => isRepaired;

	public IReadOnlyList<ConstructorPartStateData> PartsData => partsData;

	public WsoStageData()
	{
	}

	public WsoStageData(int stageIndex)
	{
		this.stageIndex = stageIndex;
	}

	public void AddPartData(ConstructorPartStateData partData)
	{
		partsData.Add(partData);
	}

	public int RepairParts(ConstructorPartReplacementConfig config)
	{
		if (config == null || isRepaired)
		{
			return 0;
		}
		int num = 0;
		foreach (ConstructorPartStateData partsDatum in partsData)
		{
			if (!partsDatum.IsRepaired && !partsDatum.IsDeleted)
			{
				if (config.ShouldDeleteModel(partsDatum.OriginalModelId))
				{
					partsDatum.SetDeleted();
					num++;
				}
				else
				{
					partsDatum.SetRepaired();
					num++;
				}
			}
		}
		if (num > 0)
		{
			isRepaired = CheckAllPartsRepaired();
		}
		return num;
	}

	public void ResetParts()
	{
		foreach (ConstructorPartStateData partsDatum in partsData)
		{
			partsDatum.Reset();
		}
		isRepaired = false;
	}

	private bool CheckAllPartsRepaired()
	{
		foreach (ConstructorPartStateData partsDatum in partsData)
		{
			if (!partsDatum.IsRepaired && !partsDatum.IsDeleted)
			{
				return false;
			}
		}
		return partsData.Count > 0;
	}
}
