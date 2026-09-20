using UnityEngine;

public static class BuildConsts
{
	public static readonly Vector2Int BUILD_GRID_DIVIDER = new Vector2Int(3, 4);

	public static readonly Vector2Int BUILD_GRID_SIZE = new Vector2Int(48 / BUILD_GRID_DIVIDER.x, 48 / BUILD_GRID_DIVIDER.y);

	public static readonly Vector2 BUILD_GRID_SIZE_WORLD_UNIT = Vector2.Scale((Vector2)BUILD_GRID_SIZE, new Vector2(0.01f, 0.0125f)) * 2f;

	public const float CORRECTION_BUILD_GRID_SCALE_Z = 0.6f;

	public static readonly Vector2 CELL_SIZE = BUILD_GRID_SIZE * new Vector2(0.01f, 0.0125f) * 2f;

	public static readonly Vector3 CASTING_BOX_HALF_EXTENTS = new Vector3(CELL_SIZE.x, 8f, CELL_SIZE.y) / 2f - VisualConsts.XYZ_STEP;
}
