using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FightingLevelPreset", menuName = "GK2/Fighting/FightingLevelPreset")]
public class FightingLevelPreset : ScriptableObject
{
	[Serializable]
	public class FightingLineData
	{
		public bool isEnabled = true;

		public int maxSpawnedCountAtOnce = 2;

		[SerializeReference]
		public List<FightingPhaseData> phases = new List<FightingPhaseData>();

		public float TotalTime => phases?.Sum((Func<FightingPhaseData, float>)((FightingPhaseData phase) => phase.duration)) ?? 0f;

		public int TotalNeededToSpawn => phases?.Sum((FightingPhaseData phase) => (phase is FightingPhaseSpawnEnemiesData) ? (phase as FightingPhaseSpawnEnemiesData).TotalEnemiesToSpawn : 0) ?? 0;

		public float SecondsAtEndOfLastSpawnEnemyPhase()
		{
			if (phases == null || phases.Count == 0)
			{
				return -1f;
			}
			float num = 0f;
			float result = -1f;
			foreach (FightingPhaseData phase in phases)
			{
				num += (float)phase.duration;
				if (phase is FightingPhaseSpawnEnemiesData)
				{
					result = num;
				}
			}
			return result;
		}
	}

	[Header("Lines")]
	public List<FightingLineData> lines = new List<FightingLineData>();
}
