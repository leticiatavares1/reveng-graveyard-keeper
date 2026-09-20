using System;
using System.Collections.Generic;

[Serializable]
public class ObjectCraftDefinition : CraftDefinition
{
	public enum BuildType
	{
		Put,
		Remove,
		None
	}

	public string out_obj = "";

	public BuildType build_type;

	public List<string> builder_ids = new List<string>();

	public List<string> locked_builders_ids = new List<string>();

	public bool enabled = true;

	public string sub_zone_id = "";

	public bool is_remove_without_hp_work;

	public bool is_destroy_worker_on_remove;

	public bool wait_script_callback;

	public bool has_variations;
}
