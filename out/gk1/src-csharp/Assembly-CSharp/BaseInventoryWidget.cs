using UnityEngine;

public class BaseInventoryWidget : MonoBehaviour
{
	public enum InventoryType
	{
		None,
		Custom,
		Main
	}

	public GameObject header_container;

	public UIWidget table_widget;

	public UIWidget table_back_widget;

	public UILabel header_label;

	protected Inventory inventory;

	protected bool initialized;

	protected bool for_gamepad;

	protected InventoryType type;

	private int _opened_at_frame;

	public Item inventory_data
	{
		get
		{
			if (inventory == null)
			{
				return null;
			}
			return inventory.data;
		}
	}

	public bool just_opened => Time.frameCount - _opened_at_frame <= 1;

	public virtual void Init()
	{
	}

	public virtual void Open(Inventory inventory, bool for_gamepad, InventoryType type = InventoryType.None)
	{
		if (!initialized)
		{
			Init();
		}
		_opened_at_frame = Time.frameCount;
		this.inventory = inventory;
		this.for_gamepad = for_gamepad;
		this.type = type;
		if (header_label != null)
		{
			header_label.text = inventory.name;
		}
	}

	public virtual void Redraw()
	{
	}

	public virtual void SetCustomNavigationTarget(BaseInventoryWidget widget, Direction direction)
	{
	}

	public virtual GamepadNavigationItem GetFirstNavigationItem(Direction dir)
	{
		return GetComponentInChildren<GamepadNavigationItem>();
	}

	public void SetMain()
	{
		type = InventoryType.Main;
	}

	public bool IsMain()
	{
		return type == InventoryType.Main;
	}

	public bool IsCustom()
	{
		return type == InventoryType.Custom;
	}
}
