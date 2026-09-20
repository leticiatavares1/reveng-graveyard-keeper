public static class FightingWgoTarget
{
	public static bool IsBarricadeOrTower(ICombatEntity entity)
	{
		if (entity is Wgo wgo)
		{
			return IsBarricadeOrTower(wgo);
		}
		return false;
	}

	public static bool IsBarricadeOrTower(Wgo wgo)
	{
		if (wgo != null)
		{
			return IsBarricadeOrTower(wgo.Data);
		}
		return false;
	}

	public static bool IsBarricadeOrTower(WgoData data)
	{
		if (data?.Definition == null)
		{
			return false;
		}
		if (data.Definition.interactionType == WGODef.InteractionType.Barricade)
		{
			return true;
		}
		if (GameBalance.Me == null || string.IsNullOrEmpty(data.id))
		{
			return false;
		}
		if (!GameBalance.Me.HasWgoIdByGroup("barricades", data.id))
		{
			return GameBalance.Me.HasWgoIdByGroup("towers", data.id);
		}
		return true;
	}
}
