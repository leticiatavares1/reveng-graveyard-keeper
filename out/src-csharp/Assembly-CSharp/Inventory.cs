public class Inventory
{
	public class VendorTierInfo
	{
		public int tier_1;

		public int tier_2;

		public float progress;

		public bool progressbar_visible;
	}

	private Item _data;

	private string _name;

	private string _preset;

	private bool _name_is_set;

	private readonly bool _is_player;

	public readonly string _obj_id;

	public bool is_locked;

	public VendorTierInfo vendor_tier_info;

	public Item data => _data;

	public int size => (int)data.GetParam("inventory_size");

	public string preset => _preset;

	public string name
	{
		get
		{
			if (!_name_is_set)
			{
				if (_is_player)
				{
					_name = "player_inventory";
				}
				else
				{
					_name = _obj_id + "_inventory";
				}
				_name_is_set = true;
			}
			return GJL.L(_name);
		}
	}

	public Inventory(Item data, string name = "", string preset = "")
	{
		_data = data;
		_name = name;
		_preset = preset;
		_name_is_set = true;
		_obj_id = string.Empty;
	}

	public Inventory(WorldGameObject obj)
	{
		_data = obj.data;
		_name_is_set = false;
		_obj_id = obj.obj_id;
		_is_player = obj.is_player;
		_preset = (obj.is_player ? string.Empty : obj.obj_def.inventory_preset);
	}

	public void ClearName()
	{
		_name = string.Empty;
		_name_is_set = true;
	}

	public bool IsTavernPalette()
	{
		if (!(_obj_id == "tavern_cellar_rack"))
		{
			return false;
		}
		return true;
	}
}
