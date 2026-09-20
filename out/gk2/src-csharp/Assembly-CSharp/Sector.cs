public struct Sector
{
	public enum RoundType
	{
		GetLower,
		GetGreater
	}

	public float AngleL { get; private set; }

	public float AngleG { get; private set; }

	public RoundType RoundApproach { get; private set; }

	public Direction Direction { get; private set; }

	public Sector(float angleL, float angleG, RoundType roundType, Direction direction)
	{
		AngleL = angleL;
		AngleG = angleG;
		RoundApproach = roundType;
		Direction = direction;
	}

	public Sector(float angleL, float angleG)
	{
		AngleL = angleL;
		AngleG = angleG;
		RoundApproach = RoundType.GetLower;
		Direction = Direction.None;
	}

	public float GetClosestAngleByGiven(float directionAngle)
	{
		if (((AngleG + AngleL) / 2f).EqualsTo(directionAngle))
		{
			switch (RoundApproach)
			{
			case RoundType.GetLower:
				return AngleL;
			case RoundType.GetGreater:
				return AngleG;
			}
		}
		if (!(AngleG - directionAngle <= directionAngle - AngleL))
		{
			return AngleL;
		}
		return AngleG;
	}
}
