using System;
using System.Collections.Generic;
using UnityEngine;

public class SteppedRotationPreset
{
	protected static readonly Sector nullSector = new Sector(0f, 0f, Sector.RoundType.GetLower, Direction.None);

	private Sector currentSector = nullSector;

	private Sector currentSubSector = nullSector;

	private float angleAccumThreshold;

	private float angleThreshold = 10f;

	private float currentAngle;

	protected virtual List<Sector> PossibleSectors { get; } = new List<Sector>
	{
		new Sector(-180f, -90f),
		new Sector(-90f, 0f),
		new Sector(0f, 180f)
	};


	public float ComputeAngle(Vector2 direction)
	{
		return GetPossibleAngleByGiven(direction);
	}

	public Vector2 ComputeDirection(Vector2 direction)
	{
		float possibleAngleByGiven = GetPossibleAngleByGiven(direction);
		return new Vector2(Mathf.Cos(possibleAngleByGiven * (MathF.PI / 180f)), Mathf.Sin(possibleAngleByGiven * (MathF.PI / 180f)));
	}

	public Direction GetDirectionFromAngle(float angle)
	{
		if (angle.Equals(180f) || angle.Equals(-180f))
		{
			List<Sector> possibleSectors = PossibleSectors;
			return possibleSectors[possibleSectors.Count - 1].Direction;
		}
		for (int i = 0; i < PossibleSectors.Count; i++)
		{
			if (angle >= PossibleSectors[i].AngleL && angle < PossibleSectors[i].AngleG)
			{
				return PossibleSectors[i].Direction;
			}
		}
		return Direction.None;
	}

	public float ComputeAngleSmooth(Vector2 direction)
	{
		return ComputePossibleAnglesWithSmooth(direction);
	}

	public Vector2 ComputeDirectionSmooth(Vector2 direction)
	{
		float num = ComputePossibleAnglesWithSmooth(direction);
		return new Vector2(Mathf.Cos(num * (MathF.PI / 180f)), Mathf.Sin(num * (MathF.PI / 180f)));
	}

	public virtual float GetSteppedDirection(Direction direction)
	{
		return GetPossibleAngleByGiven(direction.ConvertToVector2XZ());
	}

	protected virtual float ComputePossibleAnglesWithSmooth(Vector2 direction)
	{
		float num = Vector2.SignedAngle(Vector2.right, direction);
		if (num.Equals(PossibleSectors[0].AngleL) || num.Equals(PossibleSectors[PossibleSectors.Count - 1].AngleG))
		{
			return currentAngle = num;
		}
		if (num >= currentSubSector.AngleL && num < currentSubSector.AngleG)
		{
			angleAccumThreshold = 0f;
		}
		else
		{
			float num2 = Mathf.Min(Mathf.Abs(num - currentSubSector.AngleL), Mathf.Abs(num - currentSubSector.AngleG));
			angleAccumThreshold += num2;
		}
		if (angleAccumThreshold >= angleThreshold)
		{
			angleAccumThreshold = 0f;
			return currentAngle = GetPossibleAngleByGiven(direction);
		}
		return currentAngle;
	}

	protected float GetPossibleAngleByGiven(Vector2 direction)
	{
		float num = Vector2.SignedAngle(Vector2.right, direction);
		foreach (Sector possibleSector in PossibleSectors)
		{
			if (num >= possibleSector.AngleL && num < possibleSector.AngleG)
			{
				currentSector = possibleSector;
				currentAngle = currentSector.GetClosestAngleByGiven(num);
				float num2 = (currentSector.AngleG + currentSector.AngleL) / 2f;
				currentSubSector = ((currentAngle == currentSector.AngleL) ? new Sector(currentSector.AngleL, num2) : new Sector(num2, currentSector.AngleG));
				return currentAngle;
			}
		}
		currentSector = PossibleSectors[0];
		return currentAngle = currentSector.AngleL;
	}
}
