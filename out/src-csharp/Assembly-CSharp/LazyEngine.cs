using UnityEngine;

public static class LazyEngine
{
	private static LazyEngineCallbacks _callbacks = null;

	private static string _cur_item = "";

	private static Transform _world_root;

	public const int LAYER_INTERACTION_COLLIDERS = 8;

	public static LazyEngineCallbacks callbacks => _callbacks;

	public static Transform world_root => _world_root;

	public static void Init(Transform world_root, LazyEngineCallbacks custom_callbacks)
	{
		_callbacks = custom_callbacks;
		_world_root = world_root;
	}

	public static void _OnItemChanged(ItemDefinition item)
	{
		string text = ((item == null) ? "" : item.id);
		if (!(_cur_item == text))
		{
			ItemDefinition prev_item = ((_cur_item == "") ? null : GameBalance.me.GetDataOrNull<ItemDefinition>(_cur_item));
			_cur_item = text;
			callbacks.OnCurrentItemChanged(item, prev_item);
		}
	}

	public static void CancelCurrentItem()
	{
		_cur_item = "";
	}
}
