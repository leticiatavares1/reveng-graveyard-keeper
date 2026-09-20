using System;

[Serializable]
public class CPA_SetSpawnerActive : CapturePointAction
{
	public bool isActive;

	public int lineId;

	public string spawnZoneName;

	public override void Execute(LazyConsts.Fighting.TeamType teamType, FightingLevel level)
	{
		if (base.teamType == teamType)
		{
			level.SetSpawnerActive(lineId, spawnZoneName, isActive);
		}
	}
}
