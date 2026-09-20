using System;

[Serializable]
public class RiverFloatingDropData
{
	public Item item;

	public SGuid wgoUniqueId;

	public string worldId;

	public int splineIndex;

	public float t;

	public float[] splineWorldLengths;

	public float flowSpeed;

	public float fallSpeed;

	public SGuid UniqueId
	{
		get
		{
			if (item == null)
			{
				return SGuid.Empty;
			}
			return item.UniqueId;
		}
	}

	public float GetCurrentSplineLength()
	{
		if (splineWorldLengths == null || splineIndex < 0 || splineIndex >= splineWorldLengths.Length)
		{
			return 1f;
		}
		float num = splineWorldLengths[splineIndex];
		if (!(num > 0.0001f))
		{
			return 1f;
		}
		return num;
	}

	public float GetCurrentSpeed()
	{
		if (splineIndex > 0)
		{
			return fallSpeed;
		}
		return flowSpeed;
	}

	public bool HasMoreSplinesAfterCurrent()
	{
		if (splineWorldLengths != null)
		{
			return splineIndex + 1 < splineWorldLengths.Length;
		}
		return false;
	}
}
