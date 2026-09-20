using System;
using UnityEngine;

public static class VisualConsts
{
	public const int GRAPHICAL_PIXEL_SIZE = 2;

	public const float CAMERA_ANGLE = 0.6435011f;

	public const float Y_TO_Z = 0.75f;

	public const float Y_SCALE = 1.666667f;

	public const float Z_SCALE = 1.25f;

	public const float X_STEP = 0.01f;

	public const float Y_STEP = 0.01666667f;

	public const float Z_STEP = 0.0125f;

	public const int GRID_SIZE = 48;

	public const int Y_TERRAIN_GRID_SIZE = 36;

	public const float BOUNDS_EPSILON = 0.001f;

	public const float LAYER_Y_MICRO_OFFSET = 0.0001f;

	public const float FIGHT_DECAL_LAYER_Y_OFFSET = 0.002f;

	public const int FIGHT_BLOOD_LAYER_MIN = 10;

	public const int FIGHT_BLOOD_LAYER_MAX = 40;

	public const int FIGHT_GUTS_LAYER_MIN = 40;

	public const int FIGHT_GUTS_LAYER_MAX = 70;

	public const int FIGHT_GORE_LAYER_MIN = 70;

	public const int FIGHT_GORE_LAYER_MAX = 100;

	public static Vector3 XYZ_STEP => new Vector3(0.01f, 0.01666667f, 0.0125f);

	public static Vector3 XYZ_STEP_INV => new Vector3(100f, 59.99999f, 80f);

	public static Vector3 GetLayerOffset(int layer)
	{
		float num = 0.0001f * (float)layer;
		return new Vector3(0f, num, (0f - num) * 0.75f);
	}

	public static Vector3 GetFightDecalLayerOffset(int layer)
	{
		float num = 0.002f * (float)layer;
		return new Vector3(0f, num, (0f - num) * 0.75f);
	}

	public static Vector3 ProjectGroundPointToElevation(Vector3 groundPoint, float elevationY)
	{
		float num = elevationY - groundPoint.y;
		return new Vector3(groundPoint.x, elevationY, groundPoint.z - num * 0.75f);
	}

	public static Vector3 ProjectElevationPointToGround(Vector3 elevatedPoint, float groundY)
	{
		float num = elevatedPoint.y - groundY;
		return new Vector3(elevatedPoint.x, groundY, elevatedPoint.z + num * 0.75f);
	}

	public static Vector3 SnapGroundPointToReferenceGridPhase(Vector3 groundPoint, Vector3 refGroundPoint, float groundY)
	{
		Vector3 vector = groundPoint - refGroundPoint;
		Vector2 bUILD_GRID_SIZE_WORLD_UNIT = BuildConsts.BUILD_GRID_SIZE_WORLD_UNIT;
		return refGroundPoint + new Vector3(Mathf.Round(vector.x / bUILD_GRID_SIZE_WORLD_UNIT.x) * bUILD_GRID_SIZE_WORLD_UNIT.x, groundY, Mathf.Round(vector.z / bUILD_GRID_SIZE_WORLD_UNIT.y) * bUILD_GRID_SIZE_WORLD_UNIT.y);
	}

	public static Vector3 GetRoundedPosXZ(Vector3 pos, int pixelSize = 2, int step = 1)
	{
		float num = 0.01f * (float)pixelSize * (float)step;
		float num2 = 0.0125f * (float)pixelSize * (float)step;
		return new Vector3(Mathf.Round(pos.x / num) * num, pos.y, Mathf.Round(pos.z / num2) * num2);
	}

	public static Vector3 GetRoundedPosXZMPRounding(Vector3 pos, int pixelSize = 2, int step = 1, MidpointRounding midpointRounding = MidpointRounding.AwayFromZero)
	{
		float num = 0.01f * (float)pixelSize * (float)step;
		float num2 = 0.0125f * (float)pixelSize * (float)step;
		return new Vector3((float)Math.Round(pos.x / num, midpointRounding) * num, pos.y, (float)Math.Round(pos.z / num2, midpointRounding) * num2);
	}

