using UnityEngine;

public class BuildingSubZoneConfiguration : ScriptableObject
{
	public float z_build_grid_sorting = 1900f;

	public string build_cell_sorting_layer = "on_ground_3";

	public bool is_cell_ground_object = true;

	public bool override_grid_cell_sorting_order;

	public bool sort_floating_over_everything;

	public int grid_cell_sorting_order;
}
