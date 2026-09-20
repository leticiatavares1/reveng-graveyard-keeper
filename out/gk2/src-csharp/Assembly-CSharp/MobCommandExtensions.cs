using UnityEngine;

public static class MobCommandExtensions
{
	public static MobCommand ToTarget(this MobCommand mobCommand, ICombatEntity entity)
	{
		mobCommand.TargetEntity = entity;
		return mobCommand;
	}

	public static MobCommand ToPosition(this MobCommand mobCommand, Vector3 position)
	{
		mobCommand.customPosition = position;
		return mobCommand;
	}
}