	public static Vector3 GetRoundedPosXZ(Vector3 pos, Vector2Int step)
	{
		float num = 0.01f * (float)step.x * 2f;
		float num2 = 0.0125f * (float)step.y * 2f;
		return new Vector3(Mathf.Round(pos.x / num) * num, pos.y, Mathf.Round(pos.z / num2) * num2);
	}

	public static Vector3 GetRoundedPosY(Vector3 pos, int step = 1, int pixelSize = 2)
	{
		float num = 0.01666667f * (float)pixelSize * (float)step;
		pos.y = Mathf.Round(pos.y / num) * num;
		return pos;
	}

	public static Vector3 GetRoundedPosYMPRounding(Vector3 pos, int step = 1, int pixelSize = 2, MidpointRounding midpointRounding = MidpointRounding.AwayFromZero)
	{
		float num = 0.01666667f * (float)pixelSize * (float)step;
		pos.y = (float)Math.Round(pos.y / num, midpointRounding) * num;
		return pos;
	}

	public static Vector3 GetRoundedPosXYZ(Vector3 pos, int pixelSize = 2, int step = 1)
	{
		Vector3 roundedPosXZ = GetRoundedPosXZ(pos, pixelSize, step);
		return new Vector3(roundedPosXZ.x, GetRoundedPosY(pos, step, pixelSize).y, roundedPosXZ.z);
	}

	public static Vector3 GetRoundedPosXYZMPRounding(Vector3 pos, int pixelSize = 2, int step = 1, MidpointRounding midpointRounding = MidpointRounding.AwayFromZero)
	{
		Vector3 roundedPosXZMPRounding = GetRoundedPosXZMPRounding(pos, pixelSize, step, midpointRounding);
		return new Vector3(roundedPosXZMPRounding.x, GetRoundedPosYMPRounding(pos, step, pixelSize).y, roundedPosXZMPRounding.z);
	}

	public static Vector3 GetRoundedPosXYZCustomStepY(Vector3 pos, int pixelSize = 2, int stepXZ = 1, int stepY = 1)
	{
		Vector3 roundedPosXZ = GetRoundedPosXZ(pos, pixelSize, stepXZ);
		return new Vector3(roundedPosXZ.x, GetRoundedPosY(pos, stepY).y, roundedPosXZ.z);
	}

	public static Vector3 GetRoundedPosXYZCustomStepY(Vector3 pos, Vector2Int stepXZ, int stepY)
	{
		Vector3 roundedPosXZ = GetRoundedPosXZ(pos, stepXZ);
		return new Vector3(roundedPosXZ.x, GetRoundedPosY(pos, stepY).y, roundedPosXZ.z);
	}

	public static Bounds GetGreaterRoundedBoundsXZ(Bounds bounds, int step = 1)
	{
		Bounds result = bounds;
		result.max = GetRoundedPosXZ(result.max, 2, step);
		result.min = GetRoundedPosXZ(result.min, 2, step);
		float num = 0.02f * (float)step;
		float num2 = 0.025f * (float)step;
		if (result.min.x - bounds.min.x > 0.001f)
		{
			result.min += Vector3.left * num;
		}
		if (result.min.z - bounds.min.z > 0.001f)
		{
			result.min += Vector3.back * num2;
		}
		if (result.max.x - bounds.max.x < -0.001f)
		{
			result.max += Vector3.right * num;
		}
		if (result.max.z - bounds.max.z < -0.001f)
		{
			result.max += Vector3.forward * num2;
		}
		return result;
	}

	public static Bounds GetGreaterRoundedBoundsXZ(Bounds bounds, Vector2Int step)
	{
		Bounds result = bounds;
		result.max = GetRoundedPosXZ(result.max, step);
		result.min = GetRoundedPosXZ(result.min, step);
		float num = 0.02f * (float)step.x;
		float num2 = 0.025f * (float)step.y;
		if (result.min.x - bounds.min.x > 0.001f)
		{
			result.min += Vector3.left * num;
		}
		if (result.min.z - bounds.min.z > 0.001f)
		{
			result.min += Vector3.back * num2;
		}
		if (result.max.x - bounds.max.x < -0.001f)
		{
			result.max += Vector3.right * num;
		}
		if (result.max.z - bounds.max.z < -0.001f)
		{
			result.max += Vector3.forward * num2;
		}
		return result;
	}
}
