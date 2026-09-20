using UnityEngine;

public readonly struct BuildElevationArea
{
	public readonly Rect GroundRect;

	public readonly float ElevationY;

	public readonly float GroundY;

	public BuildElevationArea(Rect groundRect, float elevationY, float groundY)
	{
		GroundRect = groundRect;
		ElevationY = elevationY;
		GroundY = groundY;
	}
}
