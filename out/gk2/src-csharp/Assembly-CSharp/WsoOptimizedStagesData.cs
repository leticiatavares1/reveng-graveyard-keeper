using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WsoOptimizedStagesData : WsoComponentDataBase
{
	[SerializeField]
	private List<WsoOptimizedStageEntry> stages = new List<WsoOptimizedStageEntry>();

	public IReadOnlyList<WsoOptimizedStageEntry> Stages => stages;

	public bool IsOptimized => stages.Count > 0;

	public WsoOptimizedStagesData()
	{
	}

	public WsoOptimizedStagesData(List<WsoOptimizedStageEntry> entries)
	{
		stages = entries ?? new List<WsoOptimizedStageEntry>();
	}
}
