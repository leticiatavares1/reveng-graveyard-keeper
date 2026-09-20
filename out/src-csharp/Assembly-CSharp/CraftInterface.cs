using System.Collections.Generic;

public interface CraftInterface
{
	bool CanCraft(CraftDefinition craft, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null);

	bool OnCraft(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null, WorldGameObject other_obj_override = null);

	void OnRightClick();
}
