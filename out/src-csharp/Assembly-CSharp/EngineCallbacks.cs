public class EngineCallbacks : LazyEngineCallbacks
{
	public override void OnCurrentItemChanged(ItemDefinition item, ItemDefinition prev_item)
	{
		if (item == null)
		{
			item = ItemDefinition.none;
		}
		if (prev_item != null && prev_item.is_placable)
		{
			FloatingWorldGameObject.StopCurrentFloating();
		}
		if (item.is_placable)
		{
			FloatingWorldGameObject floatingWorldGameObject = FloatingWorldGameObject.CreateFloatingObject(Prefabs.me.test_place_obj_prefab);
			floatingWorldGameObject.wobj.SetObject(item.id);
			floatingWorldGameObject.UpdateObjSize();
			FloatingWorldGameObject.MoveCurrentFloatingObject(MainGame.me.player.transform.localPosition, is_global_pos: false, MainGame.me.player_char.direction);
		}
	}
}
