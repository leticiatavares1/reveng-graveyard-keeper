using System;

[Serializable]
public class TargetInfo
{
	public ICombatEntity entity { get; }

	public LazyConsts.Fighting.TeamType Team { get; }

	public int LineId { get; set; }

	public int SectorId { get; set; }

	public bool IsPersistent { get; set; }

	public TargetInfo(ICombatEntity entity, LazyConsts.Fighting.TeamType team, int lineId = -1, int sectorId = -1, bool isPersistent = false)
	{
		this.entity = entity;
		Team = team;
		LineId = lineId;
		SectorId = sectorId;
		IsPersistent = isPersistent;
	}
}
