using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class FightingTargetsDatabase
{
	private readonly List<TargetInfo> allTargets = new List<TargetInfo>();

	private readonly Dictionary<LazyConsts.Fighting.TeamType, List<ICombatEntity>> cachedTargetsByTeam = new Dictionary<LazyConsts.Fighting.TeamType, List<ICombatEntity>>();

	public IReadOnlyList<TargetInfo> AllTargets => allTargets;

	public event Action<TargetInfo> OnTargetAdded;

	public event Action<TargetInfo> OnTargetRemoved;

	public FightingTargetsDatabase()
	{
		foreach (LazyConsts.Fighting.TeamType value in Enum.GetValues(typeof(LazyConsts.Fighting.TeamType)))
		{
			cachedTargetsByTeam[value] = new List<ICombatEntity>();
		}
	}

	public IReadOnlyList<ICombatEntity> GetTargetsByTeam(LazyConsts.Fighting.TeamType team)
	{
		if (!cachedTargetsByTeam.TryGetValue(team, out var value))
		{
			return Array.Empty<ICombatEntity>();
		}
		return value;
	}

	public int GetTargetCountByTeam(LazyConsts.Fighting.TeamType team)
	{
		if (!cachedTargetsByTeam.TryGetValue(team, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	public TargetInfo GetTargetInfo(ICombatEntity entity)
	{
		return allTargets.FirstOrDefault((TargetInfo t) => t.entity == entity);
	}

	public void Register(ICombatEntity entity, int lineId = -1, int sectorId = -1, bool updateInfoIfExists = true, bool isPersistent = false)
	{
		TargetInfo targetInfo = allTargets.FirstOrDefault((TargetInfo t) => t.entity == entity);
		if (targetInfo != null && updateInfoIfExists)
		{
			targetInfo.LineId = lineId;
			targetInfo.SectorId = sectorId;
			return;
		}
		LazyConsts.Fighting.TeamType teamType = entity.TeamType;
		targetInfo = new TargetInfo(entity, teamType, lineId, sectorId, isPersistent);
		allTargets.Add(targetInfo);
		if (cachedTargetsByTeam.TryGetValue(teamType, out var value))
		{
			value.Add(entity);
		}
		this.OnTargetAdded?.Invoke(targetInfo);
	}

	public void Unregister(ICombatEntity entity)
	{
		TargetInfo targetInfo = GetTargetInfo(entity);
		if (targetInfo != null)
		{
			allTargets.Remove(targetInfo);
			if (cachedTargetsByTeam.TryGetValue(targetInfo.Team, out var value))
			{
				value.Remove(entity);
			}
			this.OnTargetRemoved?.Invoke(targetInfo);
		}
	}

	public void Unregister(ICombatEntity entity, int lineId, int sectorId)
	{
		TargetInfo targetInfo = GetTargetInfo(entity);
		if (targetInfo != null && targetInfo.LineId == lineId && targetInfo.SectorId == sectorId)
		{
			allTargets.Remove(targetInfo);
			if (cachedTargetsByTeam.TryGetValue(targetInfo.Team, out var value))
			{
				value.Remove(entity);
			}
			this.OnTargetRemoved?.Invoke(targetInfo);
		}
	}

	public bool IsTargetOnLine(ICombatEntity entity, int lineId)
	{
		TargetInfo targetInfo = GetTargetInfo(entity);
		if (targetInfo != null)
		{
			return targetInfo.LineId == lineId;
		}
		return false;
	}

	public int GetEnemyCountOnLine(int lineId)
	{
		return allTargets.Count((TargetInfo t) => t.Team == LazyConsts.Fighting.TeamType.WildZombie && t.LineId == lineId);
	}

	public IEnumerable<ICombatEntity> GetTargets(int lineId = -1, int sectorId = -1)
	{
		return from t in allTargets
			where (lineId == -1 || t.LineId == lineId) && (sectorId == -1 || t.SectorId == sectorId)
			select t.entity;
	}

	public static bool IsCombatEntityAlive(ICombatEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity is UnityEngine.Object @object && @object == null)
		{
			return false;
		}
		return true;
	}

	public void RemoveEntry(TargetInfo targetInfo)
	{
		if (targetInfo != null && allTargets.Remove(targetInfo))
		{
			if (cachedTargetsByTeam.TryGetValue(targetInfo.Team, out var value))
			{
				value.Remove(targetInfo.entity);
			}
			this.OnTargetRemoved?.Invoke(targetInfo);
		}
	}

	public bool PruneDeadTargets(Action<ICombatEntity> onRemoveCallback)
	{
		bool result = false;
		for (int num = allTargets.Count - 1; num >= 0; num--)
		{
			TargetInfo targetInfo = allTargets[num];
			ICombatEntity entity = targetInfo.entity;
			bool flag = !IsCombatEntityAlive(entity);
			bool flag2 = IsCombatEntityAlive(entity) && entity.CombatEntityHpComponent.Hp <= 0 && entity.CombatEntityHpComponent.WasDamagedAtLeastOnce;
			if (flag || flag2)
			{
				if (flag)
				{
					Debug.Log($"Pruned orphan target: {targetInfo.Team}");
				}
				else
				{
					string text = ((entity is Wgo wgo) ? (" WgoId: " + wgo.Id) : string.Empty);
					Debug.Log($"Pruned Dead Target: {entity.CombatEntityUID}" + text);
				}
				onRemoveCallback?.Invoke(entity);
				cachedTargetsByTeam[targetInfo.Team].Remove(targetInfo.entity);
				allTargets.RemoveAt(num);
				result = true;
				this.OnTargetRemoved?.Invoke(targetInfo);
			}
		}
		return result;
	}

	public void Clear()
	{
		allTargets.Clear();
		foreach (List<ICombatEntity> value in cachedTargetsByTeam.Values)
		{
			value.Clear();
		}
	}
}
